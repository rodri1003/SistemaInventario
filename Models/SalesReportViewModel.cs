namespace SistemaInventario.Models
{
    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int InvoiceCount { get; set; }
        public decimal NetProfit { get; set; }
    }
}
