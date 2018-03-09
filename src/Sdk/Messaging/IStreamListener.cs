using System;
namespace Sawtooth.Sdk.Messaging
{
    interface IStreamListener<T>
    {
        void Call(T request, Message message);
    }
}
