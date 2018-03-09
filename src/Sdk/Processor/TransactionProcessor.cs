using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using NetMQ;
using Sawtooth.Sdk.Messaging;
using static Message.Types;

namespace Sawtooth.Sdk.Processor
{
    public class TransactionProcessor : IStreamListener<TpProcessRequest>
    {
        readonly Stream<TpProcessRequest> Stream;

        readonly List<ITransactionHandler> Handlers;

        public TransactionProcessor(string address)
        {
            Stream = new Stream<TpProcessRequest>(address);
            Stream.SetListener(this, MessageType.TpProcessRequest);

            Handlers = new List<ITransactionHandler>();
        }

        public void AddHandler(ITransactionHandler handler) => Handlers.Add(handler);

        public void Call(TpProcessRequest request, Message message)
        {
            Handlers.FirstOrDefault(x => x.FamilyName == request.Header.FamilyName
                                          && x.Version == request.Header.FamilyVersion)?
                    .ApplyAsync(request, new TransactionContext(Stream, request.ContextId))
                    .ContinueWith((task) =>
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        Stream.Socket.SendFrame(new TpProcessResponse { Status = TpProcessResponse.Types.Status.Ok }
                                         .Wrap(message, MessageType.TpProcessResponse).ToByteArray());
                        break;

                    case TaskStatus.Faulted:
                        var errorData = ByteString.CopyFrom(task.Exception?.ToString() ?? string.Empty, Encoding.UTF8);
                        if (task.Exception != null && task.Exception.InnerException is InvalidTransactionException)
                        {
                            Stream.Socket.SendFrame(new TpProcessResponse { Status = TpProcessResponse.Types.Status.InvalidTransaction }
                                             .Wrap(message, MessageType.TpProcessResponse).ToByteArray());
                        }
                        else
                        {
                            Stream.Socket.SendFrame(new TpProcessResponse { Status = TpProcessResponse.Types.Status.InternalError }
                                           .Wrap(message, MessageType.TpProcessResponse).ToByteArray());
                        }
                        break;
                }
            });
        }

        public async void Start()
        {
            Stream.Connect();

            foreach (var handler in Handlers)
            {
                var request = new TpRegisterRequest { Version = handler.Version, Family = handler.FamilyName };
                request.Namespaces.AddRange(handler.Namespaces);

                var response = await Stream.Send(request.Wrap(MessageType.TpRegisterRequest), CancellationToken.None);
                Console.WriteLine($"Transaction processor registration: {response.Unwrap<TpRegisterResponse>().Status}");
            }
        }

        public void Stop()
        {
            Task.WaitAll(new [] { Stream.Send(new TpUnregisterRequest().Wrap(MessageType.TpUnregisterRequest), CancellationToken.None) });
            Stream.Disconnect();
        }
    }
}
