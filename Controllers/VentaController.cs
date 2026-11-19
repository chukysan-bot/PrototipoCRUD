using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaDAO _ventaDAO;

        public VentaController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _ventaDAO = new VentaDAO(connectionString);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Venta());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Venta venta, decimal porcentajeDescuento = 0)
        {
            try
            {
                venta.AplicarDescuento(porcentajeDescuento);

                if (string.IsNullOrWhiteSpace(venta.Vendedor))
                {
                    venta.Vendedor = User?.Identity?.Name is { Length: > 0 } name
                        ? name
                        : "Vendedor General";
                }

                _ventaDAO.RegistrarVenta(venta);
                TempData["MensajeExito"] = $"Venta registrada. Total final: {venta.TotalFinal:0.00} (Descuento aplicado: {venta.PorcentajeDescuento}%).";
                return RedirectToAction(nameof(Create));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(venta);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"No se pudo registrar la venta: {ex.Message}");
                return View(venta);
            }
        }
    }
}