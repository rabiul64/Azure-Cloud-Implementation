﻿namespace TimeSeriesSequence
{
    class program
    {
        /// <summary>
        /// This sample shows a typical experiment code for SP and TM.
        /// You must start this code in debugger to follow the trace.
        /// and TM.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Starting to learn Taxi Passanger data");
            MultiSequenceTaxiPassanger learning_TaxiPassanger = new MultiSequenceTaxiPassanger();

            /// Prediction of taxi passangers based on data set
            //learning_TaxiPassanger.RunPassangerTimeSeriesSequenceExperiment(dataset);
        } 
    }
}
