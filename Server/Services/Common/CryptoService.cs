using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Server.Settings;

namespace Server.Services.Common
{
    public interface ICryptoService
    {
        // Password Hashing (One-Way)
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);

        // Data Encryption (Two-Way)
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
    public class CryptoService : ICryptoService
    {
        private readonly string _key;
        private readonly string _iv;
        private readonly PasswordHasher<string> _passwordHasher;

        public CryptoService(IOptions<EncryptionSettings> settings)
        {
            _key = settings.Value.Key;
            _iv = settings.Value.IV;
            _passwordHasher = new PasswordHasher<string>();
        }

        // Password Hashing (One-Way) using Microsoft PasswordHasher
        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
