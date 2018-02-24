using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using static Message.Types;

namespace Sawtooh.Sdk.Messaging
{
    public class Stream
    {
        readonly string Address;

        readonly DealerSocket Socket;
        readonly NetMQPoller Poller;

        readonly ConcurrentDictionary<string, TaskCompletionSource<Message>> Futures;

        public Stream(string address)
        {
            Address = address;

            Socket = new DealerSocket();
            Poller = new NetMQPoller();

            Socket.ReceiveReady += OnReceive;
            Poller.Add(Socket);

            Futures = new ConcurrentDictionary<string, TaskCompletionSource<Message>>();
        }

        void OnReceive(object sender, NetMQSocketEventArgs e)
        {
            var message = new Message();
            message.MergeFrom(e.Socket.ReceiveFrameBytes());

            if (message.MessageType == MessageType.PingRequest)
            {
                var pingMessage = new Message(message)
                {
                    MessageType = MessageType.PingResponse,
                    Content = ByteString.Empty
                };
                Socket.SendFrame(pingMessage.ToByteString().ToByteArray());
                return;
            }

            if (Futures.TryGetValue(message.CorrelationId, out var source))
            {
                if (source.Task.Status == TaskStatus.RanToCompletion)
                {
                    Futures.TryRemove(message.CorrelationId, out var _);
                }
                else
                {
                    source.SetResult(message);
                }
            }
            else
            {
                Futures.TryAdd(message.CorrelationId, new TaskCompletionSource<Message>());
            }
        }

        public Task Send(Message message)
        {
            var source = new TaskCompletionSource<Message>();
            Futures.TryAdd(message.CorrelationId, source);

            Socket.SendFrame(message.ToByteString().ToByteArray());

            return source.Task;
        }

        public void SendBack(Message message)
        {
            Socket.SendFrame(message.ToByteString().ToByteArray());

            if (!Futures.TryRemove(message.CorrelationId, out var _))
            {
                Debug.WriteLine($"Couldn't find existing message {message.CorrelationId} of type {message.MessageType}");
            }
        }

        public void Connect()
        {
            Socket.Connect(Address);
            Poller.RunAsync();
        }
    }
}
