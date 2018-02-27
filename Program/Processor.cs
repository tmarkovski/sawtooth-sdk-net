using System;
using System.Threading.Tasks;
using Sawtooth.Sdk;
using Sawtooth.Sdk.Processor;
using Sawtooth.Sdk.Client;
using System.Diagnostics;
using System.Text;

namespace Program
{
    public class IntKeyProcessor : ITransactionHandler
    {
        public string FamilyName { get => "myintkey"; }
        public string Version { get => "1.0"; }
        public string[] Namespaces { get => new [] { FamilyName.ToByteArray().ToSha512().ToHexString().Substring(0, 6) }; }

        public Task Apply(TpProcessRequest request, Context context)
        {
            Debug.WriteLine($"Request received by tx processor: {Encoding.UTF8.GetString(request.Payload.ToByteArray())}");

            return Task.CompletedTask;
        }
    }
}
