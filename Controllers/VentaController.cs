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

        public IActionResult Create()
        {
            ViewBag.Productos = _productoDao.ObtenerTodos();
            return View(new Venta());
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] Venta venta)
        {
            if (venta == null)
            {
                return BadRequest(new { exito = false, mensaje = "Datos de venta inválidos." });
            }

            try
            {
                if (string.IsNullOrWhiteSpace(venta.Cliente))
                {
                    venta.Cliente = "Consumidor Final";
                }
                venta.Fecha = DateTime.Now;

                var claimId = User?.FindFirst("VendedorId")?.Value;
                venta.VendedorId = int.TryParse(claimId, out int idAutenticado) ? idAutenticado : 1;

                int idVentaGenerado = _ventaDao.RegistrarVenta(venta);

                return Ok(new
                {
                    exito = true,
                    mensaje = $"¡Venta #{idVentaGenerado} registrada! Subtotal: Bs. {venta.Subtotal:N2} | Descuento: Bs. {venta.DescuentoAplicado:N2} | Total: Bs. {venta.Total:N2} (Vendedor ID: {venta.VendedorId})"
                });
            }
            catch (DescuentoInvalidoException ex)
            {
                return UnprocessableEntity(new { exito = false, mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error interno al guardar la venta: " + ex.Message });
            }
        }
    }
}