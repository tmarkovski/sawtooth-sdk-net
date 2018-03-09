using System.Threading;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Messaging
{
    public interface IStream
    {
        void Connect();
        void Disconnect();
        Task<Message> Send(Message message, CancellationToken cancellationToken);
    }
}