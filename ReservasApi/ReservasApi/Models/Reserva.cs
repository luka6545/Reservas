namespace ReservasApi.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } = "Pendiente";

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int ZonaLoungeId { get; set; }
        public ZonaLounge? ZonaLounge { get; set; }
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    }
}