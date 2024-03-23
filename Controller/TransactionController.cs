using Kebab.Managers;
using KebabClient.Managers;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace KebabClient.Controllers;
public class TransactionController(WalletManager walletManager,BlockChainManager blockChainManager,
                                     MinerManager minerManager): Controller
{
    private KebabClient.Managers.TransactionManager transactionManager;
    [HttpGet]
    public async Task<Tuple<string,string>> CreateWallet()
    {
        Tuple<string, string> keys =  await walletManager.CreateWallet();
        transactionManager = new KebabClient.Managers.TransactionManager(blockChainManager,minerManager,walletManager);
        return keys;
    }


    // TODO: Figure out why this doesnt work
    // just making the test model a string and sending a
    // {"TestVar":"What the fuck"} works but making it an array
    // upsets it. Figure it out for stirngs then you can figure out
    // why complex types arent working for the Send Function

    // IGNORE THAT, figured that shit out for strings, so why do complex types struggle
    // Might be a char[] issue? Cause I'm technically sending a string. 
    // Try changing it / making ANOTHER wrapper class that 
    // changes it to a string to verify it works 
    // Yea thats totally the answer, ughhhhh
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
    public async void Send([FromBody] TransactionDTO transaction)
    {
        List<Tuple<char[], int>> outputs = transaction.Outputs.Select(o => new Tuple<char[],int>(o.PublicKey, o.Value)).ToList();
        _ = transactionManager.SpendTransactions(outputs);
    }
}