using System.Security.Cryptography;
using KebabClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace KebabClient.Managers;

public class WalletManager(Options options)
{
    private Options _options => options;
    public enum Key
    {
        Public,
        Private
    }

    public async Task<Tuple<string, string>> CreateWallet()
    {
        RSA rsa = RSA.Create();
        Console.WriteLine(rsa.ExportRSAPublicKeyPem());
        Console.WriteLine(rsa.ExportPkcs8PrivateKeyPem());
        Task publicWriteFinished = File.WriteAllTextAsync("./PublicKey", rsa.ExportRSAPublicKeyPem());
        Task privateWriteFinished = File.WriteAllTextAsync("./PrivateKey", rsa.ExportRSAPrivateKeyPem());
        await Task.WhenAll(privateWriteFinished, publicWriteFinished);
        Console.WriteLine("Your wallet has been made, pls check your files");
        return new Tuple<string, string>(rsa.ExportRSAPublicKeyPem(), rsa.ExportRSAPrivateKeyPem());
    }

    public async Task<char[]> ReadKey(Key key)
    {
        string? keyPath = key == Key.Public ? _options.publicKeyPath : _options.privateKeyPath;
        if (keyPath is null)
        {
            throw new Exception("Key path is null");
        }
        if (File.Exists(keyPath))
        {
            return (await File.ReadAllTextAsync(keyPath)).ToCharArray();
        }
        else
        {
            throw new Exception("File does not exist, generate wallet before attempting to read");
        }
    }
    
}