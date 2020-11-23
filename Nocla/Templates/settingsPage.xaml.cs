using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nocla.Views;
using Nocla.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Net.Http.Headers;
using SkiaSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace Nocla.Templates
{

    // Photo Upload code copied from http://gjhdigital.com/xamarin/xamarin-forms-upload-image-to-php/
    [XamlCompilation(XamlCompilationOptions.Compile)]
 
    public partial class settingsPage : Grid
    {
        public static string fileName;
        public static Label username;
            public static Label id;
            public static Label fullname;
            public static Label pos;
            public static Label sh;
            public static Label eml;
            public static Label mgr;
            public static Label assign;
        private int retryCount = 0;

        public settingsPage()
        {
            InitializeComponent();
            username = usernameLabel;
            id = IDLabel;
            fullname = flName;
            pos = position;
            sh = shift;
            eml = email;
            mgr = manager;
            assign = assignment;

        }
        async void changePhoto(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {

                    await App.Current.MainPage.DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                }

                    );
                return;
            }
            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,

            });

            if (file == null)
                return;

            fileName = file.Path;
            string fileExt = fileName.Substring(fileName.Length - 4);
            var inStream = file.GetStream();
            var outStream = new MemoryStream();
            var image = SixLabors.ImageSharp.Image.Load(inStream, out IImageFormat format);
       
                var clone = image.Clone(
                    i => i.Resize(100, 100));

                clone.Save(outStream, format);
            outStream.Position = 0;
            outStream.Seek(0, SeekOrigin.Begin);
            
                int authorID = LoginViewModel.user.employee_id;
                string username = LoginViewModel.user.username;
                var url = "https://jax-apps.com/imgapi.php";
                url += "?id=" + authorID + "&username=" + username; //any parameters you want to send to the php page.

                try
                {

                    
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://jax-apps.com/");
                    MultipartFormDataContent form = new MultipartFormDataContent();

                    StreamContent content = new StreamContent(outStream);

                    //make file's name
                    string fName = username + fileExt.ToLower();

                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "fileToUpload",
                        FileName = fName
                    };
                    form.Add(content);
                    var response = await client.PostAsync(url, form);
                    var result = response.Content.ReadAsStringAsync().Result;
                    Device.BeginInvokeOnMainThread(async () =>
                    {

                        await App.Current.MainPage.DisplayAlert("Image Upload", result, "OK");
                    });
                if (result.Contains("Success")) {
                    MsgPage.userImg.Source = new UriImageSource
                    {
                        Uri = new Uri("https://jax-apps.com/images/" + fName),
                        CachingEnabled = false
                    }; 
                }
                }
                catch (Exception ex)
                {
                    //debug
                    Device.BeginInvokeOnMainThread(async () =>
                    {

                        await App.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
                    });
                    return;
                }
            


        }
        public static async void UploadImage(Stream mfile, string fileExt)
        {
            int authorID = LoginViewModel.user.employee_id;
            string username = LoginViewModel.user.username;
            var url = "https://jax-apps.com/imgapi.php";
            url += "?id=" + authorID + "&username=" + username; //any parameters you want to send to the php page.

            try
            {

                mfile.Seek(0, SeekOrigin.Begin);
                HttpClient client = new HttpClient() ;
                client.BaseAddress = new Uri("https://jax-apps.com/");
                MultipartFormDataContent form = new MultipartFormDataContent();

                var stream = mfile;
                StreamContent content = new StreamContent(stream);

                //make file's name
                string fName = username + fileExt.ToLower();

                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "fileToUpload",
                    FileName = fName
                };
                form.Add(content);
                var response = await client.PostAsync(url, form);
                var result = response.Content.ReadAsStringAsync().Result;
                Device.BeginInvokeOnMainThread(async () =>
                {

                    await App.Current.MainPage.DisplayAlert("Error", result, "OK");
                });
            }
            catch (Exception e)
            {
                //debug
                Device.BeginInvokeOnMainThread(async () =>
                {

                    await App.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
                });
                return;
            }
        }
        public static void initUserData(string[] data) {
            username.Text = data[2].ToUpper();
            id.Text = "Employee Id: " + data[1];
            fullname.Text = data[4] + " " + data[5];
            pos.Text = data[6];
            sh.Text = data[7] + " Shift";
            eml.Text = data[3];
            assign.Text = "Assignment: PM " + data[9];
        }
        public static void initManager(string name) {
            mgr.Text = "Manager: "+ name;
        }
        async void resetPassword(object sender, EventArgs e)
        {
            bool answer =  await App.Current.MainPage.DisplayAlert("Reset Password", "Send an email to change your password?", "YES","NO");
            if (answer)
            {
                var uri = new Uri("https://jax-apps.com/rp.php?email=" + eml.Text.ToString());
                HttpClient myClient = MsgPage.client;

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
                        resetPassword(sender, e);
                        return;
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await App.Current.MainPage.DisplayAlert("Connection Error", ex.GetType().ToString(), "OK");
                        });

                        return;
                    }
                }
                retryCount = 0;
                if (response.Content == null)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await App.Current.MainPage.DisplayAlert("Server Error", "Unable to reach server", "OK");
                    });
                    return;
                }
                else
                {
                    string h = "Error";
                    string s = "Sorry.The email failed to send." + Environment.NewLine + " Try again or submit a ticket";
                    string r = await response.Content.ReadAsStringAsync();
                    if (r.Contains("Success"))
                    {
                        h = "Success!";
                        s = "An email has been sent to " + eml.Text.ToString() + Environment.NewLine + "Please check to reset your password.";
                    }
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await App.Current.MainPage.DisplayAlert(h, s, "OK");
                    });

                }
            }
        }
        async void showChangeName(object sender, EventArgs e)
        {
           popupCreate.IsVisible = true;
           firstLabel.Text = "First Name:";
           firstEntry.Text = LoginViewModel.user.firstname;
           secondLabel.Text = "Last Name:";
           secondEntry.Text = LoginViewModel.user.lastname;
            submitBtn.Clicked -= changeEmail;
            submitBtn.Clicked += changeName;
        }
        async void showChangeEmail(object sender, EventArgs e)
        {
            popupCreate.IsVisible = true;
            firstLabel.Text = "New Email:";
            firstEntry.Text ="";
            secondLabel.Text = "Confirm Email";
            secondEntry.Text ="";
            submitBtn.Clicked -= changeName;
            submitBtn.Clicked += changeEmail;
        }
        async void changeName(object sender, EventArgs e) {
            var url = "https://jax-apps.com/api.php";
            string f = firstEntry.Text.ToString();
            string l = secondEntry.Text.ToString();
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("nameChange", LoginViewModel.user.employee_id.ToString()),
                new KeyValuePair<string, string>("first", f),
                new KeyValuePair<string, string>("second", l)
            });


            //perform POST operation
            var myHttpClient = new HttpClient();

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await myHttpClient.PostAsync(url, formContent);



                if (response.Content == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error","Unable to reach server","OK");
                }
                var result = await response.Content.ReadAsStringAsync();
                if (result.Contains("Success"))
                {
                    await App.Current.MainPage.DisplayAlert("Success", "Name Changed.", "OK");
                    LoginViewModel.user.firstname = f;
                    LoginViewModel.user.lastname = l;
                    MsgPage.updateName("");
                    flName.Text = f + " " + l;
                    popupCreate.IsVisible = false;
                }
                else {
                    await App.Current.MainPage.DisplayAlert("Error", "Please Submit a Ticket", "OK");
                }

            }


            catch (Exception ex)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    changeName(sender,e);
                    return;
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");

                }
            }
            retryCount = 0;
        }
        async void changeEmail(object sender, EventArgs e)
        {

            string e1 = firstEntry.Text.ToString();
            string e2 = secondEntry.Text.ToString();
            if (string.IsNullOrEmpty(e1) || string.IsNullOrEmpty(e2)){ return; }
            if (!(e1.CompareTo(e2)==0)) {
                await App.Current.MainPage.DisplayAlert("Error", "Emails must match", "OK");
                return;
            }
            if (!IsValidEmail(e1)) {
                await App.Current.MainPage.DisplayAlert("Error", "Not a valid email."+Environment.NewLine+"(Tip: Remove any spaces. There may be a space at the end.)", "OK");
                return;
            }
            var url = "https://jax-apps.com/api.php";
            string em = firstEntry.Text.ToString();
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("emailChange", LoginViewModel.user.employee_id.ToString()),
                new KeyValuePair<string, string>("email", em ),
            });


            //perform POST operation
            var myHttpClient = new HttpClient();

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await myHttpClient.PostAsync(url, formContent);



                if (response.Content == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Unable to reach server", "OK");
                }
                var result = await response.Content.ReadAsStringAsync();
                if (result.Contains("Success"))
                {
                    await App.Current.MainPage.DisplayAlert("Success", "Email Changed.", "OK");
                    LoginViewModel.user.email = em;
                    email.Text = em;
                    popupCreate.IsVisible = false;
                }
                else if (result.Contains("Duplicate")) {
                    await App.Current.MainPage.DisplayAlert("Error", "That email is already taken", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Please Submit a Ticket", "OK");
                }

            }


            catch (Exception ex)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    changeName(sender, e);
                    return;
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");

                }
            }
            retryCount = 0;
        }
        async void exitPopup(object sender, EventArgs e) {
            popupCreate.IsVisible = false;
        }
        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
