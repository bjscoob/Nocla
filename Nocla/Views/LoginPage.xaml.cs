﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Android.Webkit;
using Newtonsoft.Json;
using Nocla.Models;
using Nocla.Templates;
using Nocla.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Nocla.Views
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        //LOGIN PAGE: CONTROLS WEBVIEW AND SPINNING GEAR
        private static Image gearIcon;
        private static bool performRotation = false;
        private static bool firstAnim = true;
        Stopwatch stopwatch = new Stopwatch();
        long incoArraySize;
        long outgoArraySize;
        long time;
        int retryCount = 0;
        //RETRIEVE DEVICE ID (PLATFORM SPECIFIC CODE) ***MOVE TO VIEW MODEL***
        public static string deviceId = DependencyService.Get<IOidClient>().getDeviceID;
        public bool popupIsTicket = false;


        public LoginPage()
        {

            //REMOVE OLD BROWSER DATA
            var cookieManager = CookieManager.Instance;
            cookieManager.RemoveAllCookie();
            InitializeComponent();




            //GET GEAR ICON
            gearIcon = gear;

            


            //REMOVE TITLE BAR AND BOTTOM NAV BAR
            Shell.SetTabBarIsVisible(this, false);


            spinGear();
        }
        protected override void OnAppearing()
        {
            //Hide popUp Box
            popBox.FadeTo(0, 1500);
            popBox.IsVisible = false;
            if (Application.Current.Properties.ContainsKey("username")) {
                userEntry.Text = Application.Current.Properties["username"] as string;
            }
        }
            //MOVE TO LOGIN VIEW MODEL
      

        //SPINS GEAR ICON INDEFINITELY 
        public async static void spinGear()
        {
            while (true)
            {
                await gearIcon.RotateTo(360, 800, Easing.Linear);
                await gearIcon.RotateTo(0, 0); // reset to initial position
                if (LoginViewModel.CheckStatus()) { return; }
            }
        }

        //Disable Back Button Press 
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
        async void login(object sender, EventArgs e)
        {
            if (userEntry.Text == null || passEntry.Text == null) { return; }
            else
            {
                var result = await authenticate();

                string[] userdata = result.Split('|');
                if (userdata.Length == 1)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Login Error", result, "OK");
                    });
                }
                else
                {
                    
                       
                            
                            LoginViewModel.user = new User(userdata);
                            LoginViewModel.updateStatus();
                            MsgPage.updateName(LoginViewModel.user.getFLName());
                            await DependencyService.Get<IOidClient>().registerUsername(LoginViewModel.user.getUsername());
                            AppConstants.SubscriptionTags.Add(LoginViewModel.user.getUsername());
                            Application.Current.Properties["username"] = LoginViewModel.user.getUsername();
                            settingsPage.initUserData(userdata);
                            await Navigation.PopAsync();
                        
                    
                }
            }
        }

        private async Task<string> authenticate()
        {

            var url = "https://jax-apps.com/auth.php";

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", userEntry.Text.ToString()),
                new KeyValuePair<string, string>("pswd", passEntry.Text.ToString())
            });
            // get outgoing packet size
            outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

            //start stopwatch
            stopwatch.Restart();

            //perform POST operation
            var myHttpClient = new HttpClient();

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await myHttpClient.PostAsync(url, formContent);



                if (response.Content == null)
                {
                    return "Unable to reach server";
                }
                var result = await response.Content.ReadAsStringAsync();
                retryCount = 0;
                //get incoming packet size
                incoArraySize = (await response.Content.ReadAsByteArrayAsync()).Length;

                //get elapsed time and display diagnostic data
                stopwatch.Stop();
                time = stopwatch.ElapsedMilliseconds;
                string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
                await DisplayAlert("Login Operation", diag, "OK");

                return result;
            }


            catch (Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    authenticate();
                }
                else
                {
                  return e.GetType().ToString();
                  
                }
            }
            return "Unknown Error";
        }
        async void openForgotPopup(object sender, EventArgs e)
        {
            restorePopup();
            popupCreate.IsVisible = true;
            AnimateIn();
        }
        async void openTicketPopup(object sender, EventArgs e)
        {
            popupIsTicket = true;
            popupHeader.Text = "Submit a Ticket";
            emailEntry.IsVisible = false;
            popupLabel.Text = "Please state the issue:";
            ticketEntry.IsVisible = true;
            popupCreate.IsVisible = true;
            popBox.IsVisible = true;
            AnimateIn();
        }

        //Respond to popup send button
        async void sendEmail(object sender, EventArgs e)
        {
            if (popupIsTicket)
            {
                try
                {
                    var message = new EmailMessage
                    {
                        Subject = "Nocla Ticket",
                        Body = ticketContent.Text.ToString(),
                        To = new List<string>() { "jacksonb424@gmail.com"},
                        //Cc = ccRecipients,
                        //Bcc = bccRecipients
                    };
                    await Email.ComposeAsync(message);
                }
                catch (FeatureNotSupportedException fbsEx)
                {
                    // Email is not supported on this device
                }
                catch (Exception ex)
                {
                    // Some other exception occurred
                }
            }
            // SEND EMAIL TO USER
            else
            { 
                var uri = new Uri("https://jax-apps.com/rp.php?email=" + emailEntry.Text.ToString());
                HttpClient myClient = MsgPage.client;

                // get outgoing packet size
                outgoArraySize = (await myClient.GetByteArrayAsync(uri)).Length;

                //start stopwatch
                stopwatch.Restart();

                //perform GET operation
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await myClient.GetAsync(uri);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount < 11)
                    {
                        sendEmail(sender, e);
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async () => {
                            await DisplayAlert("Connection Error", ex.GetType().ToString(), "OK");
                        });

                        return;
                    }
                }

                //get elapsed time and display diagnostic data
                stopwatch.Stop();
                //get incoming packet size
                if (response.Content == null)
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Server Error", "Unable to reach server", "OK");
                    });
                    return;
                }
                incoArraySize = (await response.Content.ReadAsByteArrayAsync()).Length;
                retryCount = 0;
                time = stopwatch.ElapsedMilliseconds;
                string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
                await DisplayAlert("Forgot Login Operation", diag, "OK");
                if (response.IsSuccessStatusCode)
                {

                    var content = await response.Content.ReadAsStringAsync();
                    string result = "";
                    try { 
                        Response re = JsonConvert.DeserializeObject<Response>(content);
                        result = re.response;
                    }
                    catch {
                        result = "Sorry. That email could not be found.";
                    }
                    
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Account Recovery", result, "OK");
                    });
                }
            }
        }

        //respond to popup's red button
        async void exitPopup(object sender, EventArgs e)
        {

            AnimateOut();
        }

        //restores popup to forgot password state
        void restorePopup()
        {
            popupIsTicket = false;
            popupHeader.Text = "Forgotten Username/Password";
            emailEntry.IsVisible = true;
            popupLabel.Text = "Enter your Email: ";
            ticketEntry.IsVisible = false;
        }
        private async void AnimateIn()
        {
            
            if (firstAnim) {
                popBox.IsVisible = true;
                popBox.FadeTo(1, 500);
                firstAnim = false;
            }
            else { popBox.TranslateTo(0, 0, 1200, Easing.SpringOut); }
        }

        private async void AnimateOut()
        {
            
            popBox.TranslateTo(0, popupCreate.Height, 1200, Easing.SpringOut);
            popupCreate.IsVisible = false;
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);



            popBox.TranslationY = popupCreate.Height;
        }
    }
}
       
