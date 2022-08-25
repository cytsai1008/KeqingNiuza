using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace KeqingNiuza.Core.CloudBackup;

public static class Endecryption
{
    private static readonly byte[] Key;

    private static readonly byte[] IV;

    static Endecryption()
    {
        var UserName = Environment.UserName;
        var MachineGuid = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\", "MachineGuid",
            UserName);
        var hash = SHA256.Create();
        var byte1 = Encoding.UTF8.GetBytes(MachineGuid + UserName);
        var byte2 = Encoding.UTF8.GetBytes(UserName);
        Key = hash.ComputeHash(byte1);
        IV = hash.ComputeHash(byte2).Take(16).ToArray();
    }


    public static byte[] Encrypt(string str)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(str);
                    }

                    return msEncrypt.ToArray();
                }
            }
        }
    }


    public static string Decrypt(byte[] bytes)
    {
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (var msDecrypt = new MemoryStream(bytes))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}