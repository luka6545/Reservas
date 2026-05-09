using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.Models;

namespace ReservasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Solo usuarios con sesión iniciada (Admin o Cliente) pueden acceder
    public class ReservasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
        {
            // Traemos las reservas con la info de la Zona y del Usuario para no ver solo números
            return await _context.Reservas
                .Include(r => r.ZonaLounge)
                .Include(r => r.Usuario)
                .ToListAsync();
        }

        // GET: api/Reservas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.ZonaLounge)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null) return NotFound();
            return reserva;
        }

        // POST: Crear Reserva
        [HttpPost]
        public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
        {
            // TRUCO DE SEGURIDAD: Desvinculamos objetos completos para evitar errores de EF Core
            reserva.Usuario = null;
            reserva.ZonaLounge = null;
            reserva.Estado = "Pendiente"; // Por seguridad, siempre nacen como Pendientes

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.Id }, reserva);
        }

        // PUT: Actualizar Reserva (ej: El Admin la cambia a "Aprobada" o "Cancelada")
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, Reserva reserva)
        {
            if (id != reserva.Id) return BadRequest("Los IDs no coinciden");

            var reservaDb = await _context.Reservas.FindAsync(id);
            if (reservaDb == null) return NotFound();

            // Sincronización manual segura
            reservaDb.FechaHora = reserva.FechaHora;
            reservaDb.Estado = reserva.Estado;
            reservaDb.ZonaLoungeId = reserva.ZonaLoungeId;
            // OJO: No actualizamos el UsuarioId porque la reserva siempre le pertenece a quien la creó

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: Eliminar Reserva
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo el Admin puede borrar registros físicamente
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}