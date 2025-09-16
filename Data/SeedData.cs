using OrderManagementAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderManagementAPI.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Products.AnyAsync())
            {
                return; // Database already seeded
            }

            var random = new Random();
            var categories = new[] { "Elektronik", "Giyim", "Kitap", "Ev & Yaşam", "Spor", "Oyuncak", "Kozmetik", "Gıda" };
            var units = new[] { "Adet", "Kg", "Litre", "Metre", "Paket" };

            var products = new List<Product>();

            for (int i = 1; i <= 1000; i++)
            {
                var category = categories[random.Next(categories.Length)];
                var unit = units[random.Next(units.Length)];

                products.Add(new Product
                {
                    Description = $"{category} Ürünü {i}",
                    Category = category,
                    Unit = unit,
                    UnitPrice = Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2),
                    Status = random.Next(100) < 95,
                    CreateDate = DateTime.UtcNow.AddDays(-random.Next(365)),
                    //son 1 yıl içinde random bir CreateDate
                    UpdateDate = random.Next(2) == 0 ? DateTime.UtcNow.AddDays(-random.Next(30)) : null
                    //Eğer 0 ise → son 30 gün içinde random bir tarih atar.Eğer 1 ise → null bırakır.
                    //bazı ürünler güncellenmiş, bazıları hiç güncellenmemiş olacak.
                });
            }

            context.Products.AddRange(products);
            //EF Core’a bu liste içindeki tüm Product objelerini DB’ye eklemeyi hazırla diyo.
            //Ama henüz DB’ye yazmaz, sadece change tracker içine ekler.
            await context.SaveChangesAsync();
            //Change Tracker’daki tüm pending değişiklikleri tek seferde DB’ye gönderir.
        }
    }
}
