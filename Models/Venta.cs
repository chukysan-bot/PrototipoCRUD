using System;
using System.Collections.Generic;
using System.Linq;

namespace PrototipoCRUD.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now; // Valor por defecto
        public string Cliente { get; set; } = "Consumidor Final"; // Valor por defecto
        public List<DetalleVenta> Detalles { get; set; } = new();

        // Cálculo automático del Total usando LINQ
        public decimal Total => Detalles.Sum(d => d.Subtotal);
    }
}