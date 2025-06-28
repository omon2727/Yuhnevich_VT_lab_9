using Microsoft.EntityFrameworkCore;
using Yuhnevich_vb_lab.API.Data;
using Yuhnevich_vb_lab.Domain.Entities;

namespace Yuhnevich_vb_lab.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(WebApplication app)
        {
            var uri = "https://localhost:7002/";

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            if (!context.Categories.Any() && !context.Dishes.Any())
            {
                // Создание категорий
                var categories = new Category[]
                {
            new Category { Name = "Супы", NormalizedName = "soups" }, // Заменяем "Стартеры" на "Супы"
            new Category { Name = "Салаты", NormalizedName = "salads" },
            new Category { Name = "Основные блюда", NormalizedName = "main-dishes" },
            new Category { Name = "Напитки", NormalizedName = "drinks" },
            new Category { Name = "Десерты", NormalizedName = "desserts" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Создание блюд
                var dishes = new List<Dish>
        {
            new Dish 
            { Name = "Суп-харчо", 
                Description = "Острый грузинский суп", 
                Price = 250.00m, 
                Image = uri + "Images/soup-kharcho.jpg", 
                Category = categories.FirstOrDefault(c => c.
                NormalizedName.Equals("soups")), 
                CategoryId = categories.FirstOrDefault(c => c.
                NormalizedName.Equals("soups"))?.Id ?? 0 },
    
                    new Dish { 
                        Name = "Борщ", 
                        Description = "Традиционный украинский суп", 
                        Price = 200.00m, Image = uri + "Images/borsch.jpg", 
                        Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups")),
                        CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups"))?.Id ?? 0 },
    
                    new Dish { 
                        Name = "Крем-суп",
                        Description = "Грибной крем-суп",
                        Price = 220.00m, Image = uri + "Images/cream_soup.jpg",
                        Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups")),
                        CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups"))?.Id ?? 0 },
    
                    new Dish { 
                        Name = "Солянка",
                        Description = "Суп с мясным ассорти", 
                        Price = 270.00m, Image = uri + "Images/solyanka.jpg",
                        Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups")),
                        CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups"))?.Id ?? 0 },
    
                    new Dish { 
                        Name = "Мисо-суп",
                        Description = "Японский суп с мисо-пастой",
                        Price = 180.00m, Image = uri + "Images/miso_soup.jpg",
                        Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups")),
                        CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("soups"))?.Id ?? 0 },

            
            new Dish
            {
                Name = "Цезарь",
                Description = "Салат с курицей, пармезаном и крутонами",
                Price = 350.00m,
                Image = uri + "Images/caesar-salad.jpg",
                Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("salads")),
                CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("salads"))?.Id ?? 0
            },
            new Dish
            {
                Name = "Стейк рибай",
                Description = "Сочный стейк из мраморной говядины",
                Price = 1200.00m,
                Image = uri + "Images/ribeye-steak.jpg",
                Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("main-dishes")),
                CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("main-dishes"))?.Id ?? 0
            },
            new Dish
            {
                Name = "Чизкейк",
                Description = "Классический десерт с ягодным соусом",
                Price = 300.00m,
                Image = uri + "Images/cheesecake.jpg",
                Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("desserts")),
                CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("desserts"))?.Id ?? 0
            },
            new Dish
            {
                Name = "Лимонад",
                Description = "Освежающий напиток с лимоном и мятой",
                Price = 150.00m,
                Image = uri + "Images/lemonade.jpg",
                Category = categories.FirstOrDefault(c => c.NormalizedName.Equals("drinks")),
                CategoryId = categories.FirstOrDefault(c => c.NormalizedName.Equals("drinks"))?.Id ?? 0
            }
        };

                // Проверка, что категории найдены
                foreach (var dish in dishes)
                {
                    if (dish.Category == null)
                    {
                        Console.WriteLine($"Category not found for dish: {dish.Name}");
                    }
                }

                await context.Dishes.AddRangeAsync(dishes);
                await context.SaveChangesAsync();
            }
        }
    }
}