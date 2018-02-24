using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Sawtooh.Sdk.Messaging;
using static Message.Types;

namespace Sawtooh.Sdk.Processor
{
    public class TransactionProcessor
    {
        readonly Stream Stream;

        readonly List<ITransactionHandler> Handlers;

        public TransactionProcessor(string address)
        {
            Stream = new Stream(address);
            Handlers = new List<ITransactionHandler>();
        }

        public void AddHandler(ITransactionHandler handler) => Handlers.Add(handler);

        public async Task Start()
        {
            Stream.Connect();

            foreach (var handler in Handlers)
            {
                var request = new TpRegisterRequest { Version = handler.Version, Family = handler.FamilyName };
                request.Namespaces.AddRange(handler.Namespaces);

                var response = await Stream.Send(new Message
                {
                    CorrelationId = Stream.GenerateId(),
                    MessageType = MessageType.TpRegisterRequest,
                    Content = request.ToByteString()
                }, CancellationToken.None);

                var reg = new TpRegisterResponse();
                reg.MergeFrom(response.Content);

                Debug.WriteLine($"Transaction processor {handler.FamilyName} {handler.Version} registration status: {reg.Status}");
            }
        }
    }
}
