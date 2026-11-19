using System;
using System.Collections.Generic;
using System.Linq;

namespace PrototipoCRUD.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Cliente { get; set; } = "Consumidor Final";
        public int VendedorId { get; set; }
        public decimal DescuentoAplicado { get; set; } = 0m;
        public List<DetalleVenta> Detalles { get; set; } = new();

        public decimal Subtotal => Math.Round(Detalles.Sum(d => d.Subtotal), 2);

        public decimal Total => Math.Round(Subtotal - DescuentoAplicado, 2);

        public void ValidarReglasDeNegocio()
        {
            if (Detalles == null || Detalles.Count == 0 || Subtotal <= 0)
            {
                throw new ArgumentException("La venta debe tener al menos un producto con subtotal mayor a cero.");
            }

            if (DescuentoAplicado < 0)
            {
                throw new DescuentoInvalidoException("El descuento no puede ser negativo.");
            }

            decimal descuentoMaximoPermitido = Math.Round(Subtotal * 0.20m, 2);
            if (DescuentoAplicado > descuentoMaximoPermitido)
            {
                throw new DescuentoInvalidoException(
                    $"El descuento de Bs. {DescuentoAplicado:N2} excede el límite máximo corporativo del 20% (Bs. {descuentoMaximoPermitido:N2}).");
            }
        }
    }
}