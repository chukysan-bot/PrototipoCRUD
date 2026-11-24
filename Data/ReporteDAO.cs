using System;
using System.Data;
using System.IO;
using Npgsql;

namespace PrototipoCRUD.Data
{
    public class ReporteDAO
    {
        private readonly string _connectionString;

        public ReporteDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable ObtenerReporteVentas(DateTime inicio, DateTime fin, string tipoGranularidad)
        {
            DataTable dt = new DataTable("ReporteVentas");

            if (fin.Date < inicio.Date)
                throw new ArgumentException("La fecha final no puede ser menor que la fecha inicial.");

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                string sql = tipoGranularidad == "Mensual"
                    ? @"SELECT
                            TO_CHAR(fecha, 'YYYY-MM') AS periodo_mes,
                            fecha::date AS fecha_dia,
                            COUNT(id) AS cantidad_ventas,
                            COALESCE(SUM(descuento_aplicado), 0) AS total_descuentos,
                            COALESCE(SUM(total), 0) AS total_cobrado
                        FROM ventas
                        WHERE fecha::date BETWEEN @inicio AND @fin
                        GROUP BY TO_CHAR(fecha, 'YYYY-MM'), fecha::date
                        ORDER BY periodo_mes ASC, fecha_dia ASC;"
                    : @"SELECT
                            TO_CHAR(fecha, 'YYYY-MM-DD') AS periodo_grupo,
                            id AS nro_venta,
                            TO_CHAR(fecha, 'HH24:MI') AS hora,
                            cliente,
                            COALESCE(descuento_aplicado, 0) AS monto_descuento,
                            COALESCE(total, 0) AS total_cobrado
                        FROM ventas
                        WHERE fecha::date BETWEEN @inicio AND @fin
                        ORDER BY fecha ASC, id ASC;";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("inicio", inicio.Date);
                cmd.Parameters.AddWithValue("fin", fin.Date);

                using var adapter = new NpgsqlDataAdapter(cmd);
                adapter.Fill(dt);

                return dt;
            }
            catch (NpgsqlException ex)
            {
                RegistrarLogTecnico(ex);
                throw new InvalidOperationException(
                    "No pudimos consultar los datos para generar el reporte. Verifique la conexión con la base de datos.",
                    ex);
            }
        }

        private void RegistrarLogTecnico(Exception ex)
        {
            try
            {
                string rutaLog = Path.Combine(AppContext.BaseDirectory, "errores_sistema.log");
                string entrada = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n----------------------------------------\n";
                File.AppendAllText(rutaLog, entrada);
            }
            catch
            {
                Console.Error.WriteLine($"[ERROR LOG] {DateTime.Now}: {ex}");
            }
        }
    }
}