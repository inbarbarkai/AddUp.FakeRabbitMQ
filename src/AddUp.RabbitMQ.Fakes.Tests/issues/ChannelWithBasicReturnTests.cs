using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace AddUp.RabbitMQ.Fakes
{
    [ExcludeFromCodeCoverage]
    public class ChannelWithBasicReturnTests
    {
        // https://github.com/addupsolutions/AddUp.RabbitMQ.Fakes/issues/5
        [Fact]
        public void A_Channel_with_its_BasicReturn_event_bound_should_not_throw()
        {
            try
            {
                var rabbitServer = new RabbitServer();
                var connectionFactory = new FakeConnectionFactory(rabbitServer);

                using (var publisherConnection = connectionFactory.CreateConnection())
                using (var publisherChannel = publisherConnection.CreateModel())
                {
                    publisherChannel.BasicReturn += (s, e) => _ = 42; // useless

                    const string message = "hello world!";
                    var messageBody = Encoding.ASCII.GetBytes(message);
                    publisherChannel.BasicPublish("test_exchange", "foo.bar.baz", false, null, messageBody);
                }
            }
            catch (Exception ex)
            {
                Assert.True(false, "Expected no exception, but got: " + ex.Message);
            }
        }
    }
}
