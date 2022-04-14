using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Consumer.Tax
{
    class Program
    {
        private static double _totalHold = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Consumer Tax is run");
            Console.WriteLine("");

            //var factory = new ConnectionFactory() { HostName = "localhost" };

            var factory = new ConnectionFactory()
            {
                HostName = "192.168.0.25",
                UserName = "artisUser",
                Password = "250595",
                VirtualHost = "/",
                Port = 5672
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "notifier", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "notifier",
                                  routingKey: String.Empty);

                Console.WriteLine("Waiting for payments . . .");

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (sender, e) =>
                {
                    var body = e.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());

                    var payment = GetPayment(message);
                    _totalHold += payment * 0.01;

                    Console.WriteLine("Received payment: {0}", message);
                    Console.WriteLine("Total hold: {0}", _totalHold);
                };

                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine($"Subscribed to the queue {queueName}");
                Console.WriteLine("Listening . . .");

                Console.ReadLine();
            }
        }

        private static int GetPayment(string message)
        {
            var messageWords = message.Split(' ');

            return int.Parse(messageWords[^1]);
        }
    }
}
