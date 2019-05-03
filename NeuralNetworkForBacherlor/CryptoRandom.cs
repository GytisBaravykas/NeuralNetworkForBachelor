using System;
using System.Security.Cryptography;

namespace NeuralNetworkForBacherlor
{
    public class CryptoRandom
    {
        public static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        public static double RandomValue
        {
            get
            {
                byte[] data = new byte[8];
                rng.GetBytes(data);
                var ul = BitConverter.ToUInt64(data, 0) / (1 << 11);
                Double d = ul / (Double)(1UL << 53);
                return d;
            }
        }

        public CryptoRandom()
        {

        }

    }
}
