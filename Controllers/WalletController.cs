using Kebab.Managers;
using KebabClient.Managers;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;
using static KebabClient.Managers.WalletManager;

namespace KebabClient.Controllers;
public class WalletController(WalletManager walletManager): Controller
{
    [HttpGet]
    public async Task<string> GetKey([FromQuery] string key)
    {
        // if manager returns specific exceptions you can catch here to return better errors
        return new string(await walletManager.ReadKey(key == "private" ? Key.Private : Key.Public));
    }

    [HttpGet]
    public async Task<Tuple<string,string>> CreateWallet()
    {
        Tuple<string, string> keys =  await walletManager.CreateWallet();
        return keys;
    }

    
}