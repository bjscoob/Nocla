using System;
using System.Collections.Generic;
using System.Linq;
using Auth0.OidcClient;
using Foundation;
using UIKit;

namespace Nocla.iOS
{
    public class Application
    {
        internal static Auth0Client client;

        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
             client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = "nocla.us.auth0.com",
                ClientId = "CgrY3E0D8BU0PQPHyjWxJxcLKgDR4L2i"
            });
        }
    }
}
