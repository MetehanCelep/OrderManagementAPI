using System.ComponentModel.DataAnnotations;

namespace OrderManagementAPI.Models
{
    public class ProductDetail
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır")]
        public int Amount { get; set; }
    }
}
