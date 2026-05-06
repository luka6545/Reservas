using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.Models;

namespace ReservasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BebidasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BebidasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Bebidas (Público)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bebida>>> GetBebidas()
        {
            // Include carga la información de la categoría asociada para que no salga en "null"
            return await _context.Bebidas.Include(b => b.Categoria).ToListAsync();
        }

        // POST: api/Bebidas (Solo Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Bebida>> PostBebida(Bebida bebida)
        {
            // Validamos que la categoría exista antes de agregar la bebida
            var categoriaExiste = await _context.CategoriasBebidas.AnyAsync(c => c.Id == bebida.CategoriaId);
            if (!categoriaExiste) return BadRequest("La categoría especificada no existe.");

            _context.Bebidas.Add(bebida);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBebidas), new { id = bebida.Id }, bebida);
        }

        // DELETE: api/Bebidas/5 (Solo Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBebida(int id)
        {
            var bebida = await _context.Bebidas.FindAsync(id);
            if (bebida == null) return NotFound();

            _context.Bebidas.Remove(bebida);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}