using Nocla.Models;
using Nocla.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Google.Apis.Admin.Directory.directory_v1.Data;

namespace Nocla.ViewModels
{
    public class LoginViewModel 
    {
        //LOGIN VIEW MODEL: System must check here for object containing user data and if a user is signed in

       
        public static bool loginStatus = false;
        public static Models.User user;

        public LoginViewModel()
        {
        }

        internal static bool CheckStatus()
        {
            return loginStatus;
        }
        
       
            
        //Login View Model can also update the login status
        public static void updateStatus()
        {
            loginStatus = !loginStatus;
        }

    }
 
   
}
