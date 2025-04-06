using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
// Asegúrate de tener esta using
using Rotativa.AspNetCore;
using OfficeOpenXml;


var builder = WebApplication.CreateBuilder(args);

// EPPlus 8: nueva forma de configurar la licencia


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Aquí configuras Rotativa:
Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");
// ↑↑↑ Ajusta la ruta a donde tengas tus binarios wkhtmltopdf.exe y wkhtmltox.dll

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
