using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemaInventario.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
        public decimal SalePrice { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public int Quantity { get; set; }

        // Relación con la tabla de Categorías
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
