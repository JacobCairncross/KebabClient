using Kebab.Models;
using Kebab.Managers;
using System.Security.Cryptography;
using System.Text;
using KebabClient.Models;
using static KebabClient.Managers.WalletManager;
using System.Threading.Tasks;
using System.Linq;

namespace KebabClient.Managers;
public class TransactionManager
{
    private readonly MinerManager _minerManager;
    private readonly WalletManager _walletManager;
    public TransactionManager(MinerManager minerManager,
                                WalletManager walletManager)
    {
        _minerManager = minerManager;
        _walletManager = walletManager;
    }
    // Should it be bool or return an 'error' object giving reason why it failed?
    public async Task<bool> SpendTransactions(TransactionDTO transactionDTO)
    {
        // Get all transaction outputs assigned to this public key
        // (maybe an efficiency to only get enough to spend the desired amount / keep track of last place in chain with unused transactions)
        // Use the first transactions to spend the money

        if(transactionDTO.Outputs is null)
        {
            throw new Exception("No outputs provided");
        }
        
        int outputCost = transactionDTO.Outputs.Sum(x => x.Value);
        List<TransactionOutput> UnspentTransactions = await GetAllUnspentTransactions(await _walletManager.ReadKey(Key.Public));
        List<TransactionOutput> transactionsToSpend = GetTransactionsToSpend(UnspentTransactions, outputCost);
        int walletBalance = transactionsToSpend.Sum(x => x.Value);
        if(walletBalance < outputCost)
        {
            return false;
        }

        List<TransactionInput> transactionInputs = new();
        // Change to a Select
        transactionInputs = (await Task.WhenAll(
            transactionsToSpend.Select(
                async t => new TransactionInput()
                {
                    BlockId = t.BlockId,
                    TransactionId = t.TransactionId,
                    OutputIndex = t.OutputIndex,
                    Signature = await SignOutput(t)
                }
        ))).ToList();

        // Add another output to send remainder back to yourself. 
        // Think bitcoin does it a different way to allow for 'tips' to miners but we'll add that later. Everyone here is obviously altruistic 
        Random rnd = new();
        TransactionProvisionalOutput remainder = new TransactionProvisionalOutput()
        {
            PublicKey = new String(await _walletManager.ReadKey(Key.Public)),
            Value = walletBalance - outputCost,
            Nonce = rnd.Next()
        };


        // TODO: change this to reflect new model or make a new dto
        // maybe check how bitcoin does it?
        // allow using provided nonce
        TransactionRequest transaction = new TransactionRequest(){
            Inputs = transactionInputs,
            Outputs =
            [
                ..transactionDTO.Outputs.Select(o => 
                    new TransactionProvisionalOutput()
                    {
                        PublicKey = o.PublicKey,
                        Value = o.Value,
                        Nonce = rnd.Next()
                    }),
                remainder,
            ]
        };

        // Broadcast it to all known miner nodes
        return await _minerManager.TransmitToMiners(transaction);
        // This is where client will need to be inited with some 'Well known' miners
        // add a manager for this communicating with Miner stuff
    }

    // Could move this into the Blockchain class?
    public async Task<List<TransactionOutput>> GetAllUnspentTransactions(char[] pubKey)
    {
        // Cant instantiate blockchain straight up cause it needs a dbcontext.
        // Maybe do the abstractions idea?
        List<Block> chain = await _minerManager.GetChain();
        // List<TransactionOutputWrapper> outputs = new();
        List<TransactionOutput> outputs = [];
        List<TransactionOutput> spentOutputs = [];
        // I cant math, this totes counts as O(n) where n is the num of transactions.
        // But to avoid double spend 
        foreach (Block block in chain)
        {
            foreach (var transaction in block.Transactions)
            {
                outputs.AddRange(transaction.Outputs.Where(x => x.PublicKey.SequenceEqual(pubKey)));

            }
        }
        // TODO: Actually check if they've been spent, this is just all transactions
        // TODO: Check to see if theres a better way of doing this, cause this seems slow and will just get worse with more transactions
        // maybe clients can have a personal db that marks unspent transactions in a scheduled sync
        foreach (Block block in chain)
        {
            foreach (var transaction in block.Transactions)
            {
                foreach(var input in transaction.Inputs)
                {
                    TransactionOutput output = chain[input.BlockId-1].Transactions.First(t => t.Id == input.TransactionId).Outputs[input.OutputIndex];
                    if(outputs.Contains(output))
                    {
                        spentOutputs.Add(output);
                    }
                }
                // outputs.AddRange(transaction.Outputs.Where(x => x.PublicKey.SequenceEqual(pubKey)));

            }
        }
        return outputs.ExceptBy(
                spentOutputs.Select(o => $"{o.BlockId}.{o.TransactionId}.{o.OutputIndex}"), 
                o => $"{o.BlockId}.{o.TransactionId}.{o.OutputIndex}"
            ).ToList();
    } 

    private List<TransactionOutput> GetTransactionsToSpend(List<TransactionOutput> transactionOutputs, int value)
    {
        int currentCoinTotal = 0;
        return transactionOutputs.TakeWhile(_ => currentCoinTotal < value).ToList();
        // for(int i=0; i<transactionOutputs.Count; i++)
        // {
        //     currentCoinTotal += transactionOutputs[i].Value;
        //     if(currentCoinTotal >= value)
        //     {
        //         return transactionOutputs.Slice(0,i+1);
        //     }
        // }
        // return new List<TransactionOutputWrapper>();
    }

    private async Task<byte[]> SignOutput(TransactionOutput txOut)
    {
        byte[] signedOutput;
        UTF8Encoding encoder = new();
        byte[] output = encoder.GetBytes(txOut.ToString());
        byte[] hmacKey = Encoding.ASCII.GetBytes("garlic sauce");
        using(RSACryptoServiceProvider rsa = new())
        using(HMACSHA256 hmac = new(hmacKey.ToArray()))
        {
            rsa.ImportFromPem(await _walletManager.ReadKey(Key.Private));
            byte[] hashMessage = hmac.ComputeHash(output);
            signedOutput = rsa.SignHash(hashMessage, HashAlgorithmName.SHA256.Name);
        }
        return signedOutput;
    }
}