namespace ReservasApi.Models
{
    public class CategoriaBebida
    {
        public int Id { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Bebida> Bebidas { get; set; } = new List<Bebida>();
    }
}