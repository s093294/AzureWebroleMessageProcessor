﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureWebRole.MessageProcessor.Core
{
    public interface IMessageProcessorClientProvider<MessageType> : IMessageProcessorClientProvider<IMessageProcessorProviderOptions<MessageType>, MessageType>
    {
        void StartListening(Func<MessageType, Task> OnMessageAsync);
        //Task SendMessageAsync(MessageType message);
        //Task SendMessagesAsync(IEnumerable<MessageType> message);
        Task SendMessageAsync<T>(T message) where T : BaseMessage;
        Task SendMessagesAsync<T>(IEnumerable<T> messages) where T : BaseMessage;

        Task<int> GetDeliveryCountAsync(MessageType message);

        Task CompleteMessageAsync(MessageType message);

        Task MoveToDeadLetterAsync(MessageType message, string p1, string p2);

        Task RenewLockAsync(MessageType message);

        Task<string> GetMessageIdForMessageAsync(MessageType message);


        Task StopListeningAsync();
    }
}
