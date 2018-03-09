using System;
using Sawtooth.Sdk.Messaging;
using static Message.Types;

namespace Sawtooth.Sdk.Client
{
    public class Subscriber : IStreamListener<ClientEventsGetRequest>
    {
        readonly Stream<ClientEventsGetRequest> Stream;


        public Subscriber(string address)
        {
            Stream = new Stream<ClientEventsGetRequest>(address);
            Stream.SetListener(this, MessageType.ClientEventsGetRequest);
        }

        public void Call(ClientEventsGetRequest request, Message message)
        {
            throw new NotImplementedException();
        }

        public void Subscribe()
        {
            

        }
    }
}
