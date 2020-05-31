using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AddUp.RabbitMQ.Fakes
{
    // NB: QueueingBasicConsumer is not part any more of the API of RabbiMQ.Client starting with version 6
    // SharedQueue was copied from https://github.com/rabbitmq/rabbitmq-dotnet-client/blob/master/projects/RabbitMQ.Client/util/SharedQueue.cs
    // QueueingBasicConsumer from RabbitMQ.Client v5.2.0

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

    internal sealed class SharedQueue<T> : IEnumerable<T>
    {
        private struct SharedQueueEnumerator<T> : IEnumerator<T>
        {
            private readonly SharedQueue<T> _queue;
            private T _current;

            ///<summary>Construct an enumerator for the given
            ///SharedQueue.</summary>
            public SharedQueueEnumerator(SharedQueue<T> queue)
            {
                _queue = queue;
                _current = default;
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (_current == null)
                        throw new InvalidOperationException();
                    return _current;
                }
            }

            T IEnumerator<T>.Current
            {
                get
                {
                    if (_current == null)
                        throw new InvalidOperationException();
                    return _current;
                }
            }

            public void Dispose()
            {
                // Nothing to do here...
            }

            bool System.Collections.IEnumerator.MoveNext()
            {
                try
                {
                    _current = _queue.Dequeue();
                    return true;
                }
                catch (EndOfStreamException)
                {
                    _current = default;
                    return false;
                }
            }

            void System.Collections.IEnumerator.Reset() => throw new InvalidOperationException("SharedQueue.Reset() does not make sense");
        }

        protected bool m_isOpen = true;
        protected Queue<T> m_queue = new Queue<T>();

        public void Close()
        {
            lock (m_queue)
            {
                m_isOpen = false;
                Monitor.PulseAll(m_queue);
            }
        }

        public T Dequeue()
        {
            lock (m_queue)
            {
                while (m_queue.Count == 0)
                {
                    EnsureIsOpen();
                    Monitor.Wait(m_queue);
                }
                return m_queue.Dequeue();
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
            lock (m_queue)
            {
                while (m_queue.Count == 0)
                {
                    EnsureIsOpen();
                    var elapsedTime = DateTime.Now.Subtract(startTime);
                    var remainingTime = timeout.Subtract(elapsedTime);
                    if (remainingTime <= TimeSpan.Zero)
                    {
                        result = default;
                        return false;
                    }

                    Monitor.Wait(m_queue, remainingTime);
                }

                result = m_queue.Dequeue();
                return true;
            }
        }

        public T DequeueNoWait(T defaultValue)
        {
            lock (m_queue)
            {
                if (m_queue.Count == 0)
                {
                    EnsureIsOpen();
                    return defaultValue;
                }
                else
                {
                    return m_queue.Dequeue();
                }
            }
        }

        public void Enqueue(T o)
        {
            lock (m_queue)
            {
                EnsureIsOpen();
                m_queue.Enqueue(o);
                Monitor.Pulse(m_queue);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new SharedQueueEnumerator<T>(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new SharedQueueEnumerator<T>(this);

        private void EnsureIsOpen()
        {
            if (!m_isOpen)
                throw new EndOfStreamException("SharedQueue closed");
        }
    }
}
