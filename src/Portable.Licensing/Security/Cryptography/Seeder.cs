using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Portable.Licensing.Security.Cryptography
{
    internal static class Seeder
    {
        internal static byte[] GenerateSeed(int seedLength = 32)
        {
            var random = new SecureRandom();

            var salt = random.GenerateSeed(seedLength);

            return salt;
        }


    }
}
