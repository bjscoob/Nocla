using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using Firebase.Messaging;
using WindowsAzure.Messaging;
using Nocla.Views;
using System.Threading.Tasks;
using Xamarin.Forms;
using Nocla.ViewModels;

namespace Nocla.Droid
{
    //FIREBASE SERVICE: USED TO RECIEVE NOTIFICATIONS FROM AZURE NOTIFICATION HUB
    //COPIED FROM https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/azure-services/azure-notification-hub#xamarinforms-application-functionality
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    
    public class FirebaseService : FirebaseMessagingService
    {
        public static NotificationHub hub;
        public static string tok;
        public override void OnNewToken(string token)
        {
            // NOTE: save token instance locally, or log if desired

            SendRegistrationToServer(token);
            App.Current.Properties["fb_token"] = token;
        }

        void SendRegistrationToServer(string token)
        {
            try
            {
                 hub = new NotificationHub(AppConstants.NotificationHubName, AppConstants.ListenConnectionString, this);

                // register device with Azure Notification Hub using the token from FCM
                Registration registration = hub.Register(token, AppConstants.SubscriptionTags.ToArray());

                // subscribe to the SubscriptionTags list with a simple template.
                string pnsHandle = registration.PNSHandle;
                TemplateRegistration templateReg = hub.RegisterTemplate(pnsHandle, "defaultTemplate", AppConstants.FCMTemplateBody, AppConstants.SubscriptionTags.ToArray());
            }
            catch (Exception e)
            {
                Log.Error(AppConstants.DebugTag, $"Error registering device: {e.Message}");
            }
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            string messageBody = string.Empty;

            if (message.GetNotification() != null)
            {
                messageBody = message.GetNotification().Body;
            }

            // NOTE: test messages sent via the Azure portal will be received here
            else
            {
                messageBody = message.Data.Values.First();
            }

            // convert the incoming message to a local notification
            SendLocalNotification(messageBody);

            // send the incoming message directly to the MainPage
            SendMessageToMainPage(messageBody);
        }

        void SendLocalNotification(string body)
        {
            if(LoginViewModel.user == null) { return; }
            string[] bodydata = body.Split('|');
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("message", body);

            //Unique request code to avoid PendingIntent collision.
            var requestCode = new Random().Next();
            var pendingIntent = PendingIntent.GetActivity(this, requestCode, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, "NoclaNotifyChannel")
                .SetContentTitle("New Message from "+bodydata[0]+"!")
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentText(bodydata[1])
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                notificationBuilder.SetChannelId(AppConstants.NotificationChannelName);
            }

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }

        static async void SendMessageToMainPage(string body)
        {

            Device.BeginInvokeOnMainThread(()=> {
                MsgPage.initMessages();
            });

                
        }


    }
}