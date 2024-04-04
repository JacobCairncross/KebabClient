using Kebab.Managers;
using KebabClient.Managers;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;
using static KebabClient.Managers.WalletManager;

namespace KebabClient.Controllers;
public class WalletController(WalletManager walletManager,BlockChainManager blockChainManager,
                                     MinerManager minerManager): Controller
{
    private KebabClient.Managers.TransactionManager transactionManager;

    [HttpGet]
    public async Task<char[]> GetKey([FromQuery] string key)
    {
        return await walletManager.ReadKey(key == "private" ? Key.Private : Key.Public);
    }
}