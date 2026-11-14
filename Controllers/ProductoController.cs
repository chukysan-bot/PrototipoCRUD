using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ProductoDAO _dao = new ProductoDAO();
        private readonly ProveedorDAO _proveedorDao = new ProveedorDAO();

        private void CargarProveedores(int? seleccionado = null)
        {
            ViewBag.Proveedores = new SelectList(_proveedorDao.ObtenerTodos(), "Id", "Nombre", seleccionado);
        }

        // READ (lista)
        public IActionResult Index()
        {
            return View(_dao.ObtenerTodos());
        }

        // CREATE (formulario)
        public IActionResult Create()
        {
            CargarProveedores();

            var productoPorDefecto = new Producto
            {
                Stock = 1,         
                StockMinimo = 5     
            };
            return View(productoPorDefecto);
        }

        // CREATE (guardar)
        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                CargarProveedores(producto.ProveedorId);
                return View(producto);
            }
            try
            {
                _dao.Insertar(producto);
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError("CodigoBarras", "Ya existe un producto con ese código de barras");
                CargarProveedores(producto.ProveedorId);
                return View(producto);
            }
            return RedirectToAction("Index");
        }

        // UPDATE (formulario)
        public IActionResult Edit(int id)
        {
            var producto = _dao.ObtenerPorId(id);
            if (producto == null) return NotFound();
            CargarProveedores(producto.ProveedorId);
            return View(producto);
        }

        // UPDATE (guardar)
        [HttpPost]
        public IActionResult Edit(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                CargarProveedores(producto.ProveedorId);
                return View(producto);
            }
            try
            {
                _dao.Actualizar(producto);
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError("CodigoBarras", "Ya existe un producto con ese código de barras");
                CargarProveedores(producto.ProveedorId);
                return View(producto);
            }
            return RedirectToAction("Index");
        }

        // DELETE (confirmación)
        public IActionResult Delete(int id)
        {
            var producto = _dao.ObtenerPorId(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        // DELETE (ejecutar)
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _dao.Eliminar(id);
            return RedirectToAction("Index");
        }
    }
}
