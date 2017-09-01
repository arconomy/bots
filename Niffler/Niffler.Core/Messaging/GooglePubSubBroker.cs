using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Protobuf.Collections;
using Google.Protobuf;

namespace Niffler.Messaging
{

   
    class GooglePubSubBroker
    {
        //public IConnection Connection { get; }
        private PublisherClient PublisherClient;
        private string ProjectId;

        public GooglePubSubBroker()
        {
            //ConnectionFactory factory = new ConnectionFactory();
            //// "guest"/"guest" by default, limited to localhost connections
            //factory.UserName = user;
            //factory.Password = pass;
            //factory.VirtualHost = vhost;
            //factory.HostName = hostName;

            //Connection = factory.CreateConnection();
            //ConnectionFactory factory = new ConnectionFactory();
            //factory.Uri = "amqp://user:pass@hostName:port/vhost";
            CreatePublisherClient();
            ProjectId = System.Configuration.ConfigurationManager.AppSettings["gcloud-project-id"];
        }

        //Create PublisherClient
        private async void CreatePublisherClient()
        {
                PublisherClient = await PublisherClient.CreateAsync();
        }

        //Create Topic per Rule
        public TopicName CreatePubSubTopic(String topicId)
        {
            TopicName topicName = new TopicName(ProjectId, topicId);
            PublisherClient.CreateTopic(topicName);
            return topicName;
        }

        //Create message payload
        public PubsubMessage GetPubSubMessage(bool ruleActionCompleted, MapField<String, String> attributes)
        {
            attributes.Add("RuleActionExecuted", ruleActionCompleted.ToString());

             PubsubMessage message = new PubsubMessage
            {
                // The data is any arbitrary ByteString. Here, we're using text.
                Data = ByteString.CopyFromUtf8("Rule action executed notification"),
                // The attributes provide metadata in a string-to-string dictionary.
                Attributes = { attributes }
            };
            return message;
        }
    }
}
