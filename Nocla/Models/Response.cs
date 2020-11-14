using System;
using System.Collections.Generic;
using System.Text;

namespace Nocla.Models
{
    //RESPONSE CLASS: Used to handle Response JSON from backend
    //Serializable tag so it can be transformed into JSON text
    [Serializable]
    class Response
    {
        public string response { get; set; }
    }
}
