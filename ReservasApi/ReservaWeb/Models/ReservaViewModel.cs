using System.ComponentModel.DataAnnotations;

namespace ReservaWeb.Models
{
    public class ReservaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha y hora son obligatorias")]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una zona")]
        [Display(Name = "Zona del Lounge")]
        public int ZonaLoungeId { get; set; }


        public ZonaViewModel? ZonaLounge { get; set; }
        public UsuarioViewModel? Usuario { get; set; }
    }

    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}