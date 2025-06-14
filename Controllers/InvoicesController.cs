using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaInventario.Services;

namespace SistemaInventario.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public InvoicesController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string estadoPago)
        {
            var invoicesQuery = _context.Invoices.Include(i => i.Client).AsQueryable();

            if (!string.IsNullOrEmpty(estadoPago))
            {
                if (estadoPago == "pagadas")
                    invoicesQuery = invoicesQuery.Where(i => i.IsPaid);
                else if (estadoPago == "pendientes")
                    invoicesQuery = invoicesQuery.Where(i => !i.IsPaid);
            }

            ViewBag.EstadoPago = estadoPago;
            var invoices = await invoicesQuery.ToListAsync();
            return View(invoices);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            return invoice is null ? NotFound() : View(invoice);
        }

        public IActionResult Create()
        {
            ViewBag.Clientes = new SelectList(_context.Clients, "ClientId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,GlobalDiscountPercentage,IsPaid,LineItems")] Invoice invoice)
        {
            if (invoice.LineItems is null || invoice.LineItems.Count == 0)
                ModelState.AddModelError(string.Empty, "Debes agregar al menos un ítem.");

            // 🔥 Aquí es donde debes colocarlo
            invoice.Date = DateTime.Now;

            var fechaActual = invoice.Date.ToString("yyyyMMdd");
            var contadorDelDia = await _context.Invoices.CountAsync(i => i.Date.Date == invoice.Date.Date) + 1;
            invoice.InvoiceCode = $"FAC-{fechaActual}-{contadorDelDia:D4}";

            // 🧼 Limpia el error si ya fue validado por ModelState antes
            ModelState.Remove("InvoiceCode");

            // ✅ Valida ahora que el código ya existe
            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = new SelectList(_context.Clients, "ClientId", "Name", invoice.ClientId);
                return View(invoice);
            }

            // 💰 Cálculos
            decimal totalLineas = invoice.LineItems.Sum(li => li.Quantity * li.Price * (1 - (li.DiscountPercent ?? 0m) / 100m));
            decimal totalConDesc = totalLineas * (1 - invoice.GlobalDiscountPercentage / 100m);

            invoice.TotalAmount = totalConDesc;
            invoice.NetProfit = totalConDesc * 0.30m;

            _context.Add(invoice);
            await _context.SaveChangesAsync();

            await RecalcularSaldoCliente(invoice.ClientId);

            // ✉️ Envío de correo (opcional)
            var cliente = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == invoice.ClientId);
            if (cliente != null && !string.IsNullOrWhiteSpace(cliente.Email))
            {
                var pdf = GenerateInvoicePdf(invoice);
                await _emailService.SendEmailAsync(
                    cliente.Email,
                    "Tu factura",
                    "Gracias por tu compra. Adjuntamos la factura.",
                    pdf,
                    $"Factura_{invoice.InvoiceId}.pdf"
                );
                TempData["SuccessMessage"] = "Factura creada y enviada correctamente por correo electrónico.";
            }
            else
            {
                TempData["SuccessMessage"] = "Factura creada exitosamente (sin envío por correo).";
            }

            return RedirectToAction("Details", new { id = invoice.InvoiceId });
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var invoice = await _context.Invoices.Include(i => i.LineItems).FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice is null) return NotFound();

            ViewBag.Clientes = new SelectList(_context.Clients, "ClientId", "Name", invoice.ClientId);
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,ClientId,GlobalDiscountPercentage,IsPaid,LineItems")] Invoice invoice)
        {
            if (id != invoice.InvoiceId) return NotFound();

            if (invoice.LineItems is null || invoice.LineItems.Count == 0)
                ModelState.AddModelError(string.Empty, "Debes mantener al menos un ítem.");

            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = new SelectList(_context.Clients, "ClientId", "Name", invoice.ClientId);
                return View(invoice);
            }

            decimal totalLineas = invoice.LineItems.Sum(li => li.Quantity * li.Price * (1 - (li.DiscountPercent ?? 0) / 100m));
            decimal total = totalLineas * (1 - invoice.GlobalDiscountPercentage / 100m);

            invoice.TotalAmount = total;
            invoice.NetProfit = total * 0.30m;

            try
            {
                _context.Update(invoice);
                await _context.SaveChangesAsync();
                await RecalcularSaldoCliente(invoice.ClientId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(invoice.InvoiceId)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var invoice = await _context.Invoices.Include(i => i.Client).FirstOrDefaultAsync(m => m.InvoiceId == id);
            return invoice is null ? NotFound() : View(invoice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices.Include(i => i.LineItems).FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice is not null)
            {
                var clientId = invoice.ClientId;
                _context.InvoiceItems.RemoveRange(invoice.LineItems);
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                await RecalcularSaldoCliente(clientId);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            invoice.IsPaid = true;
            _context.Update(invoice);
            await _context.SaveChangesAsync();

            await RecalcularSaldoCliente(invoice.ClientId);

            TempData["SuccessMessage"] = $"Factura #{id} marcada como pagada.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ReporteVentas()
        {
            var facturas = _context.Invoices
                .Include(i => i.Client)
                .Where(i => i.IsPaid)
                .OrderBy(i => i.Date)
                .ToList();

            var pdf = GenerateReporteVentasPdf(facturas);
            return File(pdf, "application/pdf", "ReporteVentas.pdf");
        }

        public IActionResult ReportePorFecha()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReportePorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
            {
                ModelState.AddModelError("", "La fecha de inicio no puede ser mayor que la fecha de fin.");
                return View();
            }

            var facturas = _context.Invoices
                .Include(i => i.Client)
                .Where(i => i.Date.Date >= fechaInicio.Date && i.Date.Date <= fechaFin.Date)
                .OrderBy(i => i.Date)
                .ToList();

            ViewBag.TotalVentas = facturas.Sum(f => f.TotalAmount);
            ViewBag.TotalGanancias = facturas.Sum(f => f.NetProfit);
            ViewBag.Facturas = facturas;
            ViewBag.FechaInicio = fechaInicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.ToString("yyyy-MM-dd");

            return View();
        }

        public IActionResult ExportPdf(int id)
        {
            var invoice = _context.Invoices.Include(i => i.Client).Include(i => i.LineItems).FirstOrDefault(i => i.InvoiceId == id);
            if (invoice is null) return NotFound();

            var pdf = GenerateInvoicePdf(invoice);
            return File(pdf, "application/pdf", $"Factura_{invoice.InvoiceId}.pdf");
        }

        private async Task RecalcularSaldoCliente(int? clientId)
        {
            if (clientId is null) return;

            var cliente = await _context.Clients
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cliente != null)
            {
                cliente.OutstandingBalance = cliente.Invoices
                    .Where(f => !f.IsPaid)
                    .Sum(f => f.TotalAmount);

                cliente.IsDebtor = cliente.OutstandingBalance > 0;

                _context.Update(cliente);
                await _context.SaveChangesAsync();
            }
        }

        private bool InvoiceExists(int id) => _context.Invoices.Any(e => e.InvoiceId == id);

        private byte[] GenerateReporteVentasPdf(List<Invoice> facturas)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Text("Reporte de Ventas (Facturas Pagadas)")
                                 .SemiBold().FontSize(18).FontColor(Colors.Red.Darken2);

                    page.Content().Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Fecha");
                                header.Cell().Element(CellStyle).Text("Cliente");
                                header.Cell().Element(CellStyle).Text("Total");
                                header.Cell().Element(CellStyle).Text("Ganancia");

                                static IContainer CellStyle(IContainer container) =>
                                    container.Padding(5).Background("#EEE").Border(1).AlignCenter();
                            });

                            int index = 1;
                            foreach (var f in facturas)
                            {
                                table.Cell().Element(CellData).Text(index++.ToString());
                                table.Cell().Element(CellData).Text(f.Date.ToShortDateString());
                                table.Cell().Element(CellData).Text(f.Client?.Name ?? "Sin cliente");
                                table.Cell().Element(CellData).AlignRight().Text(f.TotalAmount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-SV")));
                                table.Cell().Element(CellData).AlignRight().Text(f.NetProfit.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-SV")));
                            }

                            static IContainer CellData(IContainer container) =>
                                container.Padding(3).BorderBottom(1).BorderColor("#DDD");
                        });
                    });

                    page.Footer().AlignCenter().Text("Generado con QuestPDF - SistemaInventario");
                });
            });

            return doc.GeneratePdf();
        }

        private byte[] GenerateInvoicePdf(Invoice invoice)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text($"Factura #{invoice.InvoiceId}")
                                  .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        col.Item().Text($"Código: {invoice.InvoiceCode}")
                                  .FontSize(12).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().Text($"Fecha: {invoice.Date:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Cliente: {invoice.Client?.Name ?? "Invitado"}");
                        col.Item().Text($"Estado de pago: {(invoice.IsPaid ? "Pagada" : "Pendiente")}");

                        col.Item().Text(" ");
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Producto");
                                header.Cell().Element(CellStyle).Text("Cant");
                                header.Cell().Element(CellStyle).Text("Precio");
                                header.Cell().Element(CellStyle).Text("Desc.");
                                header.Cell().Element(CellStyle).Text("Subtotal");

                                static IContainer CellStyle(IContainer container) =>
                                    container.Padding(5).Background("#DDD").Border(1).AlignCenter();
                            });

                            int index = 1;
                            foreach (var item in invoice.LineItems)
                            {
                                var sub = item.Quantity * item.Price * (1 - (item.DiscountPercent ?? 0) / 100m);

                                table.Cell().Element(CellData).Text(index++.ToString());
                                table.Cell().Element(CellData).Text(item.ProductName);
                                table.Cell().Element(CellData).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Element(CellData).AlignRight().Text(item.Price.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-SV")));
                                table.Cell().Element(CellData).AlignRight().Text($"{item.DiscountPercent ?? 0}%");
                                table.Cell().Element(CellData).AlignRight().Text(sub.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-SV")));
                            }

                            static IContainer CellData(IContainer container) =>
                                container.Padding(3).BorderBottom(1).BorderColor("#EEE");
                        });

                        col.Item().Text(" ");
                        var totalFormatted = invoice.TotalAmount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-SV"));
                        col.Item().AlignRight().Text($"Total a pagar: {totalFormatted}");
                    });

                    page.Footer().AlignCenter().Text("Generado automáticamente con QuestPDF - SistemaInventario");
                });
            });

            return doc.GeneratePdf();
        }

    }
}
