using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Nocla.Models
{
    //User CLASS: Contains data for the current signed in user
    public class User
    {
        public string token { get; set; }
        public int employee_id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }

        //Returns first and last name of user
        internal string getFLName()
        {
            return firstname + " " + lastname; 
        }
        internal string getUsername() {
            return username;
        }
        string position { get; set; }
        string shift { get; set; }
        int manager { get; set; }
        int pm_assignment { get; set; }
        string status { get; set; }


        //Creates user, requires string array of data
        //***Note: Log failed attempts to php logbook
        public User(string[] data) {
            try
            {
                token = data[0];
                employee_id = Int32.Parse(data[1]);
                username = data[2];
                email = data[3];
                firstname = data[4];
                lastname = data[5];
                position = data[6];
                shift = data[7];
                manager = Int32.Parse(data[8]);
                pm_assignment = Int32.Parse(data[9]);
                status = data[10];
            }
            catch (Exception e)
            {
                Console.WriteLine(data.ToString());
            }
        
        }

    }
}
