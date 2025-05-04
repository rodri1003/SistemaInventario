using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using Rotativa.AspNetCore;
using OfficeOpenXml;
using SistemaInventario.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 🔽 Aquí registramos el servicio de correo
builder.Services.AddScoped<IEmailService, EmailService>();

// También tu contexto de base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
