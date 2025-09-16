using System.ComponentModel.DataAnnotations;//property’lere validation ve DB kuralları eklememizi sağlar.
using System.ComponentModel.DataAnnotations.Schema;//DB kolon tipini belirtmemizi sağlar

namespace OrderManagementAPI.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public bool Status { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }//Nullable
        //Buna required koymamamıza rağmen nullable olmaz çünkü bu bi value type(struct) olduğu için otomatik null olamaz.
    
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        //virtual EF Core’da lazy loading için kullanılır.
    }
}