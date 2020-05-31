using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace AddUp.RabbitMQ.Fakes
{
    public sealed class FakeConnectionFactory : IConnectionFactory
    {
        public FakeConnectionFactory() : this(new RabbitServer()) { }
        public FakeConnectionFactory(RabbitServer rabbitServer) => 
            Server = rabbitServer ?? throw new ArgumentNullException(nameof(rabbitServer));
        
        public IDictionary<string, object> ClientProperties { get; set; }
        public string Password { get; set; }
        public ushort RequestedChannelMax { get; set; }
        public uint RequestedFrameMax { get; set; }
        public TimeSpan RequestedHeartbeat { get; set; }
        public bool UseBackgroundThreadsForIO { get; set; }
        public string UserName { get; set; }
        public string VirtualHost { get; set; }
        public Uri Uri { get; set; }
        public string ClientProvidedName { get; set; }
        public TimeSpan HandshakeContinuationTimeout { get; set; }
        public TimeSpan ContinuationTimeout { get; set; }

        // Not part of the interface
        internal RabbitServer Server { get; }
        internal FakeConnection Connection { get; private set; }

        public IAuthMechanismFactory AuthMechanismFactory(IList<string> mechanismNames) => new PlainMechanismFactory();

        public IConnection CreateConnection() => CreateConnection("");
        public IConnection CreateConnection(IList<string> hostnames) => CreateConnection("");
        public IConnection CreateConnection(IList<string> hostnames, string clientProvidedName) => CreateConnection(clientProvidedName);
        public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints) => CreateConnection("");
        public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints, string clientProvidedName) => CreateConnection(clientProvidedName);
        public IConnection CreateConnection(string clientProvidedName)
        {
            if (Connection == null)
                Connection = new FakeConnection(Server, clientProvidedName);
            return Connection;
        }
    }
}