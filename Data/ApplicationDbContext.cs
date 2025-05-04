using System;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Models;

namespace SistemaInventario.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sembrado de categorías
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Textiles" },
                new Category { CategoryId = 2, Name = "Cerámicas" },
                new Category { CategoryId = 3, Name = "Artesanías" },
                new Category { CategoryId = 4, Name = "Costura" },
                new Category { CategoryId = 5, Name = "Varios" }
            );

            // Configurar decimales en Product
            modelBuilder.Entity<Product>().Property(p => p.PurchasePrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.SalePrice).HasColumnType("decimal(18,2)");

            // Productos
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Tela bordada",
                    PurchasePrice = 2.50m,
                    SalePrice = 4.00m,
                    RegistrationDate = new DateTime(2025, 3, 22),
                    Quantity = 10,
                    CategoryId = 1
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Jarrón de barro",
                    PurchasePrice = 3.00m,
                    SalePrice = 5.50m,
                    RegistrationDate = new DateTime(2025, 3, 22),
                    Quantity = 5,
                    CategoryId = 2
                }
            );

            // Relación opcional entre Invoice y Client
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.SetNull); 

            // Configurar decimales en Invoice
            modelBuilder.Entity<Invoice>().Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Invoice>().Property(i => i.NetProfit).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Invoice>().Property(i => i.GlobalDiscountPercentage).HasColumnType("decimal(5,2)");

            // Configurar decimales en InvoiceItem
            modelBuilder.Entity<InvoiceItem>().Property(ii => ii.Price).HasColumnType("decimal(18,2)");

            // Configurar decimales en Client
            modelBuilder.Entity<Client>().Property(c => c.OutstandingBalance).HasColumnType("decimal(18,2)");
        }
    }
}
