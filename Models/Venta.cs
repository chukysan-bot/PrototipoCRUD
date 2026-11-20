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
        public string Vendedor { get; set; } = string.Empty;
        public List<DetalleVenta> Detalles { get; set; } = new();

        public decimal PorcentajeDescuento { get; set; }

        public decimal SubtotalBruto => Math.Round(Detalles.Sum(d => d.Subtotal), 2);

        public decimal MontoDescuento => Math.Round(SubtotalBruto * (PorcentajeDescuento / 100m), 2);

        public decimal TotalFinal => Math.Round(SubtotalBruto - MontoDescuento, 2);

        public void AplicarDescuento(decimal porcentaje)
        {
            if (porcentaje < 0 || porcentaje > 20)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(porcentaje),
                    "El descuento permitido debe estar entre 0% y un máximo de 20%."
                );
            }

            PorcentajeDescuento = porcentaje;
        }
    }
}