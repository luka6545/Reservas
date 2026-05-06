using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.Models;
using ReservasApi.DTOs;

namespace ReservasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // MUY IMPORTANTE: Exige que el usuario esté logueado para usar CUALQUIER método aquí
    public class ReservasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Reservas (El cliente crea una reserva)
        [HttpPost]
        public async Task<IActionResult> CrearReserva(CrearReservaDTO request)
        {
            // 1. Extraer el ID del usuario directamente del Token JWT de forma segura
            var usuarioIdString = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(usuarioIdString)) return Unauthorized("Token inválido.");
            int usuarioId = int.Parse(usuarioIdString);

            // 2. Validar que la Zona exista
            var zonaExiste = await _context.ZonasLounge.AnyAsync(z => z.Id == request.ZonaLoungeId);
            if (!zonaExiste) return NotFound("La zona seleccionada no existe.");

            // 3. Validar que la fecha sea en el futuro
            if (request.FechaHora <= DateTime.Now)
                return BadRequest("La reserva debe ser para una fecha y hora futura.");

            // 4. Lógica de negocio: Evitar reservas dobles en la misma zona con 2 horas de diferencia
            var reservaConflicto = await _context.Reservas
                .Where(r => r.ZonaLoungeId == request.ZonaLoungeId
                         && r.Estado != "Cancelada")
                .AnyAsync(r => Math.Abs((r.FechaHora - request.FechaHora).TotalHours) < 2);

            if (reservaConflicto)
                return BadRequest("Esta zona ya está reservada para ese horario. Por favor, elige otro.");

            // 5. Crear la reserva
            var nuevaReserva = new Reserva
            {
                UsuarioId = usuarioId,
                ZonaLoungeId = request.ZonaLoungeId,
                FechaHora = request.FechaHora,
                Estado = "Pendiente"
            };

            _context.Reservas.Add(nuevaReserva);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva creada con éxito", idReserva = nuevaReserva.Id });
        }

        // GET: api/Reservas/MisReservas (El cliente ve su propio historial)
        [HttpGet("MisReservas")]
        public async Task<IActionResult> GetMisReservas()
        {
            var usuarioId = int.Parse(User.FindFirst("Id")!.Value);

            var reservas = await _context.Reservas
                .Include(r => r.ZonaLounge) // Traemos la info de la mesa
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.FechaHora)
                .Select(r => new {
                    r.Id,
                    Zona = r.ZonaLounge!.NombreZona,
                    r.FechaHora,
                    r.Estado
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // GET: api/Reservas/Todas (Solo el Admin puede ver todas las reservas del local)
        [HttpGet("Todas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTodasLasReservas()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.ZonaLounge)
                .OrderBy(r => r.FechaHora)
                .Select(r => new {
                    r.Id,
                    Cliente = r.Usuario!.Nombre,
                    Zona = r.ZonaLounge!.NombreZona,
                    r.FechaHora,
                    r.Estado
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // PUT: api/Reservas/5/Estado (El Admin cambia el estado a Confirmada o Cancelada)
        [HttpPut("{id}/Estado")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            reserva.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Estado de la reserva actualizado a {nuevoEstado}" });
        }
    }
}