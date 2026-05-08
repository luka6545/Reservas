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
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Bebida>> PostBebida(Bebida bebida)
        {
            var categoriaExiste = await _context.CategoriasBebidas.AnyAsync(c => c.Id == bebida.CategoriaId);
            if (!categoriaExiste) return BadRequest("La categoría especificada no existe.");

            // TRUCO DE SEGURIDAD: Desvinculamos el objeto Categoría para que EF Core 
            // no intente crear una categoría "fantasma" por accidente.
            bebida.Categoria = null;

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
        // 1. EL MÉTODO QUE FALTA: Traer UNA sola bebida por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Bebida>> GetBebida(int id)
        {
            // Usamos Include para que al editar también veamos la categoría
            var bebida = await _context.Bebidas
                .Include(b => b.Categoria)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bebida == null)
            {
                return NotFound();
            }

            return bebida;
        }

        // 2. EL MÉTODO PUT ACTUALIZADO (Para asegurar que guarde)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutBebida(int id, Bebida bebida)
        {
            if (id != bebida.Id) return BadRequest("Los IDs no coinciden");

            var bebidaDb = await _context.Bebidas.FindAsync(id);
            if (bebidaDb == null) return NotFound();

            // Sincronizamos los campos manualmente
            bebidaDb.Nombre = bebida.Nombre;
            bebidaDb.Precio = bebida.Precio;
            bebidaDb.Stock = bebida.Stock;
            bebidaDb.CategoriaId = bebida.CategoriaId;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}