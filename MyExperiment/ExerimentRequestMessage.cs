using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyExperiment
{
    internal class ExerimentRequestMessage : IExerimentRequestMessage
    {
        public string ExperimentId { get; set; }
        public string DataSet { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PredicationDateTime { get; set; }
    }
}


/*
 
 {
    "ExperimentId": "sasa",
    "InputFile":"sasss",

}
 
 */ 