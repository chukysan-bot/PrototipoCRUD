using System;
using Npgsql;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    public class VentaDAO
    {
        private readonly string _cadenaConexion = "Host=localhost; Port=5432; Database=minisuper; Username=postgres; Password=postgres";

        public int RegistrarVenta(Venta venta)
        {
            venta.ValidarReglasDeNegocio();

            using var conexion = new NpgsqlConnection(_cadenaConexion);
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();
            try
            {
                string sqlCabecera = @"INSERT INTO ventas (fecha, cliente, subtotal, descuento_aplicado, total, vendedor_id)  VALUES (@fecha, @cliente, @subtotal, @descuento, @total, @vendedorId) RETURNING id";
                int idVentaGenerado;

                using (var cmdCab = new NpgsqlCommand(sqlCabecera, conexion, transaccion))
                {
                    cmdCab.Parameters.AddWithValue("fecha", venta.Fecha);
                    cmdCab.Parameters.AddWithValue("cliente", venta.Cliente);
                    cmdCab.Parameters.AddWithValue("subtotal", venta.Subtotal);
                    cmdCab.Parameters.AddWithValue("descuento", venta.DescuentoAplicado);
                    cmdCab.Parameters.AddWithValue("total", venta.Total);
                    cmdCab.Parameters.AddWithValue("vendedorId", venta.VendedorId);
                    idVentaGenerado = Convert.ToInt32(cmdCab.ExecuteScalar());
                }
                string sqlDetalle = @"INSERT INTO detalle_ventas 
                    (id_venta, id_producto, nombre_producto, cantidad, precio_unitario, subtotal) 
                    VALUES (@idVenta, @idProd, @nomProd, @cant, @precio, @sub)";
                foreach (var item in venta.Detalles)
                {
                    using var cmdDet = new NpgsqlCommand(sqlDetalle, conexion, transaccion);
                    cmdDet.Parameters.AddWithValue("idVenta", idVentaGenerado);
                    cmdDet.Parameters.AddWithValue("idProd", item.IdProducto);
                    cmdDet.Parameters.AddWithValue("nomProd", item.NombreProducto);
                    cmdDet.Parameters.AddWithValue("cant", item.Cantidad);
                    cmdDet.Parameters.AddWithValue("precio", item.PrecioUnitario);
                    cmdDet.Parameters.AddWithValue("sub", item.Subtotal);
                    cmdDet.ExecuteNonQuery();
                }
                transaccion.Commit();
                return idVentaGenerado;
            }
            catch (Exception)
            {
                transaccion.Rollback();
                throw;
            }
        }
    }
}