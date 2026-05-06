namespace ReservasApi.Models
{
    public class Bebida
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        public int CategoriaId { get; set; }
        public CategoriaBebida? Categoria { get; set; }
    }
}