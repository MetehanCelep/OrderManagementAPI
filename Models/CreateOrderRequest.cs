using System.ComponentModel.DataAnnotations;

namespace OrderManagementAPI.Models
{
    public class CreateOrderRequest
    {
        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string CustomerGSM { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "En az bir ürün seçilmelidir")]
        public List<ProductDetail> Products { get; set; } = new List<ProductDetail>();
    }
}