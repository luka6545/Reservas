namespace ReservaWeb.Models
{
    public class ZonaViewModel
    {
        public int Id { get; set; }

        public string NombreZona { get; set; } = string.Empty;

        public int CapacidadMax { get; set; }

        public decimal PrecioMinimo { get; set; }
    }
}