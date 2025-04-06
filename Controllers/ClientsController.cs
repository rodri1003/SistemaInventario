using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaInventario.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clients/Index - Lista de clientes
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.ToListAsync();
            return View(clients);
        }

        // GET: Clients/Details/5 - Detalles de un cliente
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Incluye las facturas asociadas para ver el historial de compras
            var client = await _context.Clients
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create - Formulario para crear un cliente
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create - Recibe los datos del formulario y crea el cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Edit/5 - Formulario para editar un cliente
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5 - Recibe datos del formulario para actualizar el cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.ClientId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5 - Confirmación para eliminar un cliente
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5 - Elimina el cliente
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/History/5 - Muestra el historial de compras y estado de cuenta del cliente
        public async Task<IActionResult> History(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Incluye las facturas para mostrar el historial
            var client = await _context.Clients
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}
