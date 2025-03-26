using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using TaskSchedular.Data;
using TaskSchedular.Interface;
using TaskSchedular.Repository;
 
var builder = WebApplication.CreateBuilder(args);
 
// Load environment variables for production deployment
var env = builder.Environment;
var configuration = builder.Configuration;
 
// Add services to the container.
builder.Services.AddControllersWithViews();
 
// Configure Database Connection with environment variable fallback
var connectionString = configuration.GetConnectionString("DBConnection") 
                       ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
 
builder.Services.AddDbContext<TaskContext>(options => options.UseSqlServer(connectionString));
 
// Register services
builder.Services.AddScoped<ITasks, TaskRepository>();
 
// Configure CORS policies (restrict in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://your-production-url.com") // Restrict for production
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
 
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});
 
var app = builder.Build();
 
// Configure middleware based on environment
app.UseDeveloperExceptionPage();
 
 
// Enable Forwarded Headers for proxy support (useful for Docker, Nginx, IIS)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
 
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
 
// Use specific CORS policy
app.UseCors("AllowSpecificOrigin");
 
// Endpoint Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Task}/{action=Index}/{id?}");
 
// Run the app
app.Run();
