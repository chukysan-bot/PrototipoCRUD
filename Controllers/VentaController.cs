using System;
using Microsoft.AspNetCore.Mvc;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaDAO _ventaDao = new VentaDAO();
        private readonly ProductoDAO _productoDao = new ProductoDAO();

        // GET: Venta/Create
        public IActionResult Create()
        {
            ViewBag.Productos = _productoDao.ObtenerTodos();
            return View(new Venta());
        }

        // POST: Venta/Registrar
        [HttpPost]
        public IActionResult Registrar([FromBody] Venta venta)
        {
            if (venta == null || venta.Detalles == null || venta.Detalles.Count == 0)
            {
                return BadRequest(new { exito = false, mensaje = "Debe agregar al menos un producto a la venta." });
            }

            foreach (var item in venta.Detalles)
            {
                if (item.Cantidad <= 0)
                {
                    return BadRequest(new { exito = false, mensaje = $"La cantidad para '{item.NombreProducto}' debe ser mayor a 0." });
                }
            }

            try
            {
                if (string.IsNullOrWhiteSpace(venta.Cliente))
                {
                    venta.Cliente = "Consumidor Final";
                }
                venta.Fecha = DateTime.Now;

                int idVentaGenerado = _ventaDao.RegistrarVenta(venta);

                return Ok(new 
                { 
                    exito = true, 
                    mensaje = $"¡Venta #{idVentaGenerado} registrada con éxito por un total de Bs. {venta.Total:N2}!" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error al guardar en la base de datos: " + ex.Message });
            }
        }
    }
}