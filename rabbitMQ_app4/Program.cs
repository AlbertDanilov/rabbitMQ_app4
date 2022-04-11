using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace rabbitMQ_app4
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Producer is started");
            Console.WriteLine("");

            var random = new Random();
            do
            {
                int timeToSleep = new Random().Next(1000, 3000);
                Thread.Sleep(timeToSleep);

                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "notifier", type: ExchangeType.Fanout);

                    var moneyCount = random.Next(1000, 10000);
                    string message = $"Money count from publisher {moneyCount}";

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "notifier",
                                         routingKey: "",
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine($"Payment received from amount of {moneyCount}.\nNotifying by 'notifier' Exchange");
                }

            } while (true);
        }
    }
}
