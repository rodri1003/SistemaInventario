using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Notes { get; set; }

        public bool IsDebtor { get; set; }

        public List<Invoice> Invoices { get; set; } = new List<Invoice>();

        public decimal OutstandingBalance { get; set; }
    }
}
