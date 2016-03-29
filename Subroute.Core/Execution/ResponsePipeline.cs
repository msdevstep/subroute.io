using System;
using System.Reactive.Subjects;
using Microsoft.ServiceBus.Messaging;
using Subroute.Core.Exceptions;
using Subroute.Core.ServiceBus;

namespace Subroute.Core.Execution
{
    public interface IResponsePipeline
    {
        SubscriptionClient ResponseSubscription { get; }
        IObservable<BrokeredMessage> ResponseMessages { get; }
    }

    public class ResponsePipeline : IDisposable, IResponsePipeline
    {
        private readonly SubscriptionClient _responseSubscription;
        private readonly Subject<BrokeredMessage> _responseMessages = new Subject<BrokeredMessage>();

        public ResponsePipeline(ITopicFactory topicFactory)
        {
            // Since we're in a constructor, we can't use the await keyword, so we'll wait the old fashioned synchonous way.
            // The clientTask.Wait() is a blocking call. This call doesn't actually create the subscription, we do that on
            // pre-application initialization, since it's faster to do it once for the whole app up-front, instead of lazily
            // somwhere buried in a constructor, plus doing it lazily causes a dead-lock.
            var requestSubscriptionName = string.Format(Settings.ServiceBusResponseSubscriptionNameFormat, Environment.MachineName);
            var clientTask = topicFactory.CreateSubscriptionClientAsync(Settings.ServiceBusResponseTopicName, requestSubscriptionName);
            clientTask.Wait();

            // Do a quick check to ensure that the task didn't throw an exception and wasn't cancelled.
            if (clientTask.IsFaulted || clientTask.IsCanceled)
                throw new ServiceBusException($"Failed to create and subscribe to the response topic for subscription '{requestSubscriptionName}'.", clientTask.Exception);

            // Store the result in a local field as we don't want it to be garbage collected.
            _responseSubscription = clientTask.Result;

            // We only get one shot at processing the message, because the SubscriptionClient is set to ReceiveAndDelete.
            // This means that we don't have to call Complete() or Abandon() on messages, and also means we only need
            // one call to service bus to handle a message. We are publishing the message into an observable sequence
            // which will ensure only one thread handles the message at a time (even if there are multiple subscribers 
            // to the observable sequence), but we'll set the MaxConcurrentCalls to something greater than 1, because the
            // observable sequence will hold the messages in memory while they are waiting to be processed. The 
            // combination of these qualities means we can essentially prefetch messages while we are processing a 
            // message. This does create multiple threads for each message, but the observable sequence will synchronize
            // them all back to the same thread. It's a small price to pay for prefetching ability.
            var options = new OnMessageOptions
            {
                AutoComplete = true,
                MaxConcurrentCalls = 5  // Drain 5 at a time into memory, so we can do a bit of prefetching.
            };
            // Using the OnMessage delegate will handle the internal polling (or sockets, depending on mode) for us.
            // When we receive a message, we'll push it through the observable sequence. The OnNext() call will block
            // until all subscriptions have received the message, but we don't have any heavy processing happening, so
            // it's not a big deal and we are using ReceiveAndDelete anyways, so it doesn't matter how long the OnNext()
            // call takes.
            _responseSubscription.OnMessage(bm =>
            {
                _responseMessages.OnNext(bm);
            }, options);
        }

        /// <summary>
        /// Thread-Safe <see cref="SubscriptionClient"/> that is used to publish message to the observable sequence.
        /// </summary>
        public SubscriptionClient ResponseSubscription => _responseSubscription;

        /// <summary>
        /// Thread-Safe <see cref="T:IObservable`1{BrokeredMessage}"/> that can be subscribed to for listening for
        /// incoming <see cref="BrokeredMessage"/> messages that originated from this machine.
        /// </summary>
        public IObservable<BrokeredMessage> ResponseMessages => _responseMessages;

        public void Dispose()
        {
            _responseMessages.Dispose();
        }
    }
}