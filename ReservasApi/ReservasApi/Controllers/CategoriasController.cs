using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.Models;

namespace ReservasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorias (Público)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaBebida>>> GetCategorias()
        {
            return await _context.CategoriasBebidas.ToListAsync();
        }

        // POST: api/Categorias (Solo Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoriaBebida>> PostCategoria(CategoriaBebida categoria)
        {
            _context.CategoriasBebidas.Add(categoria);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategorias), new { id = categoria.Id }, categoria);
        }

        // PUT y DELETE omitidos por brevedad, pero la estructura es idéntica a ZonasController
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.CategoriasBebidas.FindAsync(id);
            if (categoria == null) return NotFound();

            _context.CategoriasBebidas.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // 1. EL MÉTODO QUE FALTABA: Traer UNA sola categoría para poder editarla
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaBebida>> GetCategoria(int id)
        {
            var categoria = await _context.CategoriasBebidas.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return categoria;
        }

        // 2. EL MÉTODO PARA ACTUALIZAR (Para asegurarnos de que guarde bien)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategoria(int id, CategoriaBebida categoria)
        {
            if (id != categoria.Id) return BadRequest("Los IDs no coinciden");

            var categoriaDb = await _context.CategoriasBebidas.FindAsync(id);
            if (categoriaDb == null) return NotFound();

            // Actualizamos los campos manualmente como hicimos en Zonas
            categoriaDb.NombreCategoria = categoria.NombreCategoria;
            categoriaDb.Descripcion = categoria.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}