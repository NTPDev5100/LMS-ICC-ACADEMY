using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AppZim.ZIM
{
    public class Encryptor
    {
        private static string key = "N@mg4";
        public static string Encrypt(string data)
        {
            try
            {
                //string key = "userpass";
                data = data.Trim();
                byte[] keydata = Encoding.ASCII.GetBytes(key);
                string md5String = BitConverter.ToString(new
                MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
                byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
                TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
                tripdes.Mode = CipherMode.ECB;
                tripdes.Key = tripleDesKey;
                tripdes.GenerateIV();
                MemoryStream ms = new MemoryStream();
                CryptoStream encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),
                CryptoStreamMode.Write);
                encStream.Write(Encoding.ASCII.GetBytes(data), 0,
                Encoding.ASCII.GetByteCount(data));
                encStream.FlushFinalBlock();
                byte[] cryptoByte = ms.ToArray();
                ms.Close();
                encStream.Close();
                return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
            }
            catch
            {
                //throw ex;
                return "";
            }
        }

        public static string Decrypt(string data)
        {
            try
            {
                byte[] keydata = System.Text.Encoding.ASCII.GetBytes(key);
                string md5String = BitConverter.ToString(new
                MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
                byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
                TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
                tripdes.Mode = CipherMode.ECB;
                tripdes.Key = tripleDesKey;
                byte[] cryptByte = Convert.FromBase64String(data);
                MemoryStream ms = new MemoryStream(cryptByte, 0, cryptByte.Length);
                ICryptoTransform cryptoTransform = tripdes.CreateDecryptor();
                CryptoStream decStream = new CryptoStream(ms, cryptoTransform,
                CryptoStreamMode.Read);
                StreamReader read = new StreamReader(decStream);
                return (read.ReadToEnd());
            }
            catch
            {
                //throw ex;
                return "";
            }
        }

        public static string EncryptURL(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    byte[] mybyte = System.Text.Encoding.UTF8.GetBytes(data);
                    string returntext = System.Convert.ToBase64String(mybyte);
                    return returntext;
                }
                catch
                {
                    return "";
                }
            }
            return "";
        }

        public static string DecryptURL(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    byte[] mybyte = System.Convert.FromBase64String(data);
                    string returntext = System.Text.Encoding.UTF8.GetString(mybyte);
                    return returntext;
                }
                catch
                {
                    return "";
                }
            }
            return "";
        }
    }
}