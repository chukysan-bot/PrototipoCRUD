using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class VentaController : Controller
    {
        private readonly string _connectionString;
        private readonly VentaDAO _ventaDAO;

        public VentaController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _ventaDAO = new VentaDAO(_connectionString);
        }

        private List<Producto> ObtenerProductos()
        {
            var lista = new List<Producto>();

            if (string.IsNullOrWhiteSpace(_connectionString))
                return lista;

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string sql = "SELECT id, nombre, precio FROM productos ORDER BY nombre;";
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Precio = reader.GetDecimal(2)
                });
            }

            return lista;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Productos = ObtenerProductos();
            return View(new Venta());
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] Venta venta)
        {
            if (venta == null)
            {
                return BadRequest(new { exito = false, mensaje = "No se recibieron los datos de la venta." });
            }

            try
            {
                venta.AplicarDescuento(venta.PorcentajeDescuento);

                if (string.IsNullOrWhiteSpace(venta.Vendedor))
                {
                    venta.Vendedor = "Vendedor General";
                }

                _ventaDAO.RegistrarVenta(venta);

                return Ok(new
                {
                    exito = true,
                    mensaje = $"Venta registrada correctamente. Total final: Bs. {venta.TotalFinal:0.00} (Descuento: {venta.PorcentajeDescuento}%)."
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { exito = false, mensaje = $"No se pudo registrar la venta: {ex.Message}" });
            }
        }
    }
}