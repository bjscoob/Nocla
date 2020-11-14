using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nocla.Models
{
    //MATERIAL ASSIGNMENT CLASS: Contains data for a material assignment instance for the PM View page
    //Serializable tag so it can be transformed into JSON text
    [Serializable]
    public class MaterialAssignment
    {
        public string pm { get; set; }
        public string material { get; set; }
        public string batchno { get; set; }
        public string bagno { get; set; }
        public string assigndate { get; set; }
        public string assigntime { get; set; }
        public string expdate { get; set; }
        public string exptime { get; set; }
        public string iscurrent { get; set; }

        public string toString() {
            return String.Format("PM: {0} Material: {1} Batch: {2} Bag: {3} Assigned: {4}{5} Exp:{6}{7}   isCurrent:{8}"
                ,pm,material,batchno,bagno,assigndate,assigntime,expdate,exptime,iscurrent);
        }
    }
}
