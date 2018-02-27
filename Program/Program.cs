using System;
using System.Diagnostics;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using Sawtooth.Sdk.Client;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var signer = new Signer();
            var (PublicKey, PrivateKey) = signer.GenerateKeyPair();

            var data = signer.Hash(new byte[] { 1, 2, 3 });
            var signature = signer.Sign(data, PrivateKey);

            Debug.WriteLine($"Message: {data.ToHex()}");
            Debug.WriteLine($"Signature: {signature.ToHex()}");
            Debug.WriteLine($"Public Key: {PublicKey.ToHex()}");

            Debug.WriteLine($"Verify: {signer.Verify(data, signature, PublicKey)}");
        }
    }
}
