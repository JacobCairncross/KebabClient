using Kebab.Models;
using Kebab.Managers;
using System.Security.Cryptography;
using System.Text;
using KebabClient.Models;
using static KebabClient.Managers.WalletManager;

namespace KebabClient.Managers;
public class TransactionManager
{
    private readonly BlockChainManager _blockChainManager;
    private readonly MinerManager _minerManager;
    private readonly WalletManager _walletManager;
    public TransactionManager(BlockChainManager blockChainManager, MinerManager minerManager,
                                WalletManager walletManager)
    {
        _blockChainManager = blockChainManager;
        _minerManager = minerManager;
        _walletManager = walletManager;
    }
    // Should it be bool or return an 'error' object giving reason why it failed?
    public async Task<bool> SpendTransactions(List<Tuple<string, int>> outputs)
    {
        // Get all transaction outputs assigned to this public key
        // (maybe an efficiency to only get enough to spend the desired amount / keep track of last place in chain with unused transactions)
        List<TransactionOutputWrapper> UnspentTransactions = GetAllUnspentTransactions(await _walletManager.ReadKey(Key.Public));
        // Use the first transactions to spend the money
        int outputCost = 0;
        foreach(var output in outputs)
        {
            outputCost += output.Item2;
        }

        //Make the outputs here
        Random rnd = new();
        List<TransactionOutput> transactionOutputs = outputs.Select(
            o => new TransactionOutput(){
                PublicKey = o.Item1,
                Value = o.Item2,
                Nonce = rnd.Next()
            }).ToList();
        
        // Transactions to spend will be enough for outputs or it will be empty
        List<TransactionOutputWrapper> transactionsToSpend = GetTransactionsToSpend(UnspentTransactions, outputCost);
        if(transactionsToSpend.Count == 0)
        {
            return false;
        }

        List<TransactionInput> transactionInputs = new();
        foreach(var transactionOutput in transactionsToSpend)
        {
            transactionInputs.Add(new TransactionInput(){
                BlockId = transactionOutput.Blockid,
                txid = transactionOutput.TxId,
                OutputIndex = transactionOutput.OutputIndex,
                Signature = await SignOutput(transactionOutput.TxOut)
            });
        }
        // (might be better to select a few that make the best match - least change - but that doesn't matter if you just make change. Choosing earliest transactions also works better for the speed up of keeping a tracker on the chain)
        // Sign the transactions to make them inputs
        // make a transaction
        Transaction transaction = new Transaction(){
            ID = "Currently a string but maybe make a GUID instead, cause why not?",
            Inputs = transactionInputs.ToArray(),
            Outputs = transactionOutputs.ToArray()
        };

        // Broadcast it to all known miner nodes
        return await _minerManager.TransmitToMiners(transaction);
        // This is where client will need to be inited with some 'Well known' miners
        // add a manager for this communicating with Miner stuff
    }

    // Could move this into the Blockchain class?
    public List<TransactionOutputWrapper> GetAllUnspentTransactions(char[] pubKey)
    {
        BlockChain chain = _blockChainManager.GetChain();
        List<TransactionOutputWrapper> outputs = new();
        // O(n^3) has no love from me, but is there a way to improve it? Maybe with a full network redo
        foreach (var block in chain.chain)
        {
            foreach (var transaction in block.Transactions)
            {
                for(int i=0;i<transaction.Outputs.Length;i++)
                {
                    if(transaction.Outputs[i].PublicKey.SequenceEqual(pubKey))
                    {
                        outputs.Add(new TransactionOutputWrapper(
                            block.BlockId,
                            transaction.ID,
                            i,
                            transaction.Outputs[i]
                        ));
                    }
                }
            }
        }
        return outputs;
    } 

    private List<TransactionOutputWrapper> GetTransactionsToSpend(List<TransactionOutputWrapper> transactions, int value)
    {
        int currentCoinTotal = 0;
        for(int i=0; i<transactions.Count; i++)
        {
            currentCoinTotal += transactions[i].TxOut.Value;
            if(currentCoinTotal >= value)
            {
                return transactions.Slice(0,i+1);
            }
        }
        return new List<TransactionOutputWrapper>();
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