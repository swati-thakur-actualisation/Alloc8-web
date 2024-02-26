using Alloc8_web.Services.Admin;
using Alloc8_web.Services.Azure;
using Alloc8_web.Services.User;
using Alloc8_web.Utilities;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Alloc8.ef;
using Alloc8.ef.Entities.Dashboard;
using Alloc8_web.Utilities.Formatter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Alloc8DbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<Alloc8DbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Set the login page as the default route
    options.AccessDeniedPath = "/Account/AccessDenied"; // Customize access denied path
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
});

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddTransient<IUserService, UserService>();

// Adding dependency for AZURE BLOB
builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobConnectionString")));
builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();

// Adding dependency for AH ADMINUSER
builder.Services.AddTransient<IAdminUserService, AdminUserService>();

// Adding dependency for REALI 
//builder.Services.AddTransient<IRealiService, RealiService>();

builder.Services.AddScoped<Formatter>(provider =>
{
    var formatter = ActivatorUtilities.CreateInstance<Formatter>(provider);
    var auth = provider.GetService<IAuth>();
    if (auth?.user() != null)
    {
        formatter.byUser(auth.user());
    }
    return formatter;
});

builder.Services.AddScoped<IAuth, Auth>();
builder.Services.AddScoped<IUserService, UserService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();
DataSeeding();

app.Run();
void DataSeeding()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.
            GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}