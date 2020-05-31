using System.Collections.Generic;
using RabbitMQ.Client;

namespace AddUp.RabbitMQ.Fakes
{
	// Adapted from RabbitMQ.Client's RabbitMQ.Client.Framing.BasicProperties
	internal sealed class FakeBasicProperties : IBasicProperties
    {
		private string m_contentType;
		private string m_contentEncoding;
		private IDictionary<string, object> m_headers;
		private byte m_deliveryMode;
		private byte m_priority;
		private string m_correlationId;
		private string m_replyTo;
		private string m_expiration;
		private string m_messageId;
		private AmqpTimestamp m_timestamp;
		private string m_type;
		private string m_userId;
		private string m_appId;
		private string m_clusterId;
		private bool m_contentType_present;
		private bool m_contentEncoding_present;
		private bool m_headers_present;
		private bool m_deliveryMode_present;
		private bool m_priority_present;
		private bool m_correlationId_present;
		private bool m_replyTo_present;
		private bool m_expiration_present;
		private bool m_messageId_present;
		private bool m_timestamp_present;
		private bool m_type_present;
		private bool m_userId_present;
		private bool m_appId_present;
		private bool m_clusterId_present;

		public string AppId
		{
			get => m_appId;
			set
			{
				m_appId_present = true;
				m_appId = value;
			}
		}

		public string ClusterId
		{
			get => m_clusterId;
			set
			{
				m_clusterId_present = true;
				m_clusterId = value;
			}
		}

		public string ContentType
		{
			get => m_contentType;
			set
			{
				m_contentType_present = true;
				m_contentType = value;
			}
		}

		public string ContentEncoding
		{
			get => m_contentEncoding;
			set
			{
				m_contentEncoding_present = true;
				m_contentEncoding = value;
			}
		}

		public IDictionary<string, object> Headers
		{
			get => m_headers;
			set
			{
				m_headers_present = true;
				m_headers = value;
			}
		}

		public byte DeliveryMode
		{
			get => m_deliveryMode;
			set
			{
				m_deliveryMode_present = true;
				m_deliveryMode = value;
			}
		}

		public byte Priority
		{
			get => m_priority;
			set
			{
				m_priority_present = true;
				m_priority = value;
			}
		}

		public string CorrelationId
		{
			get => m_correlationId;
			set
			{
				m_correlationId_present = true;
				m_correlationId = value;
			}
		}

		public string ReplyTo
		{
			get => m_replyTo;
			set
			{
				m_replyTo_present = true;
				m_replyTo = value;
			}
		}

		public string Expiration
		{
			get => m_expiration;
			set
			{
				m_expiration_present = true;
				m_expiration = value;
			}
		}

		public string MessageId
		{
			get => m_messageId;
			set
			{
				m_messageId_present = true;
				m_messageId = value;
			}
		}

		public bool Persistent
		{
			get => DeliveryMode == 2;
			set => DeliveryMode = (byte)((!value) ? 1 : 2);
		}

		public AmqpTimestamp Timestamp
		{
			get => m_timestamp;
			set
			{
				m_timestamp_present = true;
				m_timestamp = value;
			}
		}

		public string Type
		{
			get => m_type;
			set
			{
				m_type_present = true;
				m_type = value;
			}
		}

		public string UserId
		{
			get => m_userId;
			set
			{
				m_userId_present = true;
				m_userId = value;
			}
		}

		public ushort ProtocolClassId => 60;

		public string ProtocolClassName => "basic";
			   
		public PublicationAddress ReplyToAddress
		{
			get => PublicationAddress.Parse(ReplyTo);
			set => ReplyTo = value.ToString();
		}

		public void ClearContentType() => m_contentType_present = false;
		public void ClearContentEncoding() => m_contentEncoding_present = false;
		public void ClearHeaders() => m_headers_present = false;
		public void ClearDeliveryMode() => m_deliveryMode_present = false;
		public void ClearPriority() => m_priority_present = false;
		public void ClearCorrelationId() => m_correlationId_present = false;
		public void ClearReplyTo() => m_replyTo_present = false;
		public void ClearExpiration() => m_expiration_present = false;
		public void ClearMessageId() => m_messageId_present = false;
		public void ClearTimestamp() => m_timestamp_present = false;
		public void ClearType() => m_type_present = false;
		public void ClearUserId() => m_userId_present = false;
		public void ClearAppId() => m_appId_present = false;
		public void ClearClusterId() => m_clusterId_present = false;

		public bool IsContentTypePresent() => m_contentType_present;
		public bool IsContentEncodingPresent() => m_contentEncoding_present;
		public bool IsHeadersPresent() => m_headers_present;
		public bool IsDeliveryModePresent() => m_deliveryMode_present;
		public bool IsPriorityPresent() => m_priority_present;
		public bool IsCorrelationIdPresent() => m_correlationId_present;
		public bool IsReplyToPresent() => m_replyTo_present;
		public bool IsExpirationPresent() => m_expiration_present;
		public bool IsMessageIdPresent() => m_messageId_present;
		public bool IsTimestampPresent() => m_timestamp_present;
		public bool IsTypePresent() => m_type_present;
		public bool IsUserIdPresent() => m_userId_present;
		public bool IsAppIdPresent() => m_appId_present;
		public bool IsClusterIdPresent() => m_clusterId_present;
	}
}
