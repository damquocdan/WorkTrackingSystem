using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AppDBConnection") ?? throw new InvalidOperationException("Connection string 'AppDBConnection' not found.");
builder.Services.AddDbContext<WorkTrackingSystemContext>(options =>
    options.UseSqlServer(connectionString));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(3600);  // Thời gian session hết hạn
    options.Cookie.HttpOnly = true;  // Cookie chỉ có thể truy cập qua HTTP
    options.Cookie.IsEssential = true;  // Cookie là cần thiết cho ứng dụng
    options.Cookie.Name = "WorkTrackingSystem";  // Tên cookie
});
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
