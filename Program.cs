using KebabClient.Managers;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Kebab.Managers;
using KebabClient.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
Options options = new();
builder.Configuration.GetSection("Options").Bind(options);
builder.Services.AddSingleton<Options>(options);
Kebab.Models.Options blockChainOptions = new(){
    GenesisPubKey= builder.Configuration.GetValue<string>("Options:PublicKeyPath")
};
// Find out why singleton lets us do this but not scoped
builder.Services.AddSingleton<Kebab.Models.Options>(blockChainOptions);
KnownMiners miners = new(){
    miners = builder.Configuration.GetSection("KnownMiners").Get<string[]>()
};
builder.Services.AddSingleton<KnownMiners>(miners);
builder.Services.AddSingleton<BlockChainManager>();
builder.Services.AddSingleton<WalletManager>();
builder.Services.AddSingleton<MinerManager>();
builder.Services.AddSingleton<KebabClient.Managers.TransactionManager>();
// Think this is better served as singleton as it seems 'expensive' to build for each request
builder.Services.AddHttpClient();

// builder.Configuration.GetSection("KnownMiners").Get<List<string>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapGet("/hi", () => "Hello!");

app.MapDefaultControllerRoute();
app.MapControllerRoute(
        name: "Default",
        pattern: "{controller}/{action}",
        defaults: new { controller = "Home", action = "Index" }
    );
app.MapRazorPages();

app.Run();