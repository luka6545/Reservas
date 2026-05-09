using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservaWeb.Models;
using ReservaWeb.Services;
using System.IdentityModel.Tokens.Jwt;

namespace ReservaWeb.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApiService _apiService;

        public ReservasController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private string GetToken() => Request.Cookies["TokenJWT"] ?? "";

        private int GetUserIdFromToken()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token)) return 0;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                
                var idClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == "Id" ||
                    c.Type == "id" ||
                    c.Type.EndsWith("nameidentifier"));

                return idClaim != null ? int.Parse(idClaim.Value) : 0;
            }
            catch
            {
                return 0;
            }
        }
        public async Task<IActionResult> Index()
        {
            var reservas = await _apiService.GetReservasAsync(GetToken());

            if (!User.IsInRole("Admin"))
            {
                var userId = GetUserIdFromToken();
                reservas = reservas.Where(r => r.UsuarioId == userId).ToList();

                ViewBag.TienePendiente = reservas.Any(r => r.Estado == "Pendiente");
                ViewBag.TieneAprobada = reservas.Any(r => r.Estado == "Aprobada");
            }

            return View(reservas);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            var userId = GetUserIdFromToken();
            var reservas = await _apiService.GetReservasAsync(GetToken());
            var misReservas = reservas.Where(r => r.UsuarioId == userId).ToList();

            if (misReservas.Any(r => r.Estado == "Pendiente"))
            {
                TempData["Error"] = "No puedes crear una nueva reserva mientras tengas una en estado Pendiente. Espera a que sea aprobada.";
                return RedirectToAction(nameof(Index));
            }

            var zonas = await _apiService.GetZonasAsync(GetToken());
            ViewBag.Zonas = new SelectList(zonas, "Id", "NombreZona");

            var fechaSugerida = DateTime.Today.AddHours(20);
            if (DateTime.Now.Hour >= 20) fechaSugerida = DateTime.Today.AddDays(1).AddHours(20);

            return View(new ReservaViewModel
            {
                UsuarioId = userId,
                FechaHora = fechaSugerida
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReservaViewModel model)
        {
            ModelState.Remove("NombreZona");
            ModelState.Remove("NombreUsuario");

            // 1. Validar la hora (A partir de las 8 PM)
            if (model.FechaHora.Hour < 20)
            {
                ModelState.AddModelError("FechaHora", "Las reservas solo están disponibles a partir de las 20:00 (8:00 PM).");
            }

            if (model.UsuarioId == 0)
            {
                model.UsuarioId = GetUserIdFromToken();
                if (model.UsuarioId == 0) ModelState.AddModelError("UsuarioId", "Error de sesión. Por favor, vuelve a iniciar sesión.");
            }

            // 2. REGLA DE ORO: Validar que la mesa no esté ya ocupada ese día
            var todasLasReservas = await _apiService.GetReservasAsync(GetToken());

            bool zonaOcupada = todasLasReservas.Any(r =>
                r.ZonaLoungeId == model.ZonaLoungeId && // Es la misma zona que intentan reservar
                r.FechaHora.Date == model.FechaHora.Date && // Es el mismo día (comparamos solo fecha, no la hora exacta)
                r.Estado != "Cancelada" // Ignoramos si la reserva previa fue cancelada
            );

            if (zonaOcupada)
            {
                ModelState.AddModelError("ZonaLoungeId", "Esta zona ya está reservada para esta fecha. Por favor, elige otra zona u otro día.");
            }

            // 3. Procesar si todo está perfecto
            if (ModelState.IsValid)
            {
                var exito = await _apiService.CreateReservaAsync(model, GetToken());
                if (exito) return RedirectToAction(nameof(Index));

                ViewBag.Error = "La API rechazó la reserva. Verifica que el servidor esté activo.";
            }
            else
            {
                // Si falla alguna validación, mostramos los errores
                var errores = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewBag.Error = errores;
            }

            // Recargar la lista de zonas si hay que devolver a la vista
            var zonas = await _apiService.GetZonasAsync(GetToken());
            ViewBag.Zonas = new SelectList(zonas, "Id", "NombreZona", model.ZonaLoungeId);
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var token = GetToken();
            var reserva = await _apiService.GetReservaByIdAsync(id, token);
            if (reserva == null) return NotFound();

            // Traemos todos los pedidos de la noche
            var todosLosPedidos = await _apiService.GetHistorialPedidosAsync(id, token);

            // Separamos la cuenta actual (abierta) de los recibos ya pagados
            ViewBag.CuentaAbierta = todosLosPedidos.FirstOrDefault(p => p.Estado != "Cobrado");
            ViewBag.HistorialPagado = todosLosPedidos.Where(p => p.Estado == "Cobrado").ToList();

            return View(reserva);
        }
        // POST: El Admin aprueba la reserva
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Aprobar(int id)
        {
            // 1. Buscamos la reserva actual
            var reserva = await _apiService.GetReservaByIdAsync(id, GetToken());
            if (reserva != null)
            {
                // 2. Le cambiamos el estado
                reserva.Estado = "Aprobada";

                // 3. La mandamos a la API para que guarde el cambio
                await _apiService.UpdateEstadoReservaAsync(id, reserva, GetToken());
            }

            // Recargamos la tabla
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var token = GetToken();
            var reserva = await _apiService.GetReservaByIdAsync(id, token);

            if (reserva == null) return NotFound();

            // SEGURIDAD: Si no es Admin, verificar que la reserva le pertenezca
            var userId = GetUserIdFromToken();
            if (!User.IsInRole("Admin") && reserva.UsuarioId != userId)
            {
                return Forbid(); // No puede cancelar reservas de otros
            }

            // Cambiamos el estado y enviamos a la API
            reserva.Estado = "Cancelada";
            var exito = await _apiService.UpdateEstadoReservaAsync(id, reserva, token);

            if (!exito)
            {
                TempData["Error"] = "No se pudo cancelar la reserva en el servidor.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cobrar(int pedidoId, int reservaId)
        {
            await _apiService.CobrarPedidoAsync(pedidoId, GetToken());
            return RedirectToAction(nameof(Details), new { id = reservaId });
        }
    }
}