using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Utility.Security.Hashing
{

   

    public static class HashingFactory
    {
        static SHA256 sha256;
        static SHA256 sha256Managed;
        static HMACSHA256 hMACSHA256;
        public static HashAlgorithm GetHash(String HashAlgorithm,String Key = null)
        {
            switch (HashAlgorithm)
            {
                case "SHA256":
                    if (sha256 != null) return sha256;
                    else
                    {
                        sha256 = SHA256.Create();
                        return sha256;
                    }
                case "SHA256Managed":
                    if (sha256Managed != null) return sha256Managed;
                    else
                    {
                        sha256Managed = SHA256Managed.Create();
                        return sha256Managed;
                    }
                case "HMACSHA256":
                    if (hMACSHA256 != null) return hMACSHA256;
                    else
                    {
                        if (String.IsNullOrWhiteSpace(Key))
                        {
                            Key = "KeyHMACSHA256ClkMyCls_#345vdsd@43545";
                        }
                        hMACSHA256 = new HMACSHA256(Encoding.UTF8.GetBytes(Key));
                        return hMACSHA256;
                    }
                default:
                    return null;
            }
        }
    }
    
}
