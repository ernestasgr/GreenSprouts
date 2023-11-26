using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Net;
using test.Models;
using Microsoft.EntityFrameworkCore;
using Azure.ResourceManager.Compute;
using Microsoft.AspNetCore.Authentication.Cookies;

var usingVM = true;

var builder = WebApplication.CreateBuilder(args);
VirtualMachineResource vm = null;
if (usingVM)
{
    vm = AzureVM.CreateAzureVM(builder.Configuration.GetConnectionString("VM"));
    vm.PowerOn(Azure.WaitUntil.Completed);
}

void OnShutdown(VirtualMachineResource vm)
{
    if (vm != null)
    {
        vm.Deallocate(Azure.WaitUntil.Completed);
    }
}

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});


builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddSession(options =>
{
    // Set a short timeout for easy testing.
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});





var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/*app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(AppContext.BaseDirectory, "../../../wwwroot", "images")),
    RequestPath = "/images"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(AppContext.BaseDirectory, "../../../wwwroot", "css")),
    RequestPath = "/css"
});*/

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        OnShutdown(vm);
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "text/html";
        var exceptionObject = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionObject != null)
        {
            var errorMessage = $"{exceptionObject.Error.Message}";
            await context.Response.WriteAsync(errorMessage).ConfigureAwait(false);
        }
    });
});

try
{
    app.Run();
}
catch
{
    OnShutdown(vm);
}
