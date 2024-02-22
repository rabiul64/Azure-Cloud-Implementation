using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using MyExperiment;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyCloudProject
{
    class Program
    {
        /// <summary>
        /// Your project ID from the last semester.
        /// </summary>
        private static string projectName = "ML21/22-24";
        // Project Name: Investigate Label Prediction from the time-series sequence

        static async Task Main(string[] args)
        {
            CancellationTokenSource tokeSrc = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                tokeSrc.Cancel();
            };

            Console.WriteLine($"Started experiment: {projectName}");

            //init configuration
            var cfgRoot = Common.InitHelpers.InitConfiguration(args);

            var cfgSec = cfgRoot.GetSection("MyConfig");

            // InitLogging
            var logFactory = InitHelpers.InitLogging(cfgRoot);
            var logger = logFactory.CreateLogger("Train.Console");

            logger?.LogInformation($"{DateTime.Now} -  Started experiment: {projectName}");

            IStorageProvider storageProvider = new AzureStorageProvider(cfgSec);

            Experiment experiment = new Experiment(cfgSec, storageProvider, logger, null/* put some additional config here */);

            /// <summary>
            /// Below two functions are implemented using Azure HTTP Trigger Function which puts the msg on queue
            /// </summary>
            //await experiment.PutFileOnBlobStorage(dataset);
            //await experiment.PutProcessMessageOnQueue(dataset, projectName);


            /*
             * to put msg on queue, either use any tool which makes HTTPS POST Request or cURL
             * Test 1:
             * curl -H "Content-Type: application/json" -d '{ "name": "Mandar", "projectname" : "ML19/20-2.2-24", "dataset" : "rec-center-hourly.csv" }' https://azmycloudproject.azurewebsites.net/api/HttpTrigger1?code=-_clsSzYRBKbHf7Z1spcbvbO3rmasMRfNGalgeCi2jaBAzFuI_liCg==
             * 
             * Test 2:
             * curl -H "Content-Type: application/json" -d '{ "name": "Mandar", "projectname" : "ML19/20-2.2-24", "dataset" : "rec-center-hourly-og.csv" }' https://azmycloudproject.azurewebsites.net/api/HttpTrigger1?code=-_clsSzYRBKbHf7Z1spcbvbO3rmasMRfNGalgeCi2jaBAzFuI_liCg==
             * 
             */
            await experiment.RunQueueListener(tokeSrc.Token);

            logger?.LogInformation($"{DateTime.Now} -  Experiment exit: {projectName}");
        }


    }
}
