using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Sawtooh.Sdk.Messaging;
using static Message.Types;

namespace Sawtooh.Sdk.Processor
{
    public class Context
    {
        readonly Stream Stream;
        readonly string ContextId;

        public Context(Stream stream, string contextId)
        {
            Stream = stream;
            ContextId = contextId;
        }

        public async Task<(string Address, ByteString Data)[]> GetState(string[] addresses)
        {
            var request = new TpStateGetRequest { ContextId = ContextId };
            request.Addresses.AddRange(addresses);

            var response = await Stream.Send(new Message
            {
                MessageType = MessageType.TpStateGetRequest,
                CorrelationId = Stream.GenerateId(),
                Content = request.ToByteString()
            }, CancellationToken.None);

            var state = new TpStateGetResponse();
            state.MergeFrom(response.Content);

            return state.Entries.Select(x => (x.Address, x.Data)).ToArray();
        }

        public async Task<string[]> SetState((string, ByteString)[] addressValuePairs)
        {
            var request = new TpStateSetRequest { ContextId = ContextId };
            request.Entries.AddRange(addressValuePairs.Select(x => new TpStateEntry { Address = x.Item1, Data = x.Item2 }));

            var response = await Stream.Send(new Message
            {
                MessageType = MessageType.TpStateSetRequest,
                CorrelationId = Stream.GenerateId(),
                Content = request.ToByteString()
            }, CancellationToken.None);

            var state = new TpStateSetResponse();
            state.MergeFrom(response.Content);

            return state.Addresses.ToArray();
        }
    }
}
