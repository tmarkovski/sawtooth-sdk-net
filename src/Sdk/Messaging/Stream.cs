using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using static Message.Types;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading;
using Sawtooth.Sdk.Processor;
using Sawtooth.Sdk.Client;

namespace Sawtooth.Sdk.Messaging
{
    public class Stream<T> : IStream where T : IMessage, new()
    {
        readonly string Address;

        internal readonly NetMQSocket Socket;
        readonly NetMQPoller Poller;

        readonly ConcurrentDictionary<string, TaskCompletionSource<Message>> Futures;

        IStreamListener<T> Listener;
        MessageType ListenerMessageType;

        public Stream(string address)
        {
            Address = address;

            Socket = new DealerSocket();
            Socket.ReceiveReady += Receive;
            Socket.Options.ReconnectInterval = TimeSpan.FromSeconds(2);

            Poller = new NetMQPoller();
            Poller.Add(Socket);

            Futures = new ConcurrentDictionary<string, TaskCompletionSource<Message>>();
        }

        internal void SetListener(IStreamListener<T> streamDelegate, MessageType messageType)
        {
            Listener = streamDelegate;
            ListenerMessageType = messageType;
        }

        void Receive(object _, NetMQSocketEventArgs e)
        {
            var message = new Message();
            message.MergeFrom(Socket.ReceiveMultipartBytes().SelectMany(x => x).ToArray());

            if (message.MessageType == MessageType.PingRequest)
            {
                Socket.SendFrame(new PingResponse().Wrap(message, MessageType.PingResponse).ToByteArray());
            } else if (message.MessageType == ListenerMessageType)
            {
                Listener?.Call(message.Unwrap<T>(), message);
            }

            if (Futures.TryGetValue(message.CorrelationId, out var source))
            {
                if (source.Task.Status != TaskStatus.RanToCompletion) source.SetResult(message);
                Futures.TryRemove(message.CorrelationId, out var _);
            }
            else
            {
                Debug.WriteLine("Possible unexpected message received");
                Futures.TryAdd(message.CorrelationId, new TaskCompletionSource<Message>());
            }
        }

        public Task<Message> Send(Message message, CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<Message>();
            cancellationToken.Register(() => source.SetCanceled());

            if (Futures.TryAdd(message.CorrelationId, source))
            {
                Socket.SendFrame(message.ToByteString().ToByteArray());
                return source.Task;
            }
            if (Futures.TryGetValue(message.CorrelationId, out var task))
            {
                return task.Task;
            }
            throw new InvalidOperationException("Cannot get or set future context for this message.");
        }

        public void Connect()
        {
            Socket.Connect(Address);
            Poller.RunAsync();
        }

        public void Disconnect()
        {
            Socket.Disconnect(Address);
            Poller.StopAsync();
        }
    }
}
