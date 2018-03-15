using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sawtooth.Sdk.Messaging
{
    /// <summary>
    /// Stream listener.
    /// </summary>
    public interface IStreamListener
    {
        /// <summary>
        /// Ons the message.
        /// </summary>
        /// <param name="message">Message.</param>
        void OnMessage(Message message);

        /// <summary>
        /// Sends the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<Message> SendAsync(Message message, CancellationToken cancellationToken);
    }
}
