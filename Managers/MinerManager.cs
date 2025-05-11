using System.Net.Http;
using System.Text.Json;
using System.Transactions;
using Kebab.Models;
using KebabClient.Models;

namespace KebabClient.Managers;
public class MinerManager(KnownMiners knownMiners, IHttpClientFactory httpClientFactory)
{
    private KnownMiners _knownMiners => knownMiners;
    private IHttpClientFactory _httpClientFactory => httpClientFactory;
    public async Task<List<Block>> GetChain()
    {
        // Get chain from every miner and return largest one
        // This could be slow for large amounts of known miners, maybe restrict in future to only search a few
        // TODO: parallelise this 
        List<List<Block>> chains = [];
        using(HttpClient client = httpClientFactory.CreateClient())
        {
            foreach(var miner in _knownMiners.miners)
            {
                try 
                {
                    HttpResponseMessage response = await client.GetAsync($"{miner}/BlockChain/Chain");
                    chains.Add(JsonSerializer.Deserialize<List<Block>>(await response.Content.ReadAsStringAsync()) ?? new());
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("==============+=================+");
                    Console.WriteLine(e.InnerException);
                }
            }
        }
        return chains.MaxBy(c => c.Count) ?? [];
    }

    public async Task<bool> TransmitToMiners(TransactionRequest transaction)
    {
        // JsonContent transactionContent = JsonContent.Create(JsonSerializer.Serialize(transaction));
        // Why is the client being made here?
        List<Task<Tuple<int,string>>> responseTasks = new();
        using(HttpClient client = httpClientFactory.CreateClient())
        foreach(var miner in _knownMiners.miners)
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

    private async Task<Tuple<int,string>> TransmitToMiner(string miner, TransactionRequest transaction)
    {
        using(HttpClient client = httpClientFactory.CreateClient())
        {
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync($"{miner}/BlockChain/Transaction", transaction);
                // HttpResponseMessage response = await client.PostAsync($"{miner}/BlockChain/Transaction", transactionContent);
                return new Tuple<int, string>(((int)response.StatusCode), await response.Content.ReadAsStringAsync());
            }
            catch(TaskCanceledException ex)
            {
                // Dont much care if any one of them dies, just spit it out and keep going
                Console.WriteLine($"{ex.Message}, {ex.InnerException}");
                return new Tuple<int,string>(500, "Request Cancelled");
            }
            catch(HttpRequestException ex)
            {
                Console.WriteLine($"{ex.Message}, {ex.InnerException}");
                return new Tuple<int,string>(500, "Request produced exception");
            }
        }
    }
}