using RabbitMQ.Client;
using System.Text;

namespace test.Services
{
    public class RabbitSenderService
    {
        public void SendCppData(string data)
        {
            ConnectionFactory factory = new();
            factory.Uri = new Uri("amqp://user:qLyb2+PE9uPh@20.107.195.151:5672");
            factory.ClientProvidedName = "Rabbit Sender App";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();

            string exchangeName = "CppExchange";
            string routingKey = "cpp-routing-key";
            string queueName = "CppQueue";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            Console.WriteLine("Sending Message...");
            //string messageArray = JsonSerializer.Serialize(message);
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(data);
            channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);

            channel.Close();
            cnn.Close();
        }
        public void SendPyData(string data)
        {
            ConnectionFactory factory = new();
            factory.Uri = new Uri("amqp://user:qLyb2+PE9uPh@20.107.195.151:5672");
            factory.ClientProvidedName = "Rabbit Sender App";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();

            string exchangeName = "PyExchange";
            string routingKey = "py-routing-key";
            string queueName = "pyQueue";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            Console.WriteLine("Sending Message...");

            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(data);
            channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);

            channel.Close();
            cnn.Close();
        }
    }
}
