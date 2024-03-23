using System.Net.Http;
using System.Text.Json;
using System.Transactions;
using Kebab.Models;

namespace KebabClient.Managers;
public class MinerManager(IConfiguration configuration, IHttpClientFactory httpClientFactory)
{
    public async Task<bool> TransmitToMiners(Kebab.Models.Transaction transaction)
    {
        List<string>? knownMiners = configuration.GetValue<List<string>>("KnownMiners") ?? new List<string>();
        JsonContent transactionContent = JsonContent.Create(JsonSerializer.Serialize(transaction));
        List<Task<HttpResponseMessage>> responseTasks = new();
        using(HttpClient client = httpClientFactory.CreateClient())
        foreach(var miner in knownMiners)
        {
            responseTasks.Append(client.PostAsync(miner, transactionContent));
        }
        List<HttpResponseMessage> responses = [.. (await Task.WhenAll(responseTasks))];
        if(responses.All(r => r.IsSuccessStatusCode))
        {
            // Add a logger here
            Console.WriteLine("All transmissions failed. Please update your list of miners and try again");
            return false;
        }
        Console.WriteLine("Transmitted transaction");
        return true;
    }
}