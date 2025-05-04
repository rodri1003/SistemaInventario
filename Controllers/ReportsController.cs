using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;
using Rotativa.AspNetCore;            
using OfficeOpenXml;                  
using System.IO;

namespace SistemaInventario.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para imprimir una factura (usando la vista PrintInvoice.cshtml)
        public IActionResult PrintInvoice(int id)
        {
            var invoice = _context.Invoices
                                  .Include(i => i.LineItems)
                                  .FirstOrDefault(i => i.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }

            var pdf = new ViewAsPdf("PrintInvoice", invoice)
            {
                FileName = $"Factura_{invoice.InvoiceId}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type --enable-local-file-access"
            };
            return pdf;
        }

        // GET: Reports/SalesReport
        // Muestra el reporte en la vista web, permitiendo filtrar por rango de fechas.
        public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var invoices = await _context.Invoices
                .Where(i => i.Date >= startDate.Value && i.Date <= endDate.Value)
                .ToListAsync();

            var totalSales = invoices.Sum(i => i.TotalAmount);
            var invoiceCount = invoices.Count;
            var netProfit = invoices.Sum(i => i.NetProfit);

            var report = new SalesReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalSales = totalSales,
                InvoiceCount = invoiceCount,
                NetProfit = netProfit
            };

            return View(report);
        }

        // GET: Reports/ExportToPdf
        // Exporta el reporte a PDF utilizando la vista "SalesReportPrint.cshtml" (diseñada especialmente para impresión)
        public async Task<IActionResult> ExportToPdf(DateTime? startDate, DateTime? endDate)
        {

            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var invoices = await _context.Invoices
                .Where(i => i.Date >= startDate.Value && i.Date <= endDate.Value)
                .ToListAsync();

            var totalSales = invoices.Sum(i => i.TotalAmount);
            var invoiceCount = invoices.Count;
            var netProfit = invoices.Sum(i => i.NetProfit);

            var report = new SalesReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalSales = totalSales,
                InvoiceCount = invoiceCount,
                NetProfit = netProfit
            };

            // Aquí se usa la vista "SalesReportPrint.cshtml", la cual debe tener Layout = null y un diseño propio para el PDF.
            var pdfResult = new ViewAsPdf("SalesReportPrint", report)
            {
                FileName = "SalesReport.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type --enable-local-file-access"
            };

            return pdfResult;
        }

        // GET: Reports/ExportToExcel
        // Exporta el reporte a Excel utilizando EPPlus.
        public IActionResult ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            

            if (!startDate.HasValue)
                startDate = DateTime.Today.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var invoices = _context.Invoices
                .Where(i => i.Date >= startDate.Value && i.Date <= endDate.Value)
                .ToList();
                
            var totalSales = invoices.Sum(i => i.TotalAmount);
            var invoiceCount = invoices.Count;
            var netProfit = invoices.Sum(i => i.NetProfit);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reporte");

                worksheet.Cells["A1"].Value = "Reporte de Ventas y Ganancias";
                worksheet.Cells["A2"].Value = $"Rango: {startDate.Value.ToShortDateString()} - {endDate.Value.ToShortDateString()}";
                worksheet.Cells["A4"].Value = "Total de Ventas";
                worksheet.Cells["B4"].Value = totalSales;
                worksheet.Cells["A5"].Value = "Cantidad de Facturas";
                worksheet.Cells["B5"].Value = invoiceCount;
                worksheet.Cells["A6"].Value = "Ganancia Neta";
                worksheet.Cells["B6"].Value = netProfit;

                var fileContents = package.GetAsByteArray();

                return File(fileContents,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "SalesReport.xlsx");
            }
        }
    }
}
