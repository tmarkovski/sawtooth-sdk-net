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
    public class Stream
    {
        readonly string Address;

        readonly NetMQSocket Socket;
        readonly NetMQPoller Poller;

        readonly IStreamListener Listener;

        public Stream(string address, IStreamListener listener = null)
        {
            Address = address;

            Socket = new DealerSocket();
            Socket.ReceiveReady += Receive;
            Socket.Options.ReconnectInterval = TimeSpan.FromSeconds(2);

            Poller = new NetMQPoller();
            Poller.Add(Socket);

            Listener = listener;
        }

        void Receive(object _, NetMQSocketEventArgs e)
        {
            var message = new Message();
            message.MergeFrom(Socket.ReceiveMultipartBytes().SelectMany(x => x).ToArray());

            if (message.MessageType == MessageType.PingRequest)
            {
                Socket.SendFrame(new PingResponse().Wrap(message, MessageType.PingResponse).ToByteArray());
                return;
            }

            Listener?.OnMessage(message);

            if (message.MessageType == MessageType.PingRequest)
                    Socket.SendFrame(new PingResponse().Wrap(message, MessageType.PingResponse).ToByteArray());
        }

        public void Send(Message message) => Socket.SendFrame(message.ToByteString().ToByteArray());

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
