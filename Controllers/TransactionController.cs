using Kebab.Managers;
using KebabClient.Managers;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace KebabClient.Controllers;
public class TransactionController(Managers.TransactionManager transactionManager): Controller
{
    // private KebabClient.Managers.TransactionManager transactionManager;
    [HttpPost]
    public async Task<string> Test([FromBody] TestModel testVar)
    {
        // byte[] buff = new byte[50];
        // var request = this.HttpContext.Request.Body.ReadAsync(buff, 0, (int)this.HttpContext.Request.Body.Length);
        // Console.WriteLine(buff);
        var bodyStream = new StreamReader(HttpContext.Request.Body);
        // bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
        // bodyStream.
        // bodyStream.Seek(0, SeekOrigin);
        var bodyText = await bodyStream.ReadToEndAsync();
        return bodyText;
    }

    [HttpPost]
    public async Task<bool> Send([FromBody] TransactionDTO transaction)
    {
        // List<Tuple<string, int>> outputs = transaction.Outputs.Select(o => new Tuple<string,int>(o.PublicKey, o.Value)).ToList();
        return await transactionManager.SpendTransactions(transaction);
    }

    // [HttpGet]
    // public async Task<int> GetBalance()
    // {
    //     return (await transactionManager.GetAllUnspentTransactions()).Select(o => o.Value).Sum();
    // }
}