using System.Net.Http;
using System.Text.Json;
using System.Transactions;
using Kebab.Models;
using KebabClient.Models;

namespace KebabClient.Managers;
public class MinerManager(KnownMiners knownMiners, IHttpClientFactory httpClientFactory)
{
    public async Task<bool> TransmitToMiners(Kebab.Models.Transaction transaction)
    {
        // JsonContent transactionContent = JsonContent.Create(JsonSerializer.Serialize(transaction));
        List<Task<Tuple<int,string>>> responseTasks = new();
        using(HttpClient client = httpClientFactory.CreateClient())
        foreach(var miner in knownMiners.miners)
        {
            // Arguably better to just let full url be put in for more flexability but
            // Im not too bothered about that
            Console.WriteLine(miner);
            responseTasks.Add(TransmitToMiner(miner, transaction));
        }
        Console.WriteLine(responseTasks.Count);
        List<Tuple<int,string>> responses = new();
        Console.WriteLine(JsonSerializer.Serialize(transaction));
        try{
            responses = (await Task.WhenAll(responseTasks)).ToList();
        }
        catch(TaskCanceledException ex){
            // Dont much care if any one of them dies, just spit it out and keep going
            Console.WriteLine($"{ex.Message}, {ex.InnerException}");
        }
        Console.WriteLine(responses.Count);
        // Console.WriteLine(responses);
        // HttpClient testclient = httpClientFactory.CreateClient();
        // var testTrans = await testclient.PostAsync($"http://localhost:5000/BlockChain/Transaction", transactionContent);
        // Console.WriteLine(testTrans.StatusCode);
        if(responses.All(r => !(r.Item1 == 200)))
        {
            // Add a logger here
            Console.WriteLine("All transmissions failed. Please update your list of miners and try again");
            return false;
        }
        Console.WriteLine("Transmitted transaction");
        return true;
    }

    private async Task<Tuple<int,string>> TransmitToMiner(string miner, Kebab.Models.Transaction transaction)
    {
        using(HttpClient client = httpClientFactory.CreateClient())
        {
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync($"{miner}/BlockChain/Transaction", transaction);
                // HttpResponseMessage response = await client.PostAsync($"{miner}/BlockChain/Transaction", transactionContent);
                return new Tuple<int, string>(((int)response.StatusCode), await response.Content.ReadAsStringAsync());
            }
            catch(TaskCanceledException ex){
                // Dont much care if any one of them dies, just spit it out and keep going
                Console.WriteLine($"{ex.Message}, {ex.InnerException}");
                return new Tuple<int,string>(500, "Request Cancelled");
            }
        }
    }
}