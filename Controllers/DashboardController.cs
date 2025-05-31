using Kebab.Managers;
using KebabClient.Managers;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace KebabClient.Controllers;
public class DashboardController(Managers.TransactionManager transactionManager): Controller
{
    [HttpPost]
    public async Task<string> Test([FromBody] TestModel testVar)
    {
        var bodyStream = new StreamReader(HttpContext.Request.Body);
        var bodyText = await bodyStream.ReadToEndAsync();
        return bodyText;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}