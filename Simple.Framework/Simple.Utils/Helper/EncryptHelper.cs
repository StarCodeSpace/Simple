﻿using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Simple.Utils.Helper
{
    /// <summary>RSA算法类型</summary>
    public enum RSAType
    {
        /// <summary>SHA1</summary>
        RSA = 0,

        /// <summary>RSA2 密钥长度至少为2048 SHA256</summary>
        RSA2
    }

    /// <summary>SH1加密</summary>
    public static class SHA1
    {
        public static string Encrypt(string str)
        {
            var buff = Encoding.UTF8.GetBytes(str);
            var sha1 = new SHA1CryptoServiceProvider();

            var output = sha1.ComputeHash(buff);

            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }
    }

    /// <summary>MD5加密</summary>
    public static class MD5
    {
        /// <summary>使用当前缺省的字符编码对字符串进行加密</summary>
        /// <param name="str">需要进行md5演算的字符串</param>
        /// <returns>用小写字母表示的32位16进制数字字符串</returns>
        public static string Encrypt(string str)
        {
            var buff = Encoding.UTF8.GetBytes(str);
            var md5 = new MD5CryptoServiceProvider();
            var output = md5.ComputeHash(buff);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }
    }

    /// <summary>SHA256加密</summary>
    public static class SHA256
    {
        public static string Encrypt(string str)
        {
            var buff = Encoding.UTF8.GetBytes(str);
            var sha256 = new SHA256Managed();
            var output = sha256.ComputeHash(buff);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }

        public static string RSAEncrypt(string str, string pfxFilePath, string pfxPassword)
        {
            //SHA256WithRSA
            X509Certificate2 privateCert = new X509Certificate2(pfxFilePath, pfxPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.UserKeySet);
            RSACryptoServiceProvider privateKey = (RSACryptoServiceProvider)privateCert.PrivateKey;
            // This one can:
            RSACryptoServiceProvider privateKey1 = new RSACryptoServiceProvider();
            privateKey1.ImportParameters(privateKey.ExportParameters(true));
            byte[] data = Encoding.UTF8.GetBytes(str);
            byte[] signature = privateKey1.SignData(data, "SHA256");
            //密文
            string sign = Convert.ToBase64String(signature);

            return sign;
        }
    }

    /// <summary>AES加密</summary>
    public static class AES
    {
        /// <summary>AES 解密</summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string key, string iv, CipherMode mode = CipherMode.ECB)
        {
            if (string.IsNullOrEmpty(str)) return null;
            byte[] toEncryptArray = Convert.FromBase64String(str);
            byte[] newkey = Convert.FromBase64String(key);
            byte[] newiv = Convert.FromBase64String(iv);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = newkey,
                Mode = mode,
                Padding = PaddingMode.PKCS7,
                IV = newiv,
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>AES 解密</summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string key, CipherMode mode = CipherMode.ECB)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);

            RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = mode,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7,
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }

    /// <summary>Nonce帮助类</summary>
    public class NonceHelp
    {
        private const string CHARS = @"0123456789abcdefghijklmnoprstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string MakeSessionId(string passport, string password, string hostAddress, string client, Guid entranceEntry, string nonce)
        {
            if (hostAddress == "::1" || hostAddress == "localhost")
            {
                hostAddress = "127.0.0.1";
            }

            return MD5.Encrypt(SHA1.Encrypt($"{passport}({password})@{hostAddress}.{entranceEntry}.{client}#{nonce}"));
        }

        /// <summary>获取随机字符串</summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string MakeNonceString(int length = 32)
        {
            var parttern = CHARS;

            var rnd = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(parttern[rnd.Next(parttern.Length)]);
            }
            var secret = sb.ToString();

            return secret;
        }

        /// <summary>做个加密密码</summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MakeEncryptedPassword(string password)
        {
            return MD5.Encrypt(SHA1.Encrypt(password));
        }

        public static int GetTimestamp()
        {
            System.DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            return (int)(DateTime.Now - startTime).TotalSeconds;
        }
    }

    /// <summary>RSA加密</summary>
    public class RSAHelper
    {
        private readonly RSA _privateKeyRsaProvider;
        private readonly RSA _publicKeyRsaProvider;
        private readonly HashAlgorithmName _hashAlgorithmName;
        private readonly Encoding _encoding;

        /// <summary>实例化RSAHelper</summary>
        /// <param name="rsaType">加密算法类型 RSA SHA1;RSA2 SHA256 密钥长度至少为2048</param>
        /// <param name="encoding">编码类型</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="publicKey">公钥</param>
        public RSAHelper(RSAType rsaType, Encoding encoding, string privateKey, string publicKey = null)
        {
            _encoding = encoding;
            if (!string.IsNullOrEmpty(privateKey))
            {
                _privateKeyRsaProvider = CreateRsaProviderFromPrivateKey(privateKey);
            }

            if (!string.IsNullOrEmpty(publicKey))
            {
                _publicKeyRsaProvider = CreateRsaProviderFromPublicKey(publicKey);
            }

            _hashAlgorithmName = rsaType == RSAType.RSA ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
        }

        #region 使用私钥签名

        /// <summary>使用私钥签名</summary>
        /// <param name="data">原始数据</param>
        /// <returns></returns>
        public string Sign(string data)
        {
            byte[] dataBytes = _encoding.GetBytes(data);

            var signatureBytes = _privateKeyRsaProvider.SignData(dataBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        #endregion 使用私钥签名

        #region 使用公钥验证签名

        /// <summary>使用公钥验证签名</summary>
        /// <param name="data">原始数据</param>
        /// <param name="sign">签名</param>
        /// <returns></returns>
        public bool Verify(string data, string sign)
        {
            byte[] dataBytes = _encoding.GetBytes(data);
            byte[] signBytes = Convert.FromBase64String(sign);

            var verify = _publicKeyRsaProvider.VerifyData(dataBytes, signBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

            return verify;
        }

        #endregion 使用公钥验证签名

        #region 解密

        public string Decrypt(string cipherText)
        {
            if (_privateKeyRsaProvider == null)
            {
                throw new Exception("_privateKeyRsaProvider is null");
            }
            return Encoding.UTF8.GetString(_privateKeyRsaProvider.Decrypt(Convert.FromBase64String(cipherText), RSAEncryptionPadding.Pkcs1));
        }

        #endregion 解密

        #region 加密

        public string Encrypt(string text)
        {
            if (_publicKeyRsaProvider == null)
            {
                throw new Exception("_publicKeyRsaProvider is null");
            }
            return Convert.ToBase64String(_publicKeyRsaProvider.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.Pkcs1));
        }

        #endregion 加密

        #region 使用私钥创建RSA实例

        /// <summary>使用私钥创建RSA实例</summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public RSA CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsa = RSA.Create();
            var rsaParameters = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(rsaParameters);
            return rsa;
        }

        #endregion 使用私钥创建RSA实例

        #region 使用公钥创建RSA实例

        /// <summary>使用公钥创建RSA实例</summary>
        /// <param name="publicKeyString"></param>
        /// <returns></returns>
        public RSA CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            // encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            var x509Key = Convert.FromBase64String(publicKeyString);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob ------
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public
                    // key -----
                    var rsa = RSA.Create();
                    RSAParameters rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }
            }
        }

        #endregion 使用公钥创建RSA实例

        #region 导入密钥算法

        private int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        #endregion 导入密钥算法
    }
}