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

        // Propiedad para notas o comentarios adicionales del cliente.
        public string? Notes { get; set; }

        // Indica si el cliente tiene deudas pendientes: true = deudor, false = solvente.
        public bool IsDebtor { get; set; }

        // Historial de compras (facturas) asociadas al cliente.
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();

        // Balance pendiente del cliente.
        public decimal OutstandingBalance { get; set; }
    }
}
