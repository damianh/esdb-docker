using System;
using System.Threading.Tasks;
using Ductus.FluentDocker.Builders;
using EventStore.ClientAPI;

namespace esdb_docker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var tcpPort = 1113;

            var containerService = new Builder()
                .UseContainer()
                .WithName("esdb")
                .UseImage("eventstore/eventstore:20.6.1-bionic")
                .Command("--insecure --enable-external-tcp")
                .ReuseIfExists()
                .ExposePort(tcpPort, tcpPort)
                .WaitForPort($"{tcpPort}/tcp", TimeSpan.FromSeconds(20))
                .Build()
                .Start();

            var connectionString = $"ConnectTo=tcp://admin:changeit@localhost:{tcpPort};UseSslConnection=false";

            using (var eventStoreConnection = EventStoreConnection.Create(connectionString))
            {
                await eventStoreConnection.ConnectAsync();

                var allEventsSlice = await eventStoreConnection.ReadAllEventsForwardAsync(Position.Start, 100, true);
            }
        }
    }
}
