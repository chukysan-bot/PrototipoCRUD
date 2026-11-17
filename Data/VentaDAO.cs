using System;
using Npgsql;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    public class VentaDAO
    {
        // Asegúrate de poner tu contraseña real de PostgreSQL aquí
        private readonly string _cadenaConexion = "Host=localhost; Port=5432; Database=minisuper; Username=postgres; Password=postgres";

        public int RegistrarVenta(Venta venta)
        {
            using var conexion = new NpgsqlConnection(_cadenaConexion);
            conexion.Open();

            // Iniciamos la transacción atómica en PostgreSQL
            using var transaccion = conexion.BeginTransaction();
            try
            {
                // 1. Insertar Cabecera y recuperar el ID generado
                string sqlCabecera = "INSERT INTO ventas (fecha, cliente, total) VALUES (@fecha, @cliente, @total) RETURNING id";
                int idVentaGenerado;

                using (var cmdCab = new NpgsqlCommand(sqlCabecera, conexion, transaccion))
                {
                    cmdCab.Parameters.AddWithValue("fecha", venta.Fecha);
                    cmdCab.Parameters.AddWithValue("cliente", venta.Cliente);
                    cmdCab.Parameters.AddWithValue("total", venta.Total);
                    idVentaGenerado = Convert.ToInt32(cmdCab.ExecuteScalar());
                }

                // 2. Insertar cada línea del Detalle vinculada al ID de la cabecera
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

                // 3. Confirmar transacción si todo va bien
                transaccion.Commit();
                return idVentaGenerado;
            }
            catch (Exception)
            {
                // Revertir si ocurre un error (Rollback)
                transaccion.Rollback();
                throw;
            }
        }
    }
}