using System;

namespace Utility.Security.Encryption
{
    public interface IEncryption
    {
        String Encrypt(String value,String Key);
        String Decrypt(String value, String Key);

    }

    
    public static class EncryptionFactory
    {
        public static AES256 aes256;
        public static IEncryption GetEncryption(String EncryptionAlgorithm)
        {
            switch (EncryptionAlgorithm)
            {
                case "AES256":
                    if (aes256 != null) return aes256;
                    else
                    {
                        aes256 = new AES256();
                        return aes256;
                    }
                default:
                    return null;
            }
        }
    }
}
