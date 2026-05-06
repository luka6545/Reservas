using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaWeb.Models;
using ReservaWeb.Services;

namespace ReservaWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ZonasController : Controller
    {
        private readonly ApiService _apiService;

        public ZonasController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Helper para obtener el token
        private string GetToken() => Request.Cookies["TokenJWT"] ?? "";

        // LECTURA (Listar)
        public async Task<IActionResult> Index()
        {
            var zonas = await _apiService.GetZonasAsync(GetToken());
            return View(zonas);
        }

        // CREAR (Vista)
        [HttpGet]
        public IActionResult Create() => View(new ZonaViewModel());

        // CREAR (Guardar)
        [HttpPost]
        public async Task<IActionResult> Create(ZonaViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _apiService.CreateZonaAsync(model, GetToken());
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var zona = await _apiService.GetZonaByIdAsync(id, GetToken());

            if (zona == null)
            {
                // Quitamos el NotFound() y ponemos este mensaje para descubrir al culpable:
                return Content($"Error 404: La Web intentó buscar la zona {id}, pero la API de Lazzo devolvió NULL. Verifica en Swagger si el endpoint GET /api/Zonas/{id} existe y funciona.");
            }

            return View(zona);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ZonaViewModel model)
        {
            model.Id = id;

            if (ModelState.IsValid)
            {
                var token = Request.Cookies["TokenJWT"];
                var exito = await _apiService.UpdateZonaAsync(id, model, token ?? "");

                if (exito)
                {
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Error = "La API rechazó la actualización. Verifica que los datos sean correctos.";
            }

            return View(model);
        }


        // ELIMINAR (Acción directa desde el botón)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteZonaAsync(id, GetToken());
            return RedirectToAction(nameof(Index));
        }
    }
}