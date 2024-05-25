using Kebab.Managers;
using KebabClient.Managers;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;
using static KebabClient.Managers.WalletManager;

namespace KebabClient.Controllers;
public class WalletController(WalletManager walletManager, MinerManager minerManager): Controller
{
    private KebabClient.Managers.TransactionManager transactionManager;

    [HttpGet]
    public async Task<char[]> GetKey([FromQuery] string key)
    {
        return await walletManager.ReadKey(key == "private" ? Key.Private : Key.Public);
    }

    [HttpGet]
    public async Task<Tuple<string,string>> CreateWallet()
    {
        Tuple<string, string> keys =  await walletManager.CreateWallet();
        // transactionManager = new KebabClient.Managers.TransactionManager(blockChainManager,minerManager,walletManager);
        return keys;
    }
}