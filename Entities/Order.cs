using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderManagementAPI.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string CustomerGSM { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}