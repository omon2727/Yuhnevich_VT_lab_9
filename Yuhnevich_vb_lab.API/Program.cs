using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Yuhnevich_vb_lab.API.Data;

var builder = WebApplication.CreateBuilder(args);

// �������� �������
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", builder =>
    {
        builder.WithOrigins("https://localhost:7168") // ��������� �� ���� Blazor-����������
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// ��������� ���������
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp"); // ����� UseHttpsRedirection, �� UseAuthorization
app.UseAuthorization();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath, "Images")),
    RequestPath = "/Images"
});

app.MapControllers();

await DbInitializer.SeedData(app);

app.Run();