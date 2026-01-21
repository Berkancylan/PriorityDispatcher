using PriorityDispatcher.Contracts.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PriorityDispatcher.Services.Encryption
{
    public class AesEncryptionService : IEncryptionService
    {
        private const string Key = "MasterKey";
        private const string Iv = "IV";

        public string Encryption(string plainText)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                using (Aes myEas = Aes.Create())
                {
                    byte[] key = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(Key));
                    byte[] _32ByteIvHash = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(Iv));
                    byte[] _16ByteIvHash = new byte[16];
                    Array.Copy(_32ByteIvHash, _16ByteIvHash, 16);

                    myEas.Key = key;
                    myEas.IV = _16ByteIvHash;

                    ICryptoTransform encryptor = myEas.CreateEncryptor(myEas.Key, myEas.IV);

                    using (MemoryStream myMemoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(myMemoryStream, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);


                        }
                        return Convert.ToBase64String(myMemoryStream.ToArray());
                    }
                }
            }
        }
        public string Decrypt(string cipherText)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                using (Aes myEas = Aes.Create())
                {
                    byte[] key = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(Key));
                    byte[] _32ByteIvHash = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(Iv));
                    byte[] _16ByteIvHash = new byte[16];
                    Array.Copy(_32ByteIvHash, _16ByteIvHash, 16);

                    myEas.Key = key;
                    myEas.IV = _16ByteIvHash;

                    ICryptoTransform decryptor = myEas.CreateDecryptor();
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);

                    using (MemoryStream myMemoryStream = new MemoryStream(cipherBytes))
                    using (CryptoStream cryptoStream = new CryptoStream(myMemoryStream, decryptor, CryptoStreamMode.Read))
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }

        }
    }
}
