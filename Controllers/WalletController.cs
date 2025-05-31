using System.Threading.Tasks;
using Kebab.Managers;
using KebabClient.Managers;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using static KebabClient.Managers.WalletManager;

namespace KebabClient.Controllers;

public class WalletController(WalletManager walletManager, Managers.TransactionManager transactionManager) : Controller
{

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string pubKey = await GetKey("public");
        ViewData["PublicKey"] = pubKey;
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(pubKey, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            ViewData["PublicKeyPng"] = "data:image/png;base64," + Convert.ToBase64String(qrCodeImage);
        }
        // ViewData["Balance"] = (await transactionManager
        //     .GetAllUnspentTransactions(pubKey.ToCharArray()))
        //     .Sum(t => t.Value);
        return View();
    }

    [HttpGet]
    public async Task<string> GetKey([FromQuery] string key)
    {
        // if manager returns specific exceptions you can catch here to return better errors
        return new string(await walletManager.ReadKey(key == "private" ? Key.Private : Key.Public));
    }

    [HttpGet]
    public async Task<Tuple<string, string>> CreateWallet()
    {
        Tuple<string, string> keys = await walletManager.CreateWallet();
        return keys;
    }

    [HttpPost]
    public async Task<bool> Send([FromBody] TransactionDTO transaction)
    {
        // List<Tuple<string, int>> outputs = transaction.Outputs.Select(o => new Tuple<string,int>(o.PublicKey, o.Value)).ToList();
        Console.WriteLine(transaction.ToString());
        return await transactionManager.SpendTransactions(transaction);
    }
    
    // [HttpPost]
    // public async Task<bool> Send([FromBody] byte[] transaction)
    // {
    //     // List<Tuple<string, int>> outputs = transaction.Outputs.Select(o => new Tuple<string,int>(o.PublicKey, o.Value)).ToList();
    //     return await transactionManager.SpendTransactions(transaction);
    // }
}