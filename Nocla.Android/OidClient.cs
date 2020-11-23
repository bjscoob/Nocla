using Nocla.Droid;
using Nocla;
using Firebase.Iid;
using Android.Gms.Extensions;
using WindowsAzure.Messaging;
using System.Threading.Tasks;
using Nocla.Models;

[assembly: Xamarin.Forms.Dependency(typeof(OidClient))]
namespace Nocla.Droid
{
    // Android Implemenation of IODClient Interface
    public class OidClient: IOidClient
    {
        public string PNS { get; set; }

        //get device id
        public string getDeviceID {
            get {
                return MainActivity.deviceID;
            }
        }

        //Registers username to azure notification hub
        public async Task<bool> registerUsername(string user) {
            string[] userParam = { user };
            var instanceIdResult = await FirebaseInstanceId.Instance.GetInstanceId().AsAsync<IInstanceIdResult>();
            var token = instanceIdResult.Token;
            NotificationHub hub = new NotificationHub(AppConstants.NotificationHubName, AppConstants.ListenConnectionString, MainActivity.context);
            await Task.Run(()=> hub.Register(token, userParam ));
            return true;
        }
        
      
      
    }
}