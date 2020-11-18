using Newtonsoft.Json.Linq;
using Nocla.Models;
using Nocla.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace Nocla.Views
{

    public partial class MsgPage : ContentPage
    {
        //MESSAGES PAGE: CONTROLS USER IMAGE, NAME LABELS, MENU BAR, SEARCH BAR, AND MESSAGES LIST

        public static HttpClient client;
        public static Label nameField;
        public static List<Contact> contactBank;
        public static List<string> Recipients;
        public static List<Message> messages;
        public static List<Message> messageBank;
        public static ListView messageList;
        private static long outgoArraySize;
        private static Stopwatch stopwatch = new Stopwatch();
        private static long incoArraySize;
        private static long time;
        private static int retryCount=0;

        public MsgPage()
        {
            //MAKE NEW HTTPCLIENT TO TALK TO PHP BACKEND
            client = new HttpClient();
            client.CancelPendingRequests();
            //this is the first page to show on launch, the login page is forced on top if login check is not valid
            if (!LoginViewModel.CheckStatus()) goToLogin();
            InitializeComponent();
            nameField = Fullname;
            messageList = msgList;
            this.BindingContext = this;

        }

        //CHANGES USER FIRST AND LAST NAME AT TOP OF SCREEN
        public static void updateName(string name) {
            nameField.Text = "Hello, " + LoginViewModel.user.getFLName();
            initContactBank();
            initMessages();
        }

        public async static void initMessages() {

            List<Message> messages = new List<Message>();
            var uri = new Uri("https://jax-apps.com/msgapi.php");
            HttpClient myClient = MsgPage.client;
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("getMsg", "blah"),
                new KeyValuePair<string, string>("r", LoginViewModel.user.getUsername())
            });

            // get outgoing packet size
            outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

            //start stopwatch
            stopwatch.Restart();
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                result = await myClient.PostAsync(uri, formContent);
            }
            catch (Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    initMessages();
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await App.Current.MainPage.DisplayAlert("Connection Error", e.GetType().ToString(), "OK");
                    });
                    return;
                }
            }
            //get elapsed time and display diagnostic data
            stopwatch.Stop();
            if (result.Content == null)
            {
                Device.BeginInvokeOnMainThread(async () => {
                    await App.Current.MainPage.DisplayAlert("Server Error", "Unable to reach server", "OK");
                });
                return;
            }
            //get incoming packet size
            incoArraySize = (await result.Content.ReadAsByteArrayAsync()).Length;
            time = stopwatch.ElapsedMilliseconds;
            string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
            await App.Current.MainPage.DisplayAlert("Get Messages", diag, "OK");

            var content = await result.Content.ReadAsStringAsync();
            retryCount = 0;
            JArray a = JArray.Parse(content);
            foreach (JObject O in a.Children<JObject>())
            {
                List<string> msgdata = new List<string>();
                foreach (JProperty P in O.Properties())
                {

                    msgdata.Add((string)P.Value);

                }
                int id = Int32.Parse(msgdata[0]);
                string dt = DateTime.Parse(msgdata[6]).ToString("dd MMM yyyy");
                string t = DateTime.Parse(msgdata[6]).ToString("HH:mm");
                messages.Add(new Message
                {
                    id = id,
                    username = msgdata[3],
                    fullname = msgdata[4],
                    photo_url = msgdata[2],
                    content = msgdata[5],
                    date = dt,
                    time = t
                });



            }
            messageList.ItemsSource = messages;
            messageBank = messages;


        }
        private async static void initContactBank()
        {
            contactBank = new List<Contact>();
            var uri = new Uri("https://jax-apps.com/msgapi.php");
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("getUsers", "blah")
            });
            // get outgoing packet size
            outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

            //start stopwatch
            stopwatch.Restart();
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                result = await client.PostAsync(uri, formContent);
            }
            catch (Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    initContactBank();
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await App.Current.MainPage.DisplayAlert("Connection Error", e.GetType().ToString(), "OK");
                    });
                    return;
                }
            }
            //get elapsed time and display diagnostic data
            stopwatch.Stop();
            if (result.Content == null)
            {
                Device.BeginInvokeOnMainThread(async () => {
                    await App.Current.MainPage.DisplayAlert("Server Error", "Unable to reach server", "OK");
                });
                return;
            }
            //get incoming packet size
            incoArraySize = (await result.Content.ReadAsByteArrayAsync()).Length;
            retryCount = 0;
            time = stopwatch.ElapsedMilliseconds;
            string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
            await App.Current.MainPage.DisplayAlert("Get Contacts", diag, "OK");

            var content = await result.Content.ReadAsStringAsync();
            JArray a = JArray.Parse(content);
            foreach (JObject O in a.Children<JObject>())
            {
                List<string> contactdata = new List<string>();
                foreach (JProperty P in O.Properties())
                {

                    contactdata.Add((string)P.Value);

                }
                int id = Int32.Parse(contactdata[0]);
                contactBank.Add(new Contact
                {
                    contactId= id,
                    username = contactdata[1],
                    firstname = contactdata[2],
                    lastname = contactdata[3],
                    isContactGroup = contactdata[4] == "1"
                });



            }
        }

        //FORCES APP TO MOVE TO LOGIN SCREEN IF USER HAS NOT YET SIGNED IN OR SIGNS OUT
        async void goToLogin()
        {
            await Navigation.PushAsync(new LoginPage());
        }

        //forces popup message form to become visible
        async void popupNewMsg(object sender, EventArgs e) {
            popupMessageCreate.IsVisible = true;
            searchContact.Text = "";
            messageContent.Text = "";
            Recipients = new List<string>();
            recipientList.Children.Clear();
            searchContact.Text = "";
            Task.Delay(1000);
            contactSuggestions.ItemsSource = new List<String>();
        }


        //closes popup message screen
        async void exitNewMsg(object sender, EventArgs e)
        {
            popupMessageCreate.IsVisible = false;
        }
        async void addToContacts(object sender, EventArgs e) {
            Label l = (Label)sender;
            string s = l.Text.ToString().ToUpper();
            ColorTypeConverter converter = new ColorTypeConverter();
            s = strip(s);
            if (!Recipients.Contains(s))
            {
                Button newButton = new Button
                {
                    Text = s,
                    BackgroundColor = (Color)(converter.ConvertFromInvariantString("#003595")),
                    WidthRequest = 100,
                    HeightRequest = 30,
                    TextColor = Color.White,
                    FontSize = 12,
                    Padding = 1,
                    VerticalOptions = LayoutOptions.Center,
                    


                };
                newButton.Clicked += removeButton;
                recipientList.Children.Add(newButton);
                Recipients.Add(s);
                l.BackgroundColor = (Color)(converter.ConvertFromInvariantString(" #ff33b5e5"));
            }

            
        }
      

        private async void removeButton(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string s = b.Text.ToString();
            bool repsonse = await DisplayAlert("Remove Recipients", "Remove " + s + " from recipients?", "YES", "NO ");
            if (repsonse) {
                Recipients.Remove(s);
                b.IsVisible = false;
            }
        }

        //Validates Message form data and sends if successfull  ***NOTE**: ADD vALIDATION CODE
        async void sendNewMsg(object sender, EventArgs e) {
            
            string content = messageContent.Text.ToString();
            foreach (string r in Recipients) {
                sendMsg(r, content);
            }
            popupMessageCreate.IsVisible = false;
            
        }
        async void sendMsg(string recip, string cont) {
            HttpClient myClient = MsgPage.client;
            var stringContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("recipient", recip),
                new KeyValuePair<string, string>("sender", LoginViewModel.user.username),
                new KeyValuePair<string, string>("senderFL", LoginViewModel.user.getFLName()),
                new KeyValuePair<string, string>("content", cont),
                new KeyValuePair<string, string>("auth_token", LoginViewModel.user.token),

            });
            // get outgoing packet size
            outgoArraySize = (await stringContent.ReadAsByteArrayAsync()).Length;

            //start stopwatch
            stopwatch.Restart();
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                result = await myClient.PostAsync("https://jax-apps.com/notapi.php", stringContent);
            }
            catch (Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    sendMsg(recip, cont);
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Connection Error", e.GetType().ToString(), "OK");
                    });
                    return;
                }
            }
            //get elapsed time and display diagnostic data
            stopwatch.Stop();
            if (result.Content == null)
            {
                Device.BeginInvokeOnMainThread(async () => {
                    await DisplayAlert("Server Error", "Unable to reach server", "OK");
                });
                return;
            }
            //get incoming packet size
            incoArraySize = (await result.Content.ReadAsByteArrayAsync()).Length;
            retryCount = 0;
            time = stopwatch.ElapsedMilliseconds;
            string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
            await App.Current.MainPage.DisplayAlert("Send Message", diag, "OK");
        }
        void msgUpdate(object sender, EventArgs e) {
            SearchBar sb = (SearchBar)sender;
            string currText = sb.Text.ToString().ToUpper();
            if (!string.IsNullOrEmpty(currText))
            {
                List<Message> currMsgs = new List<Message>();
                foreach (Message m in messageBank)
                {
                    string user = m.username.ToUpper();
                    string full = m.fullname.ToUpper();
                    string content = m.content.ToUpper(); 
                    if (user.Contains(currText)||full.Contains(currText)||content.Contains(currText))
                    {
                        currMsgs.Add(m);
                    }
                }

               messageList.ItemsSource = currMsgs;
            }
            else
            {
                messageList.ItemsSource = messageBank;
            }
         }
        void onTextChanged(object sender, EventArgs e) {
            Entry contactEntry = (Entry)sender;
            string currText = contactEntry.Text.ToString().ToUpper();
            if(!string.IsNullOrEmpty(currText)){
                List<String> Suggestions = new List<String>();
                foreach (Contact c in contactBank)
                {
                    string s = c.toString();
                    string sc = s.ToUpper();
                    if (sc.Contains(currText))
                    {
                        Suggestions.Add(s);
                    }
                }

                contactSuggestions.ItemsSource = Suggestions;
            }

        }
        private string strip(string c) {
            string[] a = c.Split('(');
            c = a[1].TrimEnd(')');
           
            return c;
        }
        
    }

    }
