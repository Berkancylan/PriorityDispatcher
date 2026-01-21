
namespace PriorityDispatcher.Contracts.Interfaces
{
    public interface IEncryptionService
    {
        string Encryption(string plainText);
        string Decrypt(string cipherText);
    }
}
