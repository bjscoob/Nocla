using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Auth0.OidcClient;
using Foundation;
using Nocla.iOS;
using Nocla.Models;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(OidClient))]
namespace Nocla.iOS
{
    class OidClient: IOidClient
    {
        public string PNS { get; set; }
      
        public string getDeviceID
        {
            get {
                return "todo";
            }
        }
        public async void registerUsername(string user)
        {
        }

    }
}