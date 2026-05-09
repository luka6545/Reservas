using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaWeb.Models;
using ReservaWeb.Services;

namespace ReservaWeb.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApiService _apiService;

        public PedidosController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private string GetToken() => Request.Cookies["TokenJWT"] ?? "";

        // GET: Muestra el Menú y la cuenta actual de la mesa
        [HttpGet]
        public async Task<IActionResult> Menu(int reservaId)
        {
            var token = GetToken();
            var reserva = await _apiService.GetReservaByIdAsync(reservaId, token);

            // Validamos que nadie intente pedir si la mesa no está aprobada
            if (reserva == null || reserva.Estado != "Aprobada")
            {
                TempData["Error"] = "Solo puedes hacer pedidos si la reserva está Aprobada.";
                return RedirectToAction("Index", "Reservas");
            }

            // Buscamos si ya tiene una cuenta abierta
            var cuentaActual = await _apiService.GetPedidoPorReservaAsync(reservaId, token);
            ViewBag.CuentaActual = cuentaActual;
            ViewBag.ReservaId = reservaId;

            // Traemos el menú y filtramos para no mostrar lo que no tiene stock
            var bebidas = await _apiService.GetBebidasAsync();
            var bebidasDisponibles = bebidas.Where(b => b.Stock > 0).ToList();

            return View(bebidasDisponibles);
        }

        // POST: Recibe las cantidades que puso el cliente y las envía a la API
        [HttpPost]
        public async Task<IActionResult> EnviarPedido(int reservaId, List<ItemPedidoViewModel> Items)
        {
            var token = GetToken();

            // Filtramos las bebidas que dejó en 0
            var carrito = Items.Where(i => i.Cantidad > 0).ToList();

            if (!carrito.Any())
            {
                TempData["Error"] = "No seleccionaste ninguna bebida. Añade una cantidad mayor a 0.";
                return RedirectToAction(nameof(Menu), new { reservaId = reservaId });
            }

            var nuevoPedido = new CrearPedidoViewModel
            {
                ReservaId = reservaId,
                Items = carrito
            };

            var exito = await _apiService.CrearPedidoAsync(nuevoPedido, token);

            if (exito)
            {
                TempData["Exito"] = "¡Bebidas agregadas a tu mesa!";
            }
            else
            {
                TempData["Error"] = "Hubo un error al procesar el pedido. Verifica que no hayas excedido el stock disponible.";
            }

            // Recargamos la misma página para que vea su cuenta actualizada
            return RedirectToAction(nameof(Menu), new { reservaId = reservaId });
        }
    }
}