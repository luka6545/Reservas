using Microsoft.EntityFrameworkCore;
using ReservasApi.Models;

namespace ReservasApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ZonaLounge> ZonasLounge { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<CategoriaBebida> CategoriasBebidas { get; set; }
        public DbSet<Bebida> Bebidas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedidos { get; set; }
    }
}