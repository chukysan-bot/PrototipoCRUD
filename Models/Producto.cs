using System;
using System.ComponentModel.DataAnnotations;

namespace PrototipoCRUD.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El código de barras es obligatorio")]
        public string CodigoBarras { get; set; } = "";

        [Range(0, 999999, ErrorMessage = "El precio no puede ser negativo")]
        public decimal Precio { get; set; }

        [Range(0, 999999, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Range(0, 999999, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int StockMinimo { get; set; } = 5;

        public DateTime? FechaVencimiento { get; set; }

        public int? ProveedorId { get; set; }

        // Solo para mostrar en la lista (viene del JOIN)
        public string? ProveedorNombre { get; set; }
    }
}
