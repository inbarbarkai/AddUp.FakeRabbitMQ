using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace AddUp.RabbitMQ.Fakes
{
    internal sealed class FakeConnection : IConnection
    {
        private readonly RabbitServer server;

        public FakeConnection(RabbitServer rabbitServer) : this(rabbitServer, "") { }
        public FakeConnection(RabbitServer rabbitServer, string name)
        {
            server = rabbitServer ?? throw new ArgumentNullException(nameof(rabbitServer));
            ClientProvidedName = name ?? string.Empty;
            Models = new List<FakeModel>();
        }

#pragma warning disable 67
        public event EventHandler<CallbackExceptionEventArgs> CallbackException;
        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
        public event EventHandler<EventArgs> ConnectionUnblocked;
#pragma warning restore 67

        public string ClientProvidedName { get; }
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public AmqpTcpEndpoint Endpoint { get; set; }
        public IProtocol Protocol { get; set; }
        public ushort ChannelMax { get; set; }
        public uint FrameMax { get; set; }
        public TimeSpan Heartbeat { get; set; }
        public AmqpTcpEndpoint[] KnownHosts { get; set; }
        public ShutdownEventArgs CloseReason { get; private set; }
        public bool IsOpen => CloseReason == null;
        public IDictionary<string, object> ClientProperties { get; } = new Dictionary<string, object>();
        public IDictionary<string, object> ServerProperties { get; } = new Dictionary<string, object>();
        public IList<ShutdownReportEntry> ShutdownReport { get; } = new List<ShutdownReportEntry>();

        internal List<FakeModel> Models { get; } // Not part of the interface

        public void Dispose()
        {
            if (IsOpen) Abort(); // Abort rather than Close because we do not want Dispose to throw
        }

        public IModel CreateModel()
        {
            var model = new FakeModel(server);
            Models.Add(model);
            return model;
        }

        // Close and Abort (implementation inspired by RabbitMQ.Client)

        public void Abort() => Abort(Timeout.InfiniteTimeSpan);
        public void Abort(ushort reasonCode, string reasonText) => Abort(reasonCode, reasonText, Timeout.InfiniteTimeSpan);
        public void Abort(TimeSpan timeout) => Abort(200, "Connection close forced", timeout);
        public void Abort(ushort reasonCode, string reasonText, TimeSpan timeout) =>
            Close(new ShutdownEventArgs(ShutdownInitiator.Application, reasonCode, reasonText), abort: true, timeout);

        public void Close() => Close(Timeout.InfiniteTimeSpan);
        public void Close(ushort reasonCode, string reasonText) => Close(reasonCode, reasonText, Timeout.InfiniteTimeSpan);
        public void Close(TimeSpan timeout) => Close(200, "Goodbye", timeout);
        public void Close(ushort reasonCode, string reasonText, TimeSpan timeout) =>
            Close(new ShutdownEventArgs(ShutdownInitiator.Application, reasonCode, reasonText), abort: false, timeout);

        public void HandleConnectionBlocked(string reason)
        {
            // Fake implementation. Nothing to do here.
        }

        public void HandleConnectionUnblocked()
        {
            // Fake implementation. Nothing to do here.
        }

        public void UpdateSecret(string newSecret, string reason)
        {
            // Fake implementation. Nothing to do here.
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Matches the original implementation")]
        private void Close(ShutdownEventArgs reason, bool abort, TimeSpan timeout)
        {
            try
            {
                if (!IsOpen)
                    throw new AlreadyClosedException(reason);

                CloseReason = reason;
                Models.ForEach(m =>
                {
                    if (abort)
                        m.Abort();
                    else
                        m.Close();
                });

                ConnectionShutdown?.Invoke(this, reason);
            }
            catch
            {
                if (!abort) throw;
            }
        }
    }
}