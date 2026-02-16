using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WackeClient.date
{
    internal class TextKey
    {
        private static string key = "W5A1C2K2E5664678"; // 16字节密钥，AES-128
        private static string iv = "W5A1C2K2E6584892";  // 16字节IV（初始化向量）

        // 加密函数
        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create()) // 创建AES加密实例
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);  // 设置密钥
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);    // 设置初始化向量

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV); // 创建加密器

                using (MemoryStream msEncrypt = new MemoryStream())  // 用于存储加密后的数据
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))  // 创建加密数据流
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))  // 写入加密流
                        {
                            swEncrypt.Write(plainText);  // 写入明文数据
                        }
                    }
                    // 返回加密后的数据，转换为Base64字符串
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

      
        // 解密函数
        public static string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create()) // 创建AES实例
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);  // 设置密钥
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);    // 设置初始化向量

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);  // 创建解密器

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))  // 将Base64字符串转换为字节数组并读取
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))  // 创建解密数据流
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))  // 从解密流中读取数据
                        {
                            return srDecrypt.ReadToEnd();  // 返回解密后的明文
                        }
                    }
                }
            }
        }

    }
}
