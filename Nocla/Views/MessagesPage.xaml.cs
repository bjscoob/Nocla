using Newtonsoft.Json.Linq;
using Nocla.Models;
using Nocla.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static List<string> Recipients ; 
        public MsgPage()
        {
            //MAKE NEW HTTPCLIENT TO TALK TO PHP BACKEND
            client = new HttpClient();
            client.CancelPendingRequests();
            //this is the first page to show on launch, the login page is forced on top if login check is not valid
            if (!LoginViewModel.CheckStatus()) goToLogin();
            InitializeComponent();
            nameField = Fullname;

        }

        //CHANGES USER FIRST AND LAST NAME AT TOP OF SCREEN
        public static void updateName(string name) {
            nameField.Text = "Hello, " + LoginViewModel.user.getFLName();
            initContactBank();
        }

        private async static void initContactBank()
        {
            contactBank = new List<Contact>();
            var uri = new Uri("http://jax-apps.com/msgapi.php");
            HttpClient myClient = MsgPage.client;
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("getUsers", "blah")
            });
            var result = await client.PostAsync(uri, formContent);
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
        }

        //closes popup message screen
        async void exitNewMsg(object sender, EventArgs e)
        {
            popupMessageCreate.IsVisible = false;
        }
        async void addToContacts(object sender, EventArgs e) {
            string s = contactSuggestions.SelectedItem.ToString();
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
            }

            
        }
        async void bgTransform(object sender, ItemTappedEventArgs e) {
            var selectedItem = (View)e.Item;
            selectedItem.BackgroundColor = Color.Aqua;
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
            await myClient.PostAsync("http://jax-apps.com/notapi.php", stringContent);
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
