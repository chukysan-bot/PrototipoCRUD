using System;
using Npgsql;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    public class VentaDAO
    {
        private readonly string _connectionString;

        public VentaDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void RegistrarVenta(Venta venta)
        {
            if (venta == null)
                throw new ArgumentNullException(nameof(venta));

            if (venta.Detalles == null || venta.Detalles.Count == 0)
                throw new ArgumentException("La venta debe tener al menos un detalle.");

            if (venta.PorcentajeDescuento < 0 || venta.PorcentajeDescuento > 20)
                throw new ArgumentOutOfRangeException(
                    nameof(venta.PorcentajeDescuento),
                    "El descuento permitido debe estar entre 0% y un máximo de 20%."
                );

            venta.AplicarDescuento(venta.PorcentajeDescuento);

            string vendedorFinal = string.IsNullOrWhiteSpace(venta.Vendedor)
                ? "Vendedor General"
                : venta.Vendedor.Trim();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                string sqlVenta = @"
                    INSERT INTO ventas (
                        fecha,
                        cliente,
                        vendedor,
                        subtotal_bruto,
                        porcentaje_descuento,
                        monto_descuento,
                        total_final,
                        total
                    )
                    VALUES (
                        @fecha,
                        @cliente,
                        @vendedor,
                        @subtotal_bruto,
                        @porcentaje_descuento,
                        @monto_descuento,
                        @total_final,
                        @total
                    )
                    RETURNING id;";

                int ventaId;
                using (var cmdVenta = new NpgsqlCommand(sqlVenta, conn, tx))
                {
                    cmdVenta.Parameters.AddWithValue("@fecha", venta.Fecha);
                    cmdVenta.Parameters.AddWithValue("@cliente", venta.Cliente ?? "Consumidor Final");
                    cmdVenta.Parameters.AddWithValue("@vendedor", vendedorFinal);
                    cmdVenta.Parameters.AddWithValue("@subtotal_bruto", venta.SubtotalBruto);
                    cmdVenta.Parameters.AddWithValue("@porcentaje_descuento", venta.PorcentajeDescuento);
                    cmdVenta.Parameters.AddWithValue("@monto_descuento", venta.MontoDescuento);
                    cmdVenta.Parameters.AddWithValue("@total_final", venta.TotalFinal);
                    cmdVenta.Parameters.AddWithValue("@total", venta.TotalFinal);

                    ventaId = Convert.ToInt32(cmdVenta.ExecuteScalar());
                }

                string sqlDetalle = @"
                    INSERT INTO detalle_ventas (
                        id_venta,
                        id_producto,
                        nombre_producto,
                        cantidad,
                        precio_unitario,
                        subtotal
                    )
                    VALUES (
                        @id_venta,
                        @id_producto,
                        @nombre_producto,
                        @cantidad,
                        @precio_unitario,
                        @subtotal
                    );";

                foreach (var detalle in venta.Detalles)
                {
                    decimal subtotalLinea = detalle.Subtotal > 0
                        ? detalle.Subtotal
                        : detalle.Cantidad * detalle.PrecioUnitario;

                    using var cmdDetalle = new NpgsqlCommand(sqlDetalle, conn, tx);
                    cmdDetalle.Parameters.AddWithValue("@id_venta", ventaId);
                    cmdDetalle.Parameters.AddWithValue("@id_producto", detalle.IdProducto);
                    cmdDetalle.Parameters.AddWithValue("@nombre_producto", (object?)detalle.NombreProducto ?? "Producto");
                    cmdDetalle.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                    cmdDetalle.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);
                    cmdDetalle.Parameters.AddWithValue("@subtotal", subtotalLinea);
                    cmdDetalle.ExecuteNonQuery();
                }

                tx.Commit();
                venta.Id = ventaId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}