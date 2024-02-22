# ML21/22-24 - Investigate label prediction from the time-series sequence - Azure Cloud Implementation

## What is your experiment about

The objective is to learn a time-series with a label and predict the label at a given time for a given time-series sequence that has an associated entity or label. The 
label can be the number of passengers in this manner, and the encoded time-series value is the second parameter of the  Learn function. The date and time are then encoded
as a combination of two scalar values, and the time precision is reduced to a precise time via segmentation. The learned model is then used to make predictions, after which the 
outcome is analyzed, and the prediction accuracy is calculated. The user can input any date, time, or date-time
segment, and htmClassifier will return a predicted result. The second task is then used the learned model for prediction. 
The user enters the time segment (01-01-2022 00:18) and the classifier is used to predict the number of passengers.

The source code and documentation for this experiment, which was carried out on a local machine, are available [here](https://github.com/rabiul64/neocortexapi/tree/master/source/MySEProject)

The project indicated above was scaled and developed in the future using cloud computing featuress, the source code is available [here](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2021-2022/tree/Md-Rabiul-Islam/Source/MyCloudProjectSample)

## Guidline for the professor to run the experiment

## Step1 : Message input from azure portal 
It is necessary to start an input message in order to begin the experiment, so I made a message queue in the Azure storage account. Simply call it "trigger-queue." Once you select "add message," you may enter a sample message. Unchecking the box to "Encode the message body in Base64" will allow the message to be sent without encryption.

**How to add message :** 
Azure portal > Home > timeseriessequencecloud > Queues > trigger-queue > Add message

![image](https://user-images.githubusercontent.com/31253296/184973456-5d2e2e86-4b01-4bc5-b862-8c8cebe0976d.png)

Messages added to queue: It is also possible to make any custom configuration files and uploading them to the queue (trigger-queue) of storage account

![image](https://user-images.githubusercontent.com/31253296/184973544-72395e64-be2a-4435-9225-8b74992268dd.png)


### Sample message 
```
{ 
"Name": "Md Rabiul Islam", 
"Description" : "ML21/22-2.2-24", 
"DataSet" : "2021_Green.csv",
"PredicationDateTime" : "01/01/2022 12:15, 01/02/2022 2:10, 02/02/2022 05:05" 
}

```
From the message:
1. Name: Name of the experiment.
2. Description: Description of the experiment.
3. DataSet : Data set that was provided to read green taxi datas for the year 2021 which we stored into Azure Blob storage containe (training-files).
4. PredicationDateTime: user can put the date of which they want the predication of passangers. Date formate will be -- dd/mm/yyyy hh:00

**In order for the experiment to begin, the professor must deliver the message to the queue mentioned above. Then draft a statement that is similar to the sample message-**

Then please go to 'rabiulcontainerinstance' then 'Containers' then select 'logs' to see experiment is started and finally experiment completed successfully. 
The screenshoot reference is here:

![Capture13](https://user-images.githubusercontent.com/31253296/185205246-9067fd1b-16e4-4d92-86bb-865d2066bf9a.PNG)

**The professor can modify the data set parameter in the message body with a new file name and upload various test data in the input blob container as described above in the sample message**

## Step2: Pulling docker image from azure container registry

You can build image and register the remository at Docker container hub from our Docker File

![Capture12](https://user-images.githubusercontent.com/31253296/185202594-c2aaf0ef-0d58-4568-8970-9dae542d536b.PNG)

we can pull the experiment docker image from azure container registry as given below
~~~
docker login -u robiulregistrycontainer -p +uCCadEqfAhhRQtbKU4I40hnEzd0A5t2 robiulregistrycontainer.azurecr.io
~~~
~~~
docker pull robiulregistrycontainer.azurecr.io/mycloudproject:latest
~~~
![Capture11](https://user-images.githubusercontent.com/31253296/185179232-327e0c09-03d8-4ce3-8316-54ea4ececbb9.PNG)

## Step3: (Keep experimental files in an Azure Blob storage container.)
For this approach, primarily started working with a CSV file containing sample data (Taxi-TLC Trip Record Data) that is mostly unsorted. After that read the passenger data from the unsorted sample data set and prepare a new CSV file. It is known to everyone that every sample data set has many columns. However, for this paper, the lpep_pickup_datetime and passenger_count columns are important to consider from the sample data set. The pickup date-time of passengers is then segmented for each 1 hour and recorded in a processed CSV file because every 60 minutes, one segment of a given date is considered. Then read data from the modified CSV file and encode it. For encoding, these four encoders dayEncoder, monthEncoder, segmentEncoder, and dayofWeek have been used to train data. Please note that because the year is static, it is not taken into consideration during encoding. After that htmClassifier learns the sequence in HTM. The learned model is then used to make predictions, after which the outcome is analyzed, and the prediction accuracy is calculated. The user can input any date, time, or date-time segment, and htmClassifier will return a predicted result. The second task is then used the learned model for prediction. The user enters the time segment (01-01-2022 00:18) and the classifier is used to predict the number of passengers
![image](https://user-images.githubusercontent.com/31253296/184977231-283f94fd-5ed2-4b56-8906-0491654ecf42.png)

## Step4: (Describe the experiment result output tables.)

Please use storage browser to view the results of table. screenshoot given below for the reference

| Result          	  | Description 	                                           
|---------------	  |-----------------------------------------------------------|
| Partition Key 	  | Id to Partion a group of Messages result          	      |  
| Row Key        	  | Id to identify each messages result          	      | 
| Timestamp     	  | Timestamp when experiment starts in UTC.                  |  
| Experiment_ID 	  | Id number of Experiment           	                      |        
| Name		     	  | Group name of the project group            	              |        
| Description     	  | Name of the person who did the experiment                 |        
| InputFileUrl    	  | Downloaded input file url   			      |				              |        
| UserPredictedValue      | prediction result of the user input date       	      |     
| Start_TimeUtc    	  | End time of experiment in UTC             	              | 
| Status        	  | Boolean True If experiment executed otherwise False       | 
| Accuracy      	  | Show the learning max accuracy percentage                 | 
| EndTimeUtc          	  | End time of experiment in UTC             	              |              
| Duration_Sec        	  | Execution time in second              	              |        
| OutputFileName          | Experiment Out blob file name at output container         |   

![image](https://user-images.githubusercontent.com/31253296/184979033-50a87173-c0a4-4676-8bcc-df321b296e0d.png)

## Step5: (Output logs/ Result logs)
Upload the output file in the blob storage container. While the experiment is underway, two files are being added to two distinct Azure Storage Blob containers.

1.experiment-logs: Following sequence learning, insert the logs and accuracy for each cycle into a text file. That text file is being added to this container. Download to check the sequence's correctness from that container.

![image](https://user-images.githubusercontent.com/31253296/184980962-f4b50930-414e-47bc-89c1-a32871665347.png)

2. Prediction-files: Using the date and time entered by the user, prediction results of the passengers are stored in a txt file and kept in this container. Download the image to view the predicted outcome for particular container.

![image](https://user-images.githubusercontent.com/31253296/184981076-7692c34c-1493-4a0e-bc99-c3af19c78b69.png)

Check experiment is running from container Instance

![image](https://user-images.githubusercontent.com/31253296/184982079-eae23483-0fc4-4de9-b7b5-832b3246273e.png)

Experiment completed successfully

![image](https://user-images.githubusercontent.com/31253296/184982133-649b6d58-7280-48bb-9cca-b24015fa27bf.png)

## Step6: (Application publish to azure container)
Deployment to azure container registry-

![image](https://user-images.githubusercontent.com/31253296/184983374-cf4c65ca-541b-4fc9-b10c-4518e36bf38d.png)

# Resource group name with created resources

![Capture15](https://user-images.githubusercontent.com/31253296/185911802-f853b5cc-45e3-4ed3-bfcf-7ab1f93a6a8a.PNG)

