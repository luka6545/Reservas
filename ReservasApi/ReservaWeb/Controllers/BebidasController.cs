using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservaWeb.Models;
using ReservaWeb.Services;

namespace ReservaWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BebidasController : Controller
    {
        private readonly ApiService _apiService;

        public BebidasController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private string GetToken() => Request.Cookies["TokenJWT"] ?? "";

        // LECTURA
        public async Task<IActionResult> Index()
        {
            var bebidas = await _apiService.GetBebidasAsync();
            return View(bebidas);
        }

        // CREAR (Vista)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Cargamos las categorías para que la vista pueda pintar un <select>
            var categorias = await _apiService.GetCategoriasAsync();
            ViewBag.Categorias = new SelectList(categorias, "Id", "NombreCategoria");

            return View(new BebidaViewModel());
        }

        // CREAR (Guardar)
        [HttpPost]
        public async Task<IActionResult> Create(BebidaViewModel model)
        {
            ModelState.Remove("Id");
            ModelState.Remove("Categoria");

            // 1. Verificamos si la validación interna de la página falló (Ej: El Precio)
            if (!ModelState.IsValid)
            {
                // Esto extraerá el error exacto y lo mandará a la vista
                var errores = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ViewBag.Error = "Error en el formulario web: " + errores;
            }
            else
            {
                // 2. Si todo está bien, lo enviamos a la API
                var exito = await _apiService.CreateBebidaAsync(model, GetToken());
                if (exito)
                {
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Error = "La API de Lazzo rechazó la bebida. Verifica que el servidor esté corriendo.";
            }

            // Si hubo error, recargamos la lista y volvemos a mostrar la vista
            var categorias = await _apiService.GetCategoriasAsync();
            ViewBag.Categorias = new SelectList(categorias, "Id", "NombreCategoria");
            return View(model);
        }

        // EDITAR (Vista)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var bebida = await _apiService.GetBebidaByIdAsync(id);
            if (bebida == null) return NotFound();

            var categorias = await _apiService.GetCategoriasAsync();
            // Le pasamos bebida.CategoriaId al SelectList para que se seleccione por defecto
            ViewBag.Categorias = new SelectList(categorias, "Id", "NombreCategoria", bebida.CategoriaId);

            return View(bebida);
        }

        // EDITAR (Guardar)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, BebidaViewModel model)
        {
            model.Id = id;
            ModelState.Remove("Categoria");

            if (ModelState.IsValid)
            {
                var exito = await _apiService.UpdateBebidaAsync(id, model, GetToken());
                if (exito) return RedirectToAction(nameof(Index));

                ViewBag.Error = "Error al actualizar.";
            }

            var categorias = await _apiService.GetCategoriasAsync();
            ViewBag.Categorias = new SelectList(categorias, "Id", "NombreCategoria", model.CategoriaId);
            return View(model);
        }

        // ELIMINAR
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteBebidaAsync(id, GetToken());
            return RedirectToAction(nameof(Index));
        }
    }
}