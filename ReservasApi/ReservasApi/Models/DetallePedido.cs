namespace ReservasApi.Models
{
    public class DetallePedido
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }

        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        public int BebidaId { get; set; }
        public Bebida? Bebida { get; set; }
    }
}