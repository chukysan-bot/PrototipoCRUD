using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrototipoCRUD.Data;

namespace PrototipoCRUD.Controllers
{
    public class FilaReporte
    {
        public string Grupo { get; set; } = "";
        public string Col1 { get; set; } = "";
        public string Col2 { get; set; } = "";
        public string Col3 { get; set; } = "";
        public int CantidadOperaciones { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalCobrado { get; set; }
    }

    public class GrupoReporte
    {
        public string NombreGrupo { get; set; } = "";
        public List<FilaReporte> Filas { get; set; } = new();
        public int SubtotalOperaciones { get; set; }
        public decimal SubtotalDescuentos { get; set; }
        public decimal SubtotalCobrado { get; set; }
    }

    public class ReporteVentasViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Granularidad { get; set; } = "Diario";
        public List<GrupoReporte> Grupos { get; set; } = new();
        public int GranTotalOperaciones { get; set; }
        public decimal GranTotalDescuentos { get; set; }
        public decimal GranTotalCobrado { get; set; }
        public string? MensajeAviso { get; set; }
    }

    public class ReporteController : Controller
    {
        private readonly ReporteDAO _reporteDAO;

        public ReporteController(IConfiguration configuration)
        {
            string connStr = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Falta DefaultConnection en appsettings.json");
            _reporteDAO = new ReporteDAO(connStr);
        }

        [HttpGet]
        public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin, string granularidad = "Diario")
        {
            var inicio = fechaInicio ?? DateTime.Today.AddDays(-7);
            var fin = fechaFin ?? DateTime.Today;
            granularidad = granularidad == "Mensual" ? "Mensual" : "Diario";

            var vm = new ReporteVentasViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin,
                Granularidad = granularidad
            };

            try
            {
                DataTable dt = _reporteDAO.ObtenerReporteVentas(inicio, fin, granularidad);

                if (dt.Rows.Count == 0)
                {
                    vm.MensajeAviso = "No existen ventas registradas en el rango de fechas seleccionado. Intente ampliar el periodo de búsqueda.";
                    return View(vm);
                }

                var mapaGrupos = new Dictionary<string, GrupoReporte>();

                foreach (DataRow row in dt.Rows)
                {
                    if (granularidad == "Mensual")
                    {
                        string mes = row["periodo_mes"].ToString() ?? "";
                        if (!mapaGrupos.TryGetValue(mes, out var grp))
                        {
                            grp = new GrupoReporte { NombreGrupo = $"Mes: {mes}" };
                            mapaGrupos[mes] = grp;
                        }

                        int cant = Convert.ToInt32(row["cantidad_ventas"]);
                        decimal desc = Convert.ToDecimal(row["total_descuentos"]);
                        decimal cobrado = Convert.ToDecimal(row["total_cobrado"]);

                        object rawFechaDia = row["fecha_dia"];
                        string fechaDia = rawFechaDia switch
                        {
                            DateOnly d => d.ToString("yyyy-MM-dd"),
                            DateTime dtVal => dtVal.ToString("yyyy-MM-dd"),
                            _ => Convert.ToString(rawFechaDia) ?? ""
                        };
                        grp.Filas.Add(new FilaReporte
                        {
                            Grupo = mes,
                            Col1 = fechaDia,
                            Col2 = cant.ToString(),
                            Col3 = "-",
                            CantidadOperaciones = cant,
                            Descuento = desc,
                            TotalCobrado = cobrado
                        });
                        grp.SubtotalOperaciones += cant;
                        grp.SubtotalDescuentos += desc;
                        grp.SubtotalCobrado += cobrado;
                    }
                    else
                    {
                        string dia = row["periodo_grupo"].ToString() ?? "";
                        if (!mapaGrupos.TryGetValue(dia, out var grp))
                        {
                            grp = new GrupoReporte { NombreGrupo = $"Día: {dia}" };
                            mapaGrupos[dia] = grp;
                        }

                            decimal desc = Convert.ToDecimal(row["monto_descuento"]);
                            decimal cobrado = Convert.ToDecimal(row["total_cobrado"]);

                            grp.Filas.Add(new FilaReporte
                            {
                                Grupo = dia,
                                Col1 = $"#{row["nro_venta"]} ({row["hora"]})",
                                Col2 = row["cliente"].ToString() ?? "",
                                Col3 = "Venta",
                                CantidadOperaciones = 1,
                                Descuento = desc,
                                TotalCobrado = cobrado
                            });
                        grp.SubtotalOperaciones += 1;
                        grp.SubtotalDescuentos += desc;
                        grp.SubtotalCobrado += cobrado;
                    }
                }

                vm.Grupos.AddRange(mapaGrupos.Values);
                foreach (var g in vm.Grupos)
                {
                    vm.GranTotalOperaciones += g.SubtotalOperaciones;
                    vm.GranTotalDescuentos += g.SubtotalDescuentos;
                    vm.GranTotalCobrado += g.SubtotalCobrado;
                }
            }
            catch (Exception ex)
            {
                vm.MensajeAviso = ex.Message;
            }

            return View(vm);
        }
    }
}