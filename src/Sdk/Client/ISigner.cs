using System;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Client
{
    /// <summary>
    /// Represents a signer contract. This can be used to sign transactions outside of the <see cref="T:Sawtooth.Sdk.Client.Signer"/> class.
    /// </summary>
    public interface ISigner
    {
        /// <summary>
        /// Sign the specified digest.
        /// </summary>
        /// <returns>The sign.</returns>
        /// <param name="digest">Digest.</param>
        byte[] Sign(byte[] digest);

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <returns>The public key.</returns>
        byte[] GetPublicKey();
    }
}
