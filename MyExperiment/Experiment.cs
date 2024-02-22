using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TimeSeriesSequence;

namespace MyExperiment
{
    /// <summary>
    /// This class implements the ML experiment that will run in the cloud. This is refactored code from my SE project.
    /// </summary>
    public class Experiment : IExperiment
    {
        private IStorageProvider storageProvider;

        private ILogger logger;

        private MyConfig config;

        private IExerimentRequestMessage expReqMsg;

        public Experiment(IConfigurationSection configSection, IStorageProvider storageProvider, ILogger log, IExerimentRequestMessage expReqMsg)
        {
            this.storageProvider = storageProvider;
            this.logger = log;
            this.expReqMsg = expReqMsg;
            config = new MyConfig();
            configSection.Bind(config);
        }

        public Task<ExperimentResult> Run(string inputFile, string PredicationDateTime)
        {
            // TODO read file
            
            Random rnd = new Random();
            int randomnumber = rnd.Next(0, 1000);
            string rowKey = "key" + randomnumber.ToString();
            
            // YOU START HERE WITH YOUR SE EXPERIMENT!!!!
            // Run your experiment code here.
            ExperimentResult res = new ExperimentResult(this.expReqMsg.ExperimentId, rowKey);
            MultiSequenceTaxiPassanger mseq = new MultiSequenceTaxiPassanger();
            
            res.StartTimeUtc = DateTime.UtcNow;
            mseq.RunPassangerTimeSeriesSequenceExperiment(inputFile, PredicationDateTime);
            res.EndTimeUtc = DateTime.UtcNow;
            res.Accuracy = mseq.ExpAccuracy.ToString(); 
            res.InputFileUrl = inputFile;
            res.DurationSec = mseq.ElapsedTime;
            res.ExperimentId = this.expReqMsg.ExperimentId;
            res.Name = this.expReqMsg.Name;
            res.Description = this.expReqMsg.Description;
            res.Timestamp = DateTime.UnixEpoch;
            res.OutputFiles = mseq.OutputPath;
            res.PredictionOutputFiles = mseq.PredictionOutputPath;

            if (mseq.UserPredictedValues.Count == 0)
                res.UserPredictedValues = null;
            else
                res.UserPredictedValues = Newtonsoft.Json.JsonConvert.SerializeObject(mseq.UserPredictedValues);

            return Task.FromResult<ExperimentResult>(res);
        }



        /// <inheritdoc/>
        public async Task RunQueueListener(CancellationToken cancelToken)
        {
            QueueClient queueClient = new QueueClient(this.config.StorageConnectionString, this.config.Queue);
            await queueClient.CreateIfNotExistsAsync();
            ExerimentRequestMessage request = null;
            while (cancelToken.IsCancellationRequested == false)
            {

                QueueMessage message = await queueClient.ReceiveMessageAsync();

                if (message != null)
                {
                    try
                    {

                        //for some unknown reason the message is base64 encoded
                        //string msgTxt = Encoding.UTF8.GetString(Convert.FromBase64String(message.Body.ToString()));

                        this.logger?.LogInformation($"{DateTime.Now} -  RunQueueListener: got msg - {message.Body}");

                        request = JsonSerializer.Deserialize<ExerimentRequestMessage>(message.Body);
                        this.expReqMsg = request;
                        this.expReqMsg.ExperimentId = message.MessageId;

                        //dequeue the msg as soon as done reading
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                        var inputFile = await this.storageProvider.DownloadInputFile(request.DataSet);
                        this.logger?.LogInformation($"{DateTime.Now} -  RunQueueListener: download complete - {request.DataSet}");

                        this.logger?.LogInformation($"{DateTime.Now} -  RunQueueListener: running experiment - {request.DataSet}");
                        ExperimentResult result = await this.Run(inputFile, request.PredicationDateTime);

                        //TODO. do serialization of the result.
                        this.logger?.LogInformation($"{DateTime.Now} -  UploadResultFile: file - {result.OutputFiles}");
                        await storageProvider.UploadResultFile(result.OutputFiles, null);

                        //TODO. do serialization of the result.
                        this.logger?.LogInformation($"{DateTime.Now} -  UploadResultFile: file - {result.OutputFiles}");
                        await storageProvider.UploadResultFile(result.PredictionOutputFiles, null);

                        this.logger?.LogInformation($"{DateTime.Now} -  UploadExperimentResult...");
                        await storageProvider.UploadExperimentResult(result);
                        this.logger?.LogInformation($"{DateTime.Now} -  Experiment Completed Successfully...");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now} - Error RunQueueListener: {ex.ToString()}");
                        this.logger?.LogError(ex, $"{DateTime.Now} - Error RunQueueListener: ");
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                    }
                }
                else
                {
                    await Task.Delay(500);
                    logger?.LogTrace("Queue empty...");
                    logger?.LogTrace("Waiting forf trigger message...");
                }
            }

            this.logger?.LogInformation("Cancel pressed. Exiting the listener loop.");
        }

        public async Task PutFileOnBlobStorage(string dataset)
        {
            logger?.LogInformation($"{DateTime.Now} -  UploadInputFile: {dataset}");
            await storageProvider.UploadInputFile(dataset);
        }

        public async Task PutProcessMessageOnQueue(string datasetURL, string projectName)
        {
            ExerimentRequestMessage request = new ExerimentRequestMessage();

            QueueClient queue = new QueueClient(this.config.StorageConnectionString, this.config.Queue);
            await queue.CreateIfNotExistsAsync();

            /*
             * https://stackoverflow.com/questions/52390149/how-to-generate-guid-from-datetime
             * 
             */
            var today = DateTime.UtcNow;
            var bytes = BitConverter.GetBytes(today.Ticks);
            Array.Resize(ref bytes, 16);
            var guid = new Guid(bytes);

            /* just in case needs to be in human readble format
             * 
             * var dateBytes = guid.ToByteArray();
             * Array.Resize(ref dateBytes, 8);
             * var date = new DateTime(BitConverter.ToInt64(dateBytes));
             */

            request.ExperimentId = guid.ToString();
            request.DataSet = datasetURL;
            request.Name = projectName;
            request.Description = "";

            var json = JsonSerializer.Serialize(request);
            logger?.LogInformation($"{DateTime.Now} -  PutProcessMessageOnQueue: {json}");
            await queue.SendMessageAsync(json);
        }



        #region Private Methods


        #endregion
    }
}
