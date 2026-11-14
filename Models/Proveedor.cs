using System.ComponentModel.DataAnnotations;

namespace PrototipoCRUD.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = "";

        public string? Telefono { get; set; }

        public string? Correo { get; set; }
    }
}
