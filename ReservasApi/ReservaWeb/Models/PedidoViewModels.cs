namespace ReservaWeb.Models
{
    // 1. Para enviar el pedido nuevo a la API (El Carrito)
    public class CrearPedidoViewModel
    {
        public int ReservaId { get; set; }
        public List<ItemPedidoViewModel> Items { get; set; } = new List<ItemPedidoViewModel>();
    }

    public class ItemPedidoViewModel
    {
        public int BebidaId { get; set; }
        public int Cantidad { get; set; }
    }

    // 2. Para leer la cuenta actual desde la API (La Factura)
    public class PedidoViewModel
    {
        public int Id { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int ReservaId { get; set; }
        public List<DetallePedidoViewModel> Detalles { get; set; } = new List<DetallePedidoViewModel>();
    }

    public class DetallePedidoViewModel
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public BebidaViewModel? Bebida { get; set; }
    }
}