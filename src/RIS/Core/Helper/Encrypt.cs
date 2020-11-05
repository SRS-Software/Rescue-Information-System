#region

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace RIS.Core.Helper
{
    public static class Encrypt
    {
        public static string EncryptString(string plainText, string key)
        {
            try
            {
                var des = new TripleDESCryptoServiceProvider();
                des.IV = new byte[8];
                var pdb = new PasswordDeriveBytes(key, new byte[0]);
                des.Key = pdb.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
                var ms = new MemoryStream(plainText.Length * 2);
                var encStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                encStream.Write(plainBytes, 0, plainBytes.Length);
                encStream.FlushFinalBlock();
                var encryptedBytes = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(encryptedBytes, 0, (int) ms.Length);
                encStream.Close();
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DecryptString(string encryptedText, string key)
        {
            try
            {
                var des = new TripleDESCryptoServiceProvider();
                des.IV = new byte[8];
                var pdb = new PasswordDeriveBytes(key, new byte[0]);
                des.Key = pdb.CryptDeriveKey("RC2", "MD5", 128, new byte[8]);
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var ms = new MemoryStream(encryptedText.Length);
                var decStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                decStream.FlushFinalBlock();
                var plainBytes = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(plainBytes, 0, (int) ms.Length);
                decStream.Close();
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}