using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Total de la venta sin descuento
        public decimal TotalAmount { get; set; }

        // Ganancia neta calculada (puedes ajustar la fórmula)
        public decimal NetProfit { get; set; }

        // Descuento global en porcentaje (opcional, 0 a 100)
        [Range(0, 100)]
        public decimal GlobalDiscountPercentage { get; set; }

        public bool IsPaid { get; set; }

        // Relación: Cliente que realizó la compra
        [ForeignKey("Client")]
        public int? ClientId { get; set; }  // ← Nullable
        public Client? Client { get; set; }

        // Lista de ítems (detalles) de la factura
        public List<InvoiceItem> LineItems { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser positivo")]
        public decimal Price { get; set; }

        // Propiedad para calcular el subtotal del ítem
        public decimal Subtotal => Quantity * Price;
    }
}
