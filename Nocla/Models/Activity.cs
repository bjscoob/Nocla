using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace Nocla.Models
{
    //ACTIVITY CLASS: Contains data for an activity insatnce for the PM View page
    public class Activity
    {
        public int activity_id { get; set; }
        public int pm_no  { get; set; }
        public string content { get; set; }
        public string activity_time { get; set; }
        public string author { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string color_text { get; set; }
        
        //Returns string representation for object. '|' character used as delimeter for parsing by PHP backend
        public override string ToString()
        {
            return "blah";
        }
        private Color backgroundColor = Color.Transparent;
        public Color BackgroundColor
        {
            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                }
            }
            get
            {
                return backgroundColor;
            }
        }
      
    }

 

}
