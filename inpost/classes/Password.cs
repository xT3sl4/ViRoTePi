using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Text;

namespace inpost.classes
{
    public class Password
    {
        private static readonly string keyString = "TeslaMaMałeNóżki";
        private static readonly string ivString = "GdzieJestPiotrek";
        byte[] key = Encoding.UTF8.GetBytes(keyString);
        byte[] iv = Encoding.UTF8.GetBytes(ivString);

        public string password;
        public Password(string password)
        {
            this.password = Encrypt(password, key, iv);
        }
        public string Decrypt_password()
        {
            return Decrypt(password, key, iv);
        }
        static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);

            IBufferedCipher cipher = new PaddedBufferedBlockCipher(
                new CbcBlockCipher(new AesEngine()), new Pkcs7Padding());
            cipher.Init(true, new ParametersWithIV(new KeyParameter(key), iv));

            byte[] outputBytes = cipher.DoFinal(inputBytes);

            return Convert.ToBase64String(outputBytes);
        }

        static string Decrypt(string base64CipherText, byte[] key, byte[] iv)
        {
            byte[] inputBytes = Convert.FromBase64String(base64CipherText);

            IBufferedCipher cipher = new PaddedBufferedBlockCipher(
                new CbcBlockCipher(new AesEngine()), new Pkcs7Padding());
            cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));

            byte[] outputBytes = cipher.DoFinal(inputBytes);

            return Encoding.UTF8.GetString(outputBytes);
        }
    }
}
