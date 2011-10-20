namespace Defize.Gus
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    internal static class CryptographicExtensions
    {
        public static string ComputeHashCode(this FileStream fileStream, HashAlgorithmType algorithmType = HashAlgorithmType.SHA1)
        {
            using (var algorithm = GetHashAlgorithm(algorithmType))
            {
                var hash = algorithm.ComputeHash(fileStream);

                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.AppendFormat("{0:X2}", b);
                }

                return sb.ToString();
            }
        }

        private static HashAlgorithm GetHashAlgorithm(HashAlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case HashAlgorithmType.MD5:
                    return HashAlgorithm.Create("MD5");

                case HashAlgorithmType.SHA1:
                    return HashAlgorithm.Create("SHA1");

                case HashAlgorithmType.SHA256:
                    return HashAlgorithm.Create("SHA-256");

                case HashAlgorithmType.SHA384:
                    return HashAlgorithm.Create("SHA-384");

                case HashAlgorithmType.SHA512:
                    return HashAlgorithm.Create("SHA-512");
            }

            throw new ArgumentException(string.Format("Unknown hash algorithm type '{0}'.", algorithmType), "algorithmType");
        }
    }
}
