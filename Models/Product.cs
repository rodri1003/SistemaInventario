using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventario.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor que 0")]
        [Display(Name = "Precio de Compra")]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor que 0")]
        [Display(Name = "Precio de Venta")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
        public int Quantity { get; set; }

        [ForeignKey("Category")]
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
