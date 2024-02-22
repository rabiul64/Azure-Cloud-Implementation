using System;
using System.Threading.Tasks;

namespace Ex07
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Azure Queues World!");

            //QueueStorageSamples samples = new QueueStorageSamples("UseDevelopmentStorage=true");
            QueueStorageSamples samples = new QueueStorageSamples("DefaultEndpointsProtocol=https;AccountName=ex07;AccountKey=vKqwev9SQcr723CM5WE0haYFs7Fi3CZJfCExwxU0jqN/ZplWpKXoa69pADsazciUiIE1C2srzYvO+AStr4JeoA==;EndpointSuffix=core.windows.net");

            //await samples.CreateQueue();

            //await samples.SendMessages();

            await samples.ReceiveMessages();

            //await samples.SendReceiveMessagesAdvanced();

            //await samples.DeleteQueue();

            //await samples.SendManyMessages();

            //await samples.ReceiveManyMessages();

        }
    }
}