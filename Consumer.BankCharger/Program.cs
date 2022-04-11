using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Consumer.BankCharger
{
    class Program
    {
        private static int _totalHold = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Consumer Bank is run");
            Console.WriteLine("");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "notifier", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "notifier",
                                  routingKey: String.Empty);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (sender, e) =>
                {
                    var body = e.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());

                    var payment = GetPayment(message);
                    _totalHold += payment;

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
