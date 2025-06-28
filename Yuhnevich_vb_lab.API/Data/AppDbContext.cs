using Microsoft.EntityFrameworkCore;
using Yuhnevich_vb_lab.Domain.Entities;

namespace Yuhnevich_vb_lab.API.Data
{
    public class AppDbContext : DbContext
    {
        // Свойства DbSet для сущностей предметной области
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Category> Categories { get; set; }

        // Конструктор, принимающий DbContextOptions<AppDbContext>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}