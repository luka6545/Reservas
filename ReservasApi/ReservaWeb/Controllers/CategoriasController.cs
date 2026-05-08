using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservaWeb.Models;
using ReservaWeb.Services;

namespace ReservaWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriasController : Controller
    {
        private readonly ApiService _apiService;

        public CategoriasController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private string GetToken() => Request.Cookies["TokenJWT"] ?? "";

        public async Task<IActionResult> Index()
        {
            var categorias = await _apiService.GetCategoriasAsync();
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Create() => View(new CategoriaViewModel());

        [HttpPost]
        public async Task<IActionResult> Create(CategoriaViewModel model)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var exito = await _apiService.CreateCategoriaAsync(model, GetToken());
                if (exito) return RedirectToAction(nameof(Index));
                ViewBag.Error = "Error al crear la categoría en la API.";
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _apiService.GetCategoriaByIdAsync(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoriaViewModel model)
        {
            model.Id = id;
            if (ModelState.IsValid)
            {
                var exito = await _apiService.UpdateCategoriaAsync(id, model, GetToken());
                if (exito) return RedirectToAction(nameof(Index));
                ViewBag.Error = "Error al actualizar.";
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteCategoriaAsync(id, GetToken());
            return RedirectToAction(nameof(Index));
        }

    }
}