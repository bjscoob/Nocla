using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Nocla.Droid;
using Nocla.Models;
using Auth0.OidcClient;
using Firebase.Iid;
using Android.Gms.Extensions;
using WindowsAzure.Messaging;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(OidClient))]
namespace Nocla.Droid
{
    // Android Implemenation of IODClient Interface
    public class OidClient: IOidClient
    {
        public string PNS { get; set; }
        public string LoginAsyncResult
        {
            get
            {
                return "";
            }
        }

        //get device id
        public string getDeviceID {
            get {
                return MainActivity.deviceID;
            }
        }

        //Registers username to azure notification hub
        public async void registerUsername(string user) {
           
            string[] userParam = { user };
            var instanceIdResult = await FirebaseInstanceId.Instance.GetInstanceId().AsAsync<IInstanceIdResult>();
            var token = instanceIdResult.Token;
            NotificationHub hub = new NotificationHub(AppConstants.NotificationHubName, AppConstants.ListenConnectionString, MainActivity.context);
            await Task.Run(()=> hub.Register(token, userParam ));
        }
      
    }
}