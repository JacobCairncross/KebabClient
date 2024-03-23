using Kebab.Models;

namespace KebabClient.Models;
public class TransactionOutputWrapper(int blockId, string txId, int outputIndex, TransactionOutput output)
{
    public int Blockid => blockId;
    public string TxId => txId;
    public int OutputIndex => outputIndex;
    public TransactionOutput TxOut => output;
} 