using ShopSphere.DatabaseModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// MVC SERVICES

builder.Services.AddControllersWithViews();


// DATABASE (ShoppDbContext ONLY)

builder.Services.AddDbContext<ShoppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// SESSION CONFIGURATION

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// PIPELINE CONFIG

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: SESSION MUST BE BEFORE AUTH
app.UseSession();

app.UseAuthorization();


// DEFAULT ROUTE
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
