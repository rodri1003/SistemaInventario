using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaInventario.Models;

namespace SistemaInventario.Services
{
    public static class FacturaPDFGenerator
    {
        public static byte[] GenerarFacturaPDF(Invoice factura)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text($"Factura #{factura.InvoiceId}").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Fecha: {factura.Date.ToString("dd/MM/yyyy HH:mm")}");
                        col.Item().Text($"Cliente: {(factura.Client?.Name ?? "Invitado")}");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Producto");
                                header.Cell().Element(CellStyle).AlignRight().Text("Cant.");
                                header.Cell().Element(CellStyle).AlignRight().Text("Precio");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");
                            });

                            int index = 1;
                            foreach (var item in factura.LineItems)
                            {
                                var total = item.Quantity * item.Price * (1 - (item.DiscountPercent ?? 0) / 100m);
                                table.Cell().Element(CellStyle).Text(index++);
                                table.Cell().Element(CellStyle).Text(item.ProductName);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:C}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{total:C}");
                            }
                        });

                        col.Item().PaddingTop(15).Text($"Total a pagar: {factura.TotalAmount:C}").Bold();
                        col.Item().Text($"Ganancia neta (30%): {factura.NetProfit:C}");
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Gracias por su compra. ").SemiBold();
                        txt.Span("SistemaInventario ©");
                    });
                });
            });

            return doc.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(2)
                .PaddingHorizontal(5);
        }
    }
}
