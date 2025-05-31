using System.Reflection;
using System.Text;
using KebabClient.Controllers;
using KebabClient.Managers;
using KebabClient.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
Options options = new();
builder.Configuration.GetSection("Options").Bind(options);
builder.Services.AddSingleton(options);
// Kebab.Models.Options blockChainOptions = new(){
//     GenesisPubKey= builder.Configuration.GetValue<string>("Options:PublicKeyPath")
// };
// // Find out why singleton lets us do this but not scoped
// builder.Services.AddSingleton(blockChainOptions);
KnownMiners miners = new(){
    miners = builder.Configuration.GetSection("KnownMiners").Get<string[]>()
};
builder.Services.AddSingleton<KnownMiners>(miners);
// builder.Services.AddScoped<BlockChain>();
// builder.Services.AddScoped<BlockChainManager>();
builder.Services.AddScoped<WalletManager>();
builder.Services.AddScoped<MinerManager>();
builder.Services.AddScoped<TransactionManager>();
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
// try
// {
//     //The code that causes the error goes here.
//     app.MapControllers();
// }
// catch (ReflectionTypeLoadException ex)
// {
//     StringBuilder sb = new StringBuilder();
//     foreach (Exception exSub in ex.LoaderExceptions)
//     {
//         sb.AppendLine(exSub.Message);
//         FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
//         if (exFileNotFound != null)
//         {                
//             if(!string.IsNullOrEmpty(exFileNotFound.FusionLog))
//             {
//                 sb.AppendLine("Fusion Log:");
//                 sb.AppendLine(exFileNotFound.FusionLog);
//             }
//         }
//         sb.AppendLine();
//     }
//     string errorMessage = sb.ToString();
//     //Display or log the error based on your application.
//     Console.WriteLine(errorMessage);
// }


// app.MapDefaultControllerRoute();
// app.MapControllerRoute(
//         name: "Default",
//         pattern: "{controller}/{action}",
//         defaults: new { controller = "Dashboard", action = "Index" }
//     );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();