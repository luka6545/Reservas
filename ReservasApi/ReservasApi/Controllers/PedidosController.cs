using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasApi.Data;
using ReservasApi.DTOs;
using ReservasApi.Models;

namespace ReservasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Traer la cuenta completa de la mesa
        [HttpGet("Reserva/{reservaId}")]
        public async Task<ActionResult<Pedido>> GetPedidoPorReserva(int reservaId)
        {
            // CLAVE: Ordenamos de forma descendente y tomamos el primero (el más reciente)
            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Bebida)
                .Where(p => p.ReservaId == reservaId)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            if (pedido == null) return NotFound("Aún no hay pedidos para esta reserva.");

            return pedido;
        }

        // POST: Recibir el "Carrito de Compras" del cliente
        [HttpPost]
        public async Task<IActionResult> CrearPedido(CrearPedidoDTO request)
        {
            // 1. Validar que la Reserva sea válida para pedir
            var reserva = await _context.Reservas.FindAsync(request.ReservaId);
            if (reserva == null) return NotFound("La reserva no existe.");
            if (reserva.Estado != "Aprobada") return BadRequest("Solo se pueden hacer pedidos en reservas Aprobadas.");

            if (request.Items == null || !request.Items.Any())
                return BadRequest("El pedido no contiene ninguna bebida.");

            // 2. Buscar si la mesa ya tiene una cuenta abierta. Si no, la creamos.
            var pedido = await _context.Pedidos
     .FirstOrDefaultAsync(p => p.ReservaId == request.ReservaId && p.Estado != "Cobrado");

            if (pedido == null)
            {
                pedido = new Pedido
                {
                    ReservaId = request.ReservaId,
                    FechaPedido = DateTime.Now,
                    Estado = "Preparando",
                    Total = 0
                };
                _context.Pedidos.Add(pedido);
            }

            // 3. Procesar el "Carrito" (Recorremos la lista de tus DTOs)
            foreach (var item in request.Items)
            {
                var bebida = await _context.Bebidas.FindAsync(item.BebidaId);

                if (bebida == null)
                    return NotFound($"La bebida solicitada no existe.");

                if (bebida.Stock < item.Cantidad)
                    return BadRequest($"Stock insuficiente. Solo quedan {bebida.Stock} unidades de {bebida.Nombre}.");

                // Calculamos el costo de esta línea
                var subtotal = bebida.Precio * item.Cantidad;

                // Creamos el detalle
                var detalle = new DetallePedido
                {
                    BebidaId = bebida.Id,
                    Cantidad = item.Cantidad,
                    Subtotal = subtotal,
                    Pedido = pedido // Entity Framework enlazará los IDs automáticamente al guardar
                };

                _context.DetallesPedidos.Add(detalle);

                // 4. MATEMÁTICAS CLAVE: Descontamos inventario y sumamos a la cuenta total
                bebida.Stock -= item.Cantidad;
                pedido.Total += subtotal;
            }

            // 5. Guardar todo en cascada
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Pedido procesado con éxito.",
                totalCuenta = pedido.Total
            });
        }
        // PUT: api/Pedidos/Cobrar/5
        [HttpPut("Cobrar/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CobrarPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound("Pedido no encontrado.");

            pedido.Estado = "Cobrado";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cuenta cobrada exitosamente." });
        }
        // GET: api/Pedidos/Reserva/5/Historial
        // Este método trae TODOS los pedidos de una mesa (Pagados y Sin Pagar)
        [HttpGet("Reserva/{reservaId}/Historial")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetHistorialPedidos(int reservaId)
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Bebida)
                .Where(p => p.ReservaId == reservaId)
                .OrderByDescending(p => p.Id) // Los más recientes primero
                .ToListAsync();

            return Ok(pedidos);
        }
        // GET: api/Pedidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Bebida)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();
            return Ok(pedido);
        }
    }
}