using EcomWebsite.Models;
using EcomWebsite.Repositories.Implementations;
using EcomWebsite.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using EcomWebsite.Services.Interfaces;
using EcomWebsite.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Views
builder.Services.AddControllersWithViews();

// DB Context
builder.Services.AddDbContext<myContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("myconnection")));

// ✅ REPOSITORY LAYER
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// ✅ SERVICE LAYER (we will create next)
builder.Services.AddScoped<IProductService, ProductService>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ SESSION MUST BE HERE
app.UseSession();

// ⚠️ AUTH (OK to keep even if not using Identity)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customer}/{action=Index}/{id?}");

app.Run();
