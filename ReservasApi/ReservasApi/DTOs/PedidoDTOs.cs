namespace ReservasApi.DTOs
{
    public class CrearPedidoDTO
    {
        public int ReservaId { get; set; }
        public List<ItemPedidoDTO> Items { get; set; } = new List<ItemPedidoDTO>();
    }

    public class ItemPedidoDTO
    {
        public int BebidaId { get; set; }
        public int Cantidad { get; set; }
    }
}