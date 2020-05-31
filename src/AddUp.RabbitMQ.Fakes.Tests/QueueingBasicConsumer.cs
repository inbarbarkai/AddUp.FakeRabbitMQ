using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AddUp.RabbitMQ.Fakes
{
    // NB: QueueingBasicConsumer is not part any more of the API of RabbiMQ.Client starting with version 6
    // SharedQueue was copied from https://github.com/rabbitmq/rabbitmq-dotnet-client/blob/master/projects/RabbitMQ.Client/util/SharedQueue.cs
    // QueueingBasicConsumer from RabbitMQ.Client v5.2.0

    [ExcludeFromCodeCoverage]
    internal class QueueingBasicConsumer : DefaultBasicConsumer
    {
        public QueueingBasicConsumer() : this(null) { }
        public QueueingBasicConsumer(IModel model) : this(model, new SharedQueue<BasicDeliverEventArgs>()) { }
        public QueueingBasicConsumer(IModel model, SharedQueue<BasicDeliverEventArgs> queue) : base(model) => Queue = queue;

        public SharedQueue<BasicDeliverEventArgs> Queue { get; protected set; }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            var eventArgs = new BasicDeliverEventArgs
            {
                ConsumerTag = consumerTag,
                DeliveryTag = deliveryTag,
                Redelivered = redelivered,
                Exchange = exchange,
                RoutingKey = routingKey,
                BasicProperties = properties,
                Body = body
            };

            Queue.Enqueue(eventArgs);
        }

        public override void OnCancel(params string[] consumerTags)
        {
            base.OnCancel(consumerTags);
            Queue.Close();
        }
    }

    [ExcludeFromCodeCoverage]
    internal sealed class SharedQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private bool isOpen = true;

        public void Close()
        {
            lock (queue)
            {
                isOpen = false;
                Monitor.PulseAll(queue);
            }
        }

        public T Dequeue()
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    EnsureIsOpen();
                    Monitor.Wait(queue);
                }

                return queue.Dequeue();
            }
        }

        public bool Dequeue(TimeSpan timeout, out T result)
        {
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                result = Dequeue();
                return true;
            }

            var startTime = DateTime.Now;
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    EnsureIsOpen();
                    var elapsedTime = DateTime.Now.Subtract(startTime);
                    var remainingTime = timeout.Subtract(elapsedTime);
                    if (remainingTime <= TimeSpan.Zero)
                    {
                        result = default;
                        return false;
                    }

                    Monitor.Wait(queue, remainingTime);
                }

                result = queue.Dequeue();
                return true;
            }
        }

        public T DequeueNoWait(T defaultValue)
        {
            lock (queue)
            {
                if (queue.Count == 0)
                {
                    EnsureIsOpen();
                    return defaultValue;
                }

                return queue.Dequeue();
            }
        }

        public void Enqueue(T o)
        {
            lock (queue)
            {
                EnsureIsOpen();
                queue.Enqueue(o);
                Monitor.Pulse(queue);
            }
        }

        public IEnumerator<T> GetEnumerator() => new SharedQueueEnumerator<T>(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void EnsureIsOpen()
        {
            if (!isOpen)
                throw new EndOfStreamException("SharedQueue closed");
        }
    }

    [ExcludeFromCodeCoverage]
    internal struct SharedQueueEnumerator<T> : IEnumerator<T>
    {
        private readonly SharedQueue<T> queue;
        private T current;

        public SharedQueueEnumerator(SharedQueue<T> q)
        {
            queue = q;
            current = default;
        }

        public void Dispose()
        {
            // Nothing to do here...
        }

        public T Current
        {
            get
            {
                if (current == null)
                    throw new InvalidOperationException();
                return current;
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            try
            {
                current = queue.Dequeue();
                return true;
            }
            catch (EndOfStreamException)
            {
                current = default;
                return false;
            }
        }

        public void Reset() => throw new InvalidOperationException(
            "SharedQueue.Reset() does not make sense");
    }
}
