using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace test.Services
{
    public class RabbitReceiverService
    {
        public string ReceiveCppData()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://user:qLyb2+PE9uPh@20.107.195.151:5672");
            factory.ClientProvidedName = "Rabbit Receiver App";

            using (IConnection cnn = factory.CreateConnection())
            using (IModel channel = cnn.CreateModel())
            {
                string exchangeName = "CppExchange";
                string routingKey = "cpp-routing-key";
                string queueName = "CppQueue";

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);
                channel.BasicQos(0, 1, false);

                while (true)
                {
                    BasicGetResult result = channel.BasicGet(queueName, true);
                    if (result != null)
                    {
                        var body = result.Body.ToArray();
                        string message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Message Received: {message}");
                        channel.BasicAck(result.DeliveryTag, false);
                        return message;
                    }

                    Thread.Sleep(1000); // Wait for 1 second before checking the queue again
                }
            }

            
        }

        public string ReceivePyData()
        {
            ConnectionFactory factory = new();
            factory.Uri = new Uri("amqp://user:qLyb2+PE9uPh@20.107.195.151:5672");
            factory.ClientProvidedName = "Rabbit Receiver App";

            using (IConnection cnn = factory.CreateConnection())
            using (IModel channel = cnn.CreateModel())
            {
                string exchangeName = "PyExchange";
                string routingKey = "py-routing-key";
                string queueName = "pyQueue";

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);
                channel.BasicQos(0, 1, false);

                while (true)
                {
                    BasicGetResult result = channel.BasicGet(queueName, true);
                    if (result != null)
                    {
                        var body = result.Body.ToArray();
                        string message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Message Received: {message}");
                        channel.BasicAck(result.DeliveryTag, false);
                        return message;
                    }

                    Thread.Sleep(1000); // Wait for 1 second before checking the queue again
                }
            }
        }

    }
}
