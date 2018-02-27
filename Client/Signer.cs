using System;
using System.Diagnostics;
using System.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace Sawtooth.Sdk.Client
{
    public class Signer
    {
        readonly X9ECParameters Secp256k1 = ECNamedCurveTable.GetByName("secp256k1");
        readonly ECDomainParameters DomainParams;
        readonly IDigest Sha256Digest = new Sha256Digest();

        public Signer()
        {
            DomainParams = new ECDomainParameters(Secp256k1.Curve, Secp256k1.G, Secp256k1.N, Secp256k1.H, Secp256k1.GetSeed());
        }

        public (byte[] Public, byte[] Private) GenerateKeyPair()
        {
            var keyParams = new ECKeyGenerationParameters(DomainParams, new SecureRandom());

            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(keyParams);
            var keyPair = generator.GenerateKeyPair();

            var privateKey = keyPair.Private as ECPrivateKeyParameters;
            var publicKey = keyPair.Public as ECPublicKeyParameters;

            return (publicKey.Q.GetEncoded(), 
                    privateKey.D.ToByteArray());
        }

        public byte[] Sign(byte[] digest, byte[] privatekey)
        {
            var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));

            signer.Init(true, new ECPrivateKeyParameters(new BigInteger(1, privatekey), DomainParams));
            var signature = signer.GenerateSignature(digest);

            var R = signature[0];
            var S = signature[1];

            // Ensure low S
            if (!(S.CompareTo(Secp256k1.N.ShiftRight(1)) <= 0))
            {
                S = Secp256k1.N.Subtract(S);
            }

            return R.ToByteArrayUnsigned().Concat(S.ToByteArrayUnsigned()).ToArray();
        }

        public bool Verify(byte[] digest, byte[] signature, byte[] publicKey)
        {
            var X = new BigInteger(1, publicKey.Skip(1).Take(32).ToArray());
            var Y = new BigInteger(1, publicKey.Skip(33).Take(32).ToArray());

            var R = new BigInteger(1, signature.Take(32).ToArray());
            var S = new BigInteger(1, signature.Skip(32).ToArray());

            var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));

            var point = Secp256k1.Curve.CreatePoint(X, Y);
            
            signer.Init(false, new ECPublicKeyParameters(point, DomainParams));
            return signer.VerifySignature(digest, R, S);
        }

        public byte[] Hash(byte[] digest)
        {
            var result = new byte[Sha256Digest.GetDigestSize()];

            Sha256Digest.Reset();
            Sha256Digest.BlockUpdate(digest, 0, digest.Length);
            Sha256Digest.DoFinal(result, 0);

            return result;
        }
    }
}
