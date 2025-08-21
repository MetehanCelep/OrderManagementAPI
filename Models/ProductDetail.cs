using System.ComponentModel.DataAnnotations;

namespace OrderManagementAPI.Models
{
    public class ProductDetail
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan b�y�k olmal�d�r")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den b�y�k olmal�d�r")]
        public int Amount { get; set; }
    }
}