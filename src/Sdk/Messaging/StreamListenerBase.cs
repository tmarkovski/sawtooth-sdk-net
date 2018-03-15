using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Sawtooth.Sdk.Messaging
{
    public abstract class StreamListenerBase : IStreamListener
    {
        readonly ConcurrentDictionary<string, TaskCompletionSource<Message>> Futures;
        protected readonly Stream Stream;

        public StreamListenerBase(string address)
        {
            Stream = new Stream(address, this);
            Futures = new ConcurrentDictionary<string, TaskCompletionSource<Message>>();
        }

        public virtual void OnMessage(Message message)
        {
            if (Futures.TryGetValue(message.CorrelationId, out var source))
            {
                if (source.Task.Status != TaskStatus.RanToCompletion) source.SetResult(message);
                Futures.TryRemove(message.CorrelationId, out var _);
            }
        }

        public Task<Message> SendAsync(Message message, CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<Message>();
            cancellationToken.Register(() => source.SetCanceled());

            if (Futures.TryAdd(message.CorrelationId, source))
            {
                Stream.Send(message);
                return source.Task;
            }
            if (Futures.TryGetValue(message.CorrelationId, out var task))
            {
                return task.Task;
            }
            throw new InvalidOperationException("Cannot get or set future context for this message.");
        }

        protected void Connect() => Stream.Connect();

        protected void Disconnect() => Stream.Disconnect();
    }
}
