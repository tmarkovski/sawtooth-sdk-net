using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Messaging
{
    public interface IStreamListener
    {
        void OnMessage(Message message);

        Task<Message> SendAsync(Message message, CancellationToken cancellationToken);
    }
}
