using System;
using System.Collections.Generic;
using Npgsql;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    public class ProductoDAO
    {
        private const string SqlSelect =
            "SELECT p.id, p.nombre, p.codigo_barras, p.precio, p.stock, p.stock_minimo, " +
            "p.fecha_vencimiento, p.proveedor_id, pr.nombre " +
            "FROM productos p LEFT JOIN proveedores pr ON pr.id = p.proveedor_id ";

        public void Insertar(Producto p)
        {
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                string sql = "INSERT INTO productos (nombre, codigo_barras, precio, stock, stock_minimo, fecha_vencimiento, proveedor_id) " +
                            "VALUES (@nombre, @codigo, @precio, @stock, @minimo, @vence, @proveedor)";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    Parametros(comando, p);
                    comando.ExecuteNonQuery();
                }
            }
        }

        public List<Producto> ObtenerTodos()
        {
            var lista = new List<Producto>();
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand(SqlSelect + "ORDER BY p.id", conexion))
                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(Leer(reader));
                    }
                }
            }
            return lista;
        }

        public Producto? ObtenerPorId(int id)
        {
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand(SqlSelect + "WHERE p.id = @id", conexion))
                {
                    comando.Parameters.AddWithValue("id", id);
                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read()) return Leer(reader);
                    }
                }
            }
            return null;
        }

        public void Actualizar(Producto p)
        {
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                string sql = "UPDATE productos SET nombre = @nombre, codigo_barras = @codigo, precio = @precio, " +
                        "stock = @stock, stock_minimo = @minimo, fecha_vencimiento = @vence, proveedor_id = @proveedor " +
                        "WHERE id = @id";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    Parametros(comando, p);
                    comando.Parameters.AddWithValue("id", p.Id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        public void Eliminar(int id)
        {
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand("DELETE FROM productos WHERE id = @id", conexion))
                {
                    comando.Parameters.AddWithValue("id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        private static void Parametros(NpgsqlCommand comando, Producto p)
        {
            comando.Parameters.AddWithValue("nombre", p.Nombre);
            comando.Parameters.AddWithValue("codigo", p.CodigoBarras);
            comando.Parameters.AddWithValue("precio", p.Precio);
            comando.Parameters.AddWithValue("stock", p.Stock);
            comando.Parameters.AddWithValue("minimo", p.StockMinimo);
            comando.Parameters.AddWithValue("vence", NpgsqlTypes.NpgsqlDbType.Date,
                p.FechaVencimiento.HasValue ? (object)p.FechaVencimiento.Value.Date : DBNull.Value);
            comando.Parameters.AddWithValue("proveedor", NpgsqlTypes.NpgsqlDbType.Integer,
                p.ProveedorId.HasValue ? (object)p.ProveedorId.Value : DBNull.Value);
        }

        private static Producto Leer(NpgsqlDataReader reader)
        {
            return new Producto
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                CodigoBarras = reader.GetString(2),
                Precio = reader.GetDecimal(3),
                Stock = reader.GetInt32(4),
                StockMinimo = reader.GetInt32(5),
                FechaVencimiento = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                ProveedorId = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                ProveedorNombre = reader.IsDBNull(8) ? null : reader.GetString(8)
            };
        }
    }
}
