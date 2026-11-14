using Microsoft.AspNetCore.Mvc;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly ProveedorDAO _dao = new ProveedorDAO();

        // READ (lista)
        public IActionResult Index()
        {
            return View(_dao.ObtenerTodos());
        }

        // CREATE (formulario)
        public IActionResult Create()
        {
            return View();
        }

        // CREATE (guardar)
        [HttpPost]
        public IActionResult Create(Proveedor proveedor)
        {
            if (!ModelState.IsValid) return View(proveedor);
            _dao.Insertar(proveedor);
            return RedirectToAction("Index");
        }

        // UPDATE (formulario)
        public IActionResult Edit(int id)
        {
            var proveedor = _dao.ObtenerPorId(id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        // UPDATE (guardar)
        [HttpPost]
        public IActionResult Edit(Proveedor proveedor)
        {
            if (!ModelState.IsValid) return View(proveedor);
            _dao.Actualizar(proveedor);
            return RedirectToAction("Index");
        }

        // DELETE (confirmación)
        public IActionResult Delete(int id)
        {
            var proveedor = _dao.ObtenerPorId(id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
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
