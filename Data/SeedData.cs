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
            var categories = new[] { "Elektronik", "Giyim", "Kitap", "Ev & Ya�am", "Spor", "Oyuncak", "Kozmetik", "G�da" };
            var units = new[] { "Adet", "Kg", "Litre", "Metre", "Paket" };

            var products = new List<Product>();

            for (int i = 1; i <= 1000; i++)
            {
                var category = categories[random.Next(categories.Length)];
                var unit = units[random.Next(units.Length)];

                products.Add(new Product
                {
                    Description = $"{category} �r�n� {i}",
                    Category = category,
                    Unit = unit,
                    UnitPrice = Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2),
                    Status = random.Next(100) < 95, // %95 aktif �r�n
                    CreateDate = DateTime.Now.AddDays(-random.Next(365)),
                    UpdateDate = random.Next(2) == 0 ? DateTime.Now.AddDays(-random.Next(30)) : null
                });
            }

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}