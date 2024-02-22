using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace MyExperiment
{
    public class AzureStorageProvider : IStorageProvider
    {
        private MyConfig config;

        public AzureStorageProvider(IConfigurationSection configSection)
        {
            config = new MyConfig();
            configSection.Bind(config);
        }

        public async Task UploadInputFile(string fileName)
        {
            BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, this.config.TrainingContainer);
            await container.CreateIfNotExistsAsync();

            // Get a reference to a blob named "sample-file"
            BlobClient blob = container.GetBlobClient(fileName);

            await blob.UploadAsync(fileName, true);
        }

        public async Task<string> DownloadInputFile(string fileName)
        {
            BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, this.config.TrainingContainer);

            //string BasePath = AppDomain.CurrentDomain.BaseDirectory;
            //string localFilePath = Path.Combine(BasePath, fileName);
            string localFilePath = fileName;
            string downloadFilePath = localFilePath.Replace(".csv", "_downloaded.csv");
            Console.WriteLine("\nDownloading blob from\n\t{0}\n", localFilePath);
            Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

            // Get a reference to a blob named "sample-file"
            BlobClient blob = container.GetBlobClient(localFilePath);
            //BlobClient blob = container.GetBlobClient(fileName);

            if (await blob.ExistsAsync())
            {
                // Download the blob's contents and save it to a file
                Console.WriteLine($"Downloading blob to: {downloadFilePath}");
                await blob.DownloadToAsync(downloadFilePath);
                return downloadFilePath;
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public async Task UploadExperimentResult(ExperimentResult result)
        {
            // modify fields which you want to upload
            var minimalResult = new ExperimentResult(result.PartitionKey, result.RowKey)
            {
                Timestamp = result.Timestamp,
                ExperimentId = result.ExperimentId,
                Name = result.Name,
                Description = result.Description,
                StartTimeUtc = result.StartTimeUtc,
                EndTimeUtc = result.EndTimeUtc,
                DurationSec = result.DurationSec,
                InputFileUrl = result.InputFileUrl,
                OutputFiles = result.OutputFiles,
                Accuracy = result.Accuracy,
                UserPredictedValues = result.UserPredictedValues,
            };
            Console.WriteLine($"Upload ExperimentResult to table: {this.config.ResultTable}");
            var client = new TableClient(this.config.StorageConnectionString, this.config.ResultTable);

            await client.CreateIfNotExistsAsync();
            try
            {
                await client.AddEntityAsync<ExperimentResult>(minimalResult);
                //await client.UpsertEntityAsync<ExperimentResult>(minimalResult);
                Console.WriteLine("Uploaded to Table Storage completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload to Table Storage: {ex.ToString()}");
            }
            
        }

        public async Task UploadResultFile(string fileName, byte[] data)
        {
            BlobContainerClient container;

            if (fileName.Contains("PredictionExperimentLearning"))
            {
                 container = new BlobContainerClient(this.config.StorageConnectionString, this.config.TrainingLogContainer);
            }
            else
            {
                 container = new BlobContainerClient(this.config.StorageConnectionString, this.config.ResultContainer);
            }

            await container.CreateIfNotExistsAsync();

            try
            {

                string resultFile = fileName.Split(Path.DirectorySeparatorChar).Last();
                BlobClient blobClient = container.GetBlobClient(resultFile);
                Console.WriteLine($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}\n");
                await blobClient.UploadAsync(fileName, true);
                Console.WriteLine("Uploaded to Blob Storage successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload to Blob Storage: {ex.ToString()}");
            }
        }

    }


}
