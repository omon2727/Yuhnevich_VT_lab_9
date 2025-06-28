using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yuhnevich_vb_lab.API.Data;
using Yuhnevich_vb_lab.Domain.Entities;
using Yuhnevich_vb_lab.Domain.Models;

namespace Yuhnevich_vb_lab.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DishesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/dishes or api/dishes/page/{pageNo}
        [HttpGet]
        [Route("")] // Обрабатывает /api/dishes
        [Route("page/{pageNo:int?}")] // Обрабатывает /api/dishes/page/2
        public async Task<ActionResult<ResponseData<ListModel<Dish>>>> GetDishesAsync(int pageNo = 1, int pageSize = 3)
        {
            Console.WriteLine($"DishesController.GetDishesAsync: Category=null, PageNo={pageNo}, PageSize={pageSize}");
            var result = new ResponseData<ListModel<Dish>>();
            var data = _context.Dishes
                .Include(d => d.Category);

            int totalItems = await data.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (pageNo < 1)
                pageNo = 1;
            if (pageNo > totalPages && totalPages > 0)
                pageNo = totalPages;

            var items = await data
                .OrderBy(d => d.Id)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var listData = new ListModel<Dish>
            {
                Items = items ?? new List<Dish>(),
                CurrentPage = pageNo,
                TotalPages = totalPages
            };

            result.Data = listData;

            if (totalItems == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Нет объектов в базе данных";
            }

            Console.WriteLine($"DishesController: ItemsCount={items.Count}, CurrentPage={pageNo}, TotalPages={totalPages}");
            return Ok(result);
        }

        // GET: api/dishes/{category}/{pageNo}
        [HttpGet("{category}/{pageNo:int?}")]
        public async Task<ActionResult<ResponseData<ListModel<Dish>>>> GetDishesByCategoryAsync(string category, int pageNo = 1, int pageSize = 3)
        {
            Console.WriteLine($"DishesController.GetDishesByCategoryAsync: Category={category}, PageNo={pageNo}, PageSize={pageSize}");
            var result = new ResponseData<ListModel<Dish>>();
            var data = _context.Dishes
                .Include(d => d.Category)
                .Where(d => d.Category.NormalizedName.ToLower() == category.ToLower());

            int totalItems = await data.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (pageNo < 1)
                pageNo = 1;
            if (pageNo > totalPages && totalPages > 0)
                pageNo = totalPages;

            var items = await data
                .OrderBy(d => d.Id)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var listData = new ListModel<Dish>
            {
                Items = items ?? new List<Dish>(),
                CurrentPage = pageNo,
                TotalPages = totalPages
            };

            result.Data = listData;

            if (totalItems == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Нет объектов в выбранной категории";
            }

            Console.WriteLine($"DishesController: ItemsCount={items.Count}, CurrentPage={pageNo}, TotalPages={totalPages}");
            return Ok(result);
        }

        // GET: api/dishes/id/5
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Dish>> GetDish(int id)
        {
            Console.WriteLine($"DishesController.GetDish: Id={id}");
            var dish = await _context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null)
            {
                return NotFound();
            }

            return dish;
        }

        // POST: api/Dishes
        [HttpPost("json")]
        public async Task<ActionResult<Dish>> PostDishJson([FromBody] Dish dish)
        {
            try
            {
                Console.WriteLine($"DishesController.PostDishJson: Name={dish.Name}, CategoryId={dish.CategoryId}");
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    Console.WriteLine($"DishesController.PostDishJson: ModelState invalid, Errors={string.Join(", ", errors)}");
                    return BadRequest(ModelState);
                }

                if (dish.CategoryId > 0)
                {
                    var category = await _context.Categories.FindAsync(dish.CategoryId);
                    if (category == null)
                    {
                        Console.WriteLine($"DishesController.PostDishJson: CategoryId={dish.CategoryId} not found");
                        return BadRequest("Категория не найдена");
                    }
                }
                else
                {
                    Console.WriteLine("DishesController.PostDishJson: CategoryId is 0");
                    return BadRequest("Категория не выбрана");
                }

                _context.Dishes.Add(dish);
                await _context.SaveChangesAsync();
                Console.WriteLine($"DishesController.PostDishJson: Dish created, Id={dish.Id}");

                return CreatedAtAction(nameof(GetDish), new { id = dish.Id }, dish);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DishesController.PostDishJson: Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Dishes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDish(int id, [FromForm] Dish dish, [FromForm] IFormFile? formFile)
        {
            if (id != dish.Id)
            {
                return BadRequest("ID не совпадает");
            }

            var existingDish = await _context.Dishes.FindAsync(id);
            if (existingDish == null)
            {
                return NotFound();
            }

            existingDish.Name = dish.Name;
            existingDish.Description = dish.Description;
            existingDish.Price = dish.Price;
            existingDish.CategoryId = dish.CategoryId;

            if (formFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "Images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
                existingDish.Image = $"https://localhost:7002/Images/{fileName}";
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Dishes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            _context.Dishes.Remove(dish);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Dishes/{id}/image
        [HttpPost("{id}/image")]
        public async Task<IActionResult> SaveImage(int id, [FromForm] IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    Console.WriteLine($"DishesController.SaveImage: Id={id}, Error=Файл изображения не предоставлен");
                    return BadRequest("Файл изображения не предоставлен");
                }

                var dish = await _context.Dishes.FindAsync(id);
                if (dish == null)
                {
                    Console.WriteLine($"DishesController.SaveImage: Id={id}, Error=Блюдо не найдено");
                    return NotFound("Блюдо с указанным ID не найдено");
                }

                var imagesPath = Path.Combine(_environment.WebRootPath, "Images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                    Console.WriteLine($"DishesController.SaveImage: Created directory {imagesPath}");
                }

                var randomName = Path.GetRandomFileName();
                var extension = Path.GetExtension(image.FileName);
                var fileName = Path.ChangeExtension(randomName, extension);
                var filePath = Path.Combine(imagesPath, fileName);

                Console.WriteLine($"DishesController.SaveImage: Saving file to {filePath}");
                using (var stream = System.IO.File.OpenWrite(filePath))
                {
                    await image.CopyToAsync(stream);
                }

                var host = "https://" + Request.Host;
                var url = $"{host}/Images/{fileName}";
                dish.Image = url;
                await _context.SaveChangesAsync();

                Console.WriteLine($"DishesController.SaveImage: Id={id}, ImageUrl={url}");
                return Ok(new { ImageUrl = url });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DishesController.SaveImage: Id={id}, Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}