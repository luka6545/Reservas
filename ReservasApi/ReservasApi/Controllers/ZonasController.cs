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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ZonaLounge>>> GetZonas()
        {
            return await _context.ZonasLounge.ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ZonaLounge>> PostZona(ZonaLounge zona)
        {
            _context.ZonasLounge.Add(zona);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetZonas), new { id = zona.Id }, zona);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutZona(int id, ZonaLounge zona)
        {
            if (id != zona.Id) return BadRequest("Los IDs no coinciden");

            _context.Entry(zona).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

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