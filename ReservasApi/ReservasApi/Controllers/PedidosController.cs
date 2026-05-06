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
    [Authorize] // Protegido por JWT
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Pedidos (El cliente hace una orden desde su mesa)
        [HttpPost]
        public async Task<IActionResult> CrearPedido(CrearPedidoDTO request)
        {
            // 1. Validar que la reserva exista y no esté cancelada
            var reserva = await _context.Reservas.FindAsync(request.ReservaId);
            if (reserva == null || reserva.Estado == "Cancelada")
                return BadRequest("Reserva no válida o cancelada.");

            // 2. Crear la cabecera del pedido
            var nuevoPedido = new Pedido
            {
                ReservaId = request.ReservaId,
                FechaPedido = DateTime.Now,
                Estado = "Preparando",
                Total = 0 // Lo calcularemos ahora
            };

            // 3. Procesar cada bebida solicitada
            foreach (var item in request.Items)
            {
                var bebidaDb = await _context.Bebidas.FindAsync(item.BebidaId);
                if (bebidaDb == null) return NotFound($"La bebida con ID {item.BebidaId} no existe.");

                if (bebidaDb.Stock < item.Cantidad)
                    return BadRequest($"No hay suficiente stock para {bebidaDb.Nombre}. Stock actual: {bebidaDb.Stock}");

                // Descontar del inventario
                bebidaDb.Stock -= item.Cantidad;

                // Calcular subtotal
                var subtotal = bebidaDb.Precio * item.Cantidad;
                nuevoPedido.Total += subtotal;

                // Agregar el detalle al pedido
                nuevoPedido.Detalles.Add(new DetallePedido
                {
                    BebidaId = item.BebidaId,
                    Cantidad = item.Cantidad,
                    Subtotal = subtotal
                });
            }

            // 4. Guardar todo en la base de datos (Pedido, Detalles y actualización de Stock)
            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Pedido registrado con éxito", pedidoId = nuevoPedido.Id, total = nuevoPedido.Total });
        }

        // GET: api/Pedidos/Reserva/5 (Ver todo lo que se ha pedido en una mesa)
        [HttpGet("Reserva/{reservaId}")]
        public async Task<IActionResult> GetPedidosPorReserva(int reservaId)
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Bebida)
                .Where(p => p.ReservaId == reservaId)
                .Select(p => new {
                    p.Id,
                    p.FechaPedido,
                    p.Estado,
                    p.Total,
                    Detalles = p.Detalles.Select(d => new {
                        d.Bebida!.Nombre,
                        d.Cantidad,
                        d.Subtotal
                    })
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        // PUT: api/Pedidos/5/Estado (El bartender/admin actualiza si ya se entregó)
        [HttpPut("{id}/Estado")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiarEstadoPedido(int id, [FromBody] string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Estado del pedido actualizado a {nuevoEstado}" });
        }
    }
}