namespace ReservasApi.Models
{
    public class ZonaLounge
    {
        public int Id { get; set; }
        public string NombreZona { get; set; } = string.Empty;
        public int CapacidadMax { get; set; }
        public decimal PrecioMinimo { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}