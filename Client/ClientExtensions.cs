using System;
using System.Linq;

namespace Sawtooth.Sdk.Client
{
    public static class ClientExtensions
    {
        public static string ToHex(this byte[] data) => String.Concat(data.Select(x => x.ToString("x2")));
    }
}
