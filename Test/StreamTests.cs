using System;
using System.Threading.Tasks;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using Sawtooh.Sdk.Messaging;
using Xunit;
using static Message.Types;

namespace Test
{
    public class StreamTests
    {
        [Fact]
        public void RespondToPing()
        {
            var serverSocket = new PairSocket();
            var clientSocket = new PairSocket();

            serverSocket.Bind("inproc://inproc-test");

            var correlationId = Stream.GenerateId();
            var pingMessage = new Message() { CorrelationId = correlationId, MessageType = MessageType.PingRequest, Content = ByteString.Empty };

            var stream = new Stream("inproc://inproc-test", clientSocket);
            stream.Connect();

            var task1 = Task.Run(() => serverSocket.SendFrame(pingMessage.ToByteString().ToByteArray()));
            var task2 = Task.Run(() =>
            {
                var message = new Message();
                message.MergeFrom(serverSocket.ReceiveFrameBytes());

                return message;
            });

            Task.WaitAll(new[] { task1, task2 });

            var actualMessage = task2.Result;

            Assert.Equal(MessageType.PingResponse, actualMessage.MessageType);
            Assert.Equal(correlationId, actualMessage.CorrelationId);
        }
    }
}
