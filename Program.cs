using Microsoft.EntityFrameworkCore;
using ASP.NET_Project.Models;
using ASP.NET_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession();

// Add the DbContext with SQL Server configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Register EmailService with the required parameters
builder.Services.AddSingleton<EmailService>(provider =>
    new EmailService(
        "sandbox875b5aa93b5444958fc19792587a17ba.mailgun.org", // Domain
        "c4599b8d4cf907b44da1c61f1e19c472-3724298e-24e3a09a", // API Key
        "YourApp <mailgun@sandbox875b5aa93b5444958fc19792587a17ba.mailgun.org>" // From Email
));

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ensure UseRouting comes before UseSession
app.UseRouting();

// Enable session handling
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
