using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Nocla.Models
{
    [Serializable]
    //MESSAGE CLASS: Contains data for a message instance in the messages page
    public class Message
    {
        public int id { get; set; }
        public string recipient { get; set; }
        public string photo_url { get; set; }
        public string username { get; set; }
        public string fullname { get; set; }
        public string content { get; set; }
        public string time { get; set; }
        public string date { get; set; }


    }
}
