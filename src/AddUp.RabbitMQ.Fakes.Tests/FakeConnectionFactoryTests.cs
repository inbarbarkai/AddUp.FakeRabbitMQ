using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace AddUp.RabbitMQ.Fakes
{
    [ExcludeFromCodeCoverage]
    public class FakeConnectionFactoryTests
    {

        [Fact]
        public void Connection_is_null_before_CreateConnection()
        {
            var factory = new FakeConnectionFactory();
            Assert.Null(factory.Connection);
        }

        [Fact]
        public void CreateConnection_returns_FakeConnection()
        {
            var factory = new FakeConnectionFactory();
            var connection = factory.CreateConnection();

            Assert.Same(factory.Connection, connection);
        }

        [Fact]
        public void CreateConnection_called_twice_returns_the_same_connection()
        {
            var factory = new FakeConnectionFactory();
            var connection1 = factory.CreateConnection();
            var connection2 = factory.CreateConnection();

            Assert.Same(connection1, connection2);
        }
    }
}