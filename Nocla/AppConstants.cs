using System;
using System.Collections.Generic;
using System.Text;

namespace Nocla
{
    public static class AppConstants
    {
        public static string NotificationChannelName { get; set; } = "NoclaNotifyChannel";
        public static string NotificationHubName { get; set; } = "noclaNotifyHub";
        public static string ListenConnectionString { get; set; } = "Endpoint=sb://noclahub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=lkTVul72HMyEzy5lETKbW8JVCo2Y4qgl/KExbaMlHXM=";
        public static string DebugTag { get; set; } = "XamarinNotify";
        public static List<string> SubscriptionTags = new List<string>(){ "default" };
        public static string FCMTemplateBody { get; set; } = "{\"data\":{\"message\":\"$(messageParam)\"}}";
        public static string APNTemplateBody { get; set; } = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";
    }
}
