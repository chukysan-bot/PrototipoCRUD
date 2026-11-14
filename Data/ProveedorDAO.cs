using System.Collections.Generic;
using Npgsql;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    public class ProveedorDAO
    {
        public void Insertar(Proveedor p)
        {
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                string sql = "INSERT INTO proveedores (nombre, telefono, correo) VALUES (@nombre, @telefono, @correo)";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("nombre", p.Nombre);
                    comando.Parameters.AddWithValue("telefono", (object?)p.Telefono ?? System.DBNull.Value);
                    comando.Parameters.AddWithValue("correo", (object?)p.Correo ?? System.DBNull.Value);
                    comando.ExecuteNonQuery();
                }
            }
        }

        public List<Proveedor> ObtenerTodos()
        {
            var lista = new List<Proveedor>();
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand("SELECT id, nombre, telefono, correo FROM proveedores ORDER BY id", conexion))
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

        public Proveedor? ObtenerPorId(int id)
        {
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand("SELECT id, nombre, telefono, correo FROM proveedores WHERE id = @id", conexion))
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

        public void Actualizar(Proveedor p)
        {
            using (var conexion = new NpgsqlConnection(Conexion.Cadena))
            {
                conexion.Open();
                string sql = "UPDATE proveedores SET nombre = @nombre, telefono = @telefono, correo = @correo WHERE id = @id";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("nombre", p.Nombre);
                    comando.Parameters.AddWithValue("telefono", (object?)p.Telefono ?? System.DBNull.Value);
                    comando.Parameters.AddWithValue("correo", (object?)p.Correo ?? System.DBNull.Value);
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
                using (var comando = new NpgsqlCommand("DELETE FROM proveedores WHERE id = @id", conexion))
                {
                    comando.Parameters.AddWithValue("id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        private static Proveedor Leer(NpgsqlDataReader reader)
        {
            return new Proveedor
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Telefono = reader.IsDBNull(2) ? null : reader.GetString(2),
                Correo = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }
    }
}
