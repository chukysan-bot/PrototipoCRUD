using System;
using System.Data;
using System.IO;
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
            // 1. Cláusulas de guarda (validaciones rápidas antes de tocar la BD)
            if (venta == null)
                throw new ArgumentNullException(nameof(venta), "No se recibieron los datos de la venta.");

            if (venta.Detalles == null || venta.Detalles.Count == 0)
                throw new ArgumentException("Debe agregar al menos un producto a la venta antes de confirmar.");

            if (venta.PorcentajeDescuento < 0 || venta.PorcentajeDescuento > 20)
                throw new ArgumentOutOfRangeException(
                    nameof(venta.PorcentajeDescuento),
                    "El descuento permitido debe estar entre 0% y un máximo de 20%."
                );

            venta.AplicarDescuento(venta.PorcentajeDescuento);

            string vendedorFinal = string.IsNullOrWhiteSpace(venta.Vendedor)
                ? "Vendedor General"
                : venta.Vendedor.Trim();

            NpgsqlConnection? conn = null;
            NpgsqlTransaction? tx = null;

            try
            {
                // 2. Bloque TRY específico para la operación en PostgreSQL
                conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                tx = conn.BeginTransaction();

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
            catch (PostgresException exPg)
            {
                // 3a. Error específico del motor PostgreSQL (ej. violación de clave, restricción, etc.)
                try { tx?.Rollback(); } catch { }
                RegistrarLogTecnico(exPg);

                if (exPg.SqlState == "23505")
                    throw new InvalidOperationException("Ya existe un registro duplicado con esos datos. Verifique e intente nuevamente.");

                if (exPg.SqlState == "23503")
                    throw new InvalidOperationException("Uno de los productos seleccionados no existe o está vinculado a otro registro.");

                throw new InvalidOperationException("No se pudo guardar la venta por una restricción de datos en la base de datos.");
            }
            catch (NpgsqlException exRed)
            {
                // 3b. Error de conexión o servicio PostgreSQL apagado
                try { tx?.Rollback(); } catch { }
                RegistrarLogTecnico(exRed);
                throw new InvalidOperationException("No fue posible conectar con la base de datos en este momento. Verifique que el servicio PostgreSQL esté activo e intente nuevamente.");
            }
            catch (Exception exGeneral)
            {
                // 3c. Red de seguridad final: registra log y relanza preservando Call Stack
                try { tx?.Rollback(); } catch { }
                RegistrarLogTecnico(exGeneral);
                throw;
            }
            finally
            {
                // 4. Limpieza garantizada de transacción y conexión
                tx?.Dispose();
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn?.Dispose();
            }
        }

        private static void RegistrarLogTecnico(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "errores_sistema.log");
                string contenido = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n--------------------------------------------------\n";
                File.AppendAllText(logPath, contenido);
            }
            catch
            {
                // Evita que un problema al escribir el log oculte el error principal
            }
        }
    }
}
