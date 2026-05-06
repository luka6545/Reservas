using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.Models;

namespace ReservasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZonasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ZonasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Trae TODAS las zonas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ZonaLounge>>> GetZonas()
        {
            return await _context.ZonasLounge.ToListAsync();
        }

        // 2. GET: Trae UNA SOLA zona por su ID (¡ESTE ERA EL QUE FALTABA!)
        [HttpGet("{id}")]
        public async Task<ActionResult<ZonaLounge>> GetZona(int id)
        {
            var zona = await _context.ZonasLounge.FindAsync(id);

            if (zona == null)
            {
                return NotFound();
            }

            return zona;
        }

        // 3. POST: Crear zona
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ZonaLounge>> PostZona(ZonaLounge zona)
        {
            _context.ZonasLounge.Add(zona);
            await _context.SaveChangesAsync();

            // Ajuste aquí: Ahora apunta al nuevo método GetZona
            return CreatedAtAction(nameof(GetZona), new { id = zona.Id }, zona);
        }

        // 4. PUT: Editar zona
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutZona(int id, ZonaLounge zona)
        {
            if (id != zona.Id) return BadRequest("Los IDs no coinciden");

            var zonaDb = await _context.ZonasLounge.FindAsync(id);
            if (zonaDb == null) return NotFound("La zona no existe en la BD.");

            // Actualizamos con los nombres reales
            zonaDb.NombreZona = zona.NombreZona;
            zonaDb.CapacidadMax = zona.CapacidadMax;
            zonaDb.PrecioMinimo = zona.PrecioMinimo; // ¡AQUÍ ESTÁ LA CLAVE!

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 5. DELETE: Eliminar zona
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteZona(int id)
        {
            var zona = await _context.ZonasLounge.FindAsync(id);
            if (zona == null) return NotFound();

            _context.ZonasLounge.Remove(zona);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}