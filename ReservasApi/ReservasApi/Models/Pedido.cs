namespace ReservasApi.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = "Preparando";
        public decimal Total { get; set; }

        public int ReservaId { get; set; }
        public Reserva? Reserva { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}