using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace SistemaInventario.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [BindNever]
        [Required]
        [StringLength(20)]
        public string InvoiceCode { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetProfit { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal GlobalDiscountPercentage { get; set; }

        public bool IsPaid { get; set; }

        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        public List<InvoiceItem> LineItems { get; set; } = new();
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercent { get; set; } = 0;

        [NotMapped]
        public decimal Subtotal => Quantity * Price;

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
