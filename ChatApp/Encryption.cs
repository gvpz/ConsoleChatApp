using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;


namespace ChatApp;

public class Encryption
{
    #region Client's RSA
    
    private RSA rsa;
    private RSAParameters rsaKeyInfo;
    
    private string publicKey;
    private string privateKey;
    
    #endregion
    
    #region Chatter's RSA
    
    private RSA chatterRSA;
    
    #endregion
    
    //Constructor creates key 
    public Encryption()
    {
        CreateKey();
    }
    
    //Create RSA keys
    private void CreateKey()
    {
        rsa = RSA.Create();
        rsaKeyInfo = rsa.ExportParameters(true);

        publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
    }

    //Returns the encrypted message
    public byte[] EncryptMessage(string message)
    {
        var messageInBytes = Encoding.UTF8.GetBytes(message);
        var encryptedMessage = chatterRSA.Encrypt(messageInBytes, RSAEncryptionPadding.Pkcs1);

        return encryptedMessage;
    }

    //Returns the decrypted message
    public string DecryptMessage(byte[] message)
    {
        var decryptedData = rsa.Decrypt(message, RSAEncryptionPadding.Pkcs1);
        var decryptedMessage = Encoding.UTF8.GetString(decryptedData);

        return decryptedMessage;
    }

    //Sends public key to other chatter
    public async Task SendPublicKey(TcpClient client)
    {
        var stream = client.GetStream();
        var key = Convert.FromBase64String(publicKey);
        
        Console.WriteLine();
        Console.WriteLine($"Sending Public Key: {key}");
        Console.WriteLine();
        
        await stream.WriteAsync(key);
        
        Console.WriteLine("Sent key");
        Console.WriteLine();
    }

    //Receives public key from other chatter
    public async Task ReceiveKey(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        
        Console.WriteLine();
        Console.WriteLine($"Listening for Public Key");
        Console.WriteLine();
        
        var bytesRead = await stream.ReadAsync(buffer);
        var key = new byte[bytesRead];
        Array.Copy(buffer, 0, key, 0, bytesRead);
        
        Console.WriteLine($"Received Key: {key}");
        Console.WriteLine();
        
        chatterRSA = RSA.Create();
        chatterRSA.ImportRSAPublicKey(key, out _);
    }
}