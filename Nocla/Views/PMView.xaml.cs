using Android.Webkit;
using Java.Lang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nocla.Models;
using Nocla.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Xaml;

namespace Nocla.Views
{
    //PMVIEW PAGE: HANDLES ACTIVITY LOG LISTVIEW, SPC PULL CHECKBOXES, AND RAW MATERIAL SCAN BUTTONS/DATA
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PMViewPage : ContentPage
    {
        //ACTIVIES LISTVIEW USES THIS LIST
        public static IList<Activity> Activities;
        public List<SPCTime> SPCTimes;
        public List<Label> spcSlots = new List<Label>();
        public List<CheckBox> checkBoxes = new List<CheckBox>();
        public List<MaterialAssignment> rmAssigns;
        public static ListView ActivitiesListView;
        public bool initMode = true;
        public bool inChkRecoveryState = false;
        public int currentSPCOpen = 0;

        public string ft32_batch;
        public static string pm_number;
        public static Color c;
        public string name;
        public string currentSPC_slot;
        private int oldTxtLn;
        private static long outgoArraySize;
        private static Stopwatch stopwatch = new Stopwatch() ;
        private static long incoArraySize;
        private static long time;
        static int retryCount = 0;

        //PM NUMBER AND COLOR NEEDED FROM PM SELECT TO BUILD THIS PAGE
        public PMViewPage(string pm_name, Color color)
        {
            c = color;
            name = pm_name;
            pm_number = pm_name.Substring(2);
            InitializeComponent();


            //REMOVE TITLE AND NAV BAR
            NavigationPage.SetHasNavigationBar(this, false);
            titleBar.BackgroundColor = color;
            add_act_btn.BackgroundColor = color;
            pmTitle.Text = pm_name;
            ActivitiesListView = ActivitiesList;
        }

        //ONAPPEARING CALLS FROM PM SELECT OR ON SCAN PAGE'S BACK BUTTON
        protected override void OnAppearing()
        {
            //REMOVE OLD BROWSER DATA
            var cookieManager = CookieManager.Instance;
            cookieManager.RemoveAllCookie();
            base.OnAppearing();
            getRM();
            generateList(c);
            Init_spc(c);
            Init_RM(c);

        }

        //OPEN SCAN PAGE FOR FT32 
        async void scanFT32(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ScanPage("FT32", pm_number));
        }

        //OPEN SCAN PAGE FOR PHOSPHATE
        async void scanPhosp(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ScanPage("Phosphate", pm_number));
        }

        //OPEN SCAN PAGE FOR SALINE
        async void scanSaline(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ScanPage("Saline-14", pm_number));
        }

        //COLOR SPC BUTTONS 
        private void Init_RM(Color color)
        {
            FT32_btn.BackgroundColor = color;
            Phosphate_btn.BackgroundColor = color;
            Saline_btn.BackgroundColor = color;
        }

        //COLOR SPC CHECKBOXES ***NOTE***: ALSO ADD CODE TO UPDATE TIMES/CHECK STATUS FROM BACKEND
        private async void Init_spc(Color color)
        {
            var cookieManager = CookieManager.Instance;
            cookieManager.RemoveAllCookie();
            SPCTimes = new List<SPCTime>();
            var uri = new Uri("https://jax-apps.com/api.php");
            HttpClient myClient = MsgPage.client;
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("spcGET", "blah"),
                new KeyValuePair<string, string>("pm", pm_number)
            });

            var myHttpClient = new HttpClient();
            // get outgoing packet size
            outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

            //start stopwatch
            stopwatch.Restart();
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                result = await myHttpClient.PostAsync(uri, formContent);
            }
            catch (System.Exception ex)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    Init_spc(color);
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Connection Error", ex.GetType().ToString(), "OK");
                    });
                    return;
                }
            }

            var content = await result.Content.ReadAsStringAsync();
            //get elapsed time and display diagnostic data
            stopwatch.Stop();
            retryCount = 0;
            if (result.Content == null)
            {
                Device.BeginInvokeOnMainThread(async () => {
                    await DisplayAlert("Server Error", "Unable to reach server", "OK");
                });
                return;
            }
            //get incoming packet size
            incoArraySize = (await result.Content.ReadAsByteArrayAsync()).Length;

            time = stopwatch.ElapsedMilliseconds;
            string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
            await DisplayAlert("Get SPC", diag, "OK");

            JArray a = JArray.Parse(content);
            foreach (JObject O in a.Children<JObject>())
            {
                List<string> spcdata = new List<string>();
                foreach (JProperty P in O.Properties())
                {
                    if ((string)P.Value == null)
                    {
                        spcdata.Add("--:--");
                    }
                    else
                    {
                        spcdata.Add((string)P.Value);
                    }
                }
                string timeStr = spcdata[2];
                if (timeStr != "--:--") {
                    timeStr = spcdata[2].Substring(12, 5);
                }
                bool isP = false;
                if (spcdata[3] == "1") {
                    isP = true;
                }
                SPCTimes.Add(new SPCTime
                {
                    pm_no = Int32.Parse(spcdata[0]),
                    spc_no = Int32.Parse(spcdata[1]),
                    time = timeStr,
                    isPulled = isP

                });


            }



            pullTime1.Text = SPCTimes[0].time; spcSlots.Add(pullTime1);
            pullTime2.Text = SPCTimes[1].time; spcSlots.Add(pullTime2);
            pullTime3.Text = SPCTimes[2].time; spcSlots.Add(pullTime3);
            pullTime4.Text = SPCTimes[3].time; spcSlots.Add(pullTime4);
            pullTime5.Text = SPCTimes[4].time; spcSlots.Add(pullTime5);
            pullTime6.Text = SPCTimes[5].time; spcSlots.Add(pullTime6);


            //Check Off Boxes if database says they are checked
            if (SPCTimes[0].isPulled == true) { spc1_check.IsChecked = true; currentSPCOpen++; } checkBoxes.Add(spc1_check);
            if (SPCTimes[1].isPulled == true) { spc2_check.IsChecked = true; currentSPCOpen++; } checkBoxes.Add(spc2_check);
            if (SPCTimes[2].isPulled == true) { spc3_check.IsChecked = true; currentSPCOpen++; } checkBoxes.Add(spc3_check);
            if (SPCTimes[3].isPulled == true) { spc4_check.IsChecked = true; currentSPCOpen++; } checkBoxes.Add(spc4_check);
            if (SPCTimes[4].isPulled == true) { spc5_check.IsChecked = true; currentSPCOpen++; } checkBoxes.Add(spc5_check);
            if (SPCTimes[5].isPulled == true) { spc6_check.IsChecked = true; currentSPCOpen++; } checkBoxes.Add(spc6_check);

            foreach (CheckBox c in checkBoxes) {
                c.Color = color;
            }
            initMode = false;
        }

        // Activities list data retrieved from php backend 
        private static  async void generateList(Color color)
        {
            Activities = new List<Activity>();
            var uri = new Uri("https://jax-apps.com/api.php");
            HttpClient myClient = MsgPage.client;
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("actGET", "blah"),
                new KeyValuePair<string, string>("pm", pm_number)
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
            catch (System.Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    generateList(color);
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
            await App.Current.MainPage.DisplayAlert("Generate Act List", diag, "OK");




            var content = await result.Content.ReadAsStringAsync();
            JArray a = JArray.Parse(content);
            foreach (JObject O in a.Children<JObject>())
            {
                List<string> actdata = new List<string>();
                foreach (JProperty P in O.Properties())
                {

                    actdata.Add((string)P.Value);

                }
                string dt = DateTime.Parse(actdata[3]).ToString("dd MMM yyyy");
                string t = DateTime.Parse(actdata[3]).ToString("HH:mm");
                Activities.Add(new Activity
                {
                    activity_id = Int32.Parse(actdata[0]),
                    pm_no = Int32.Parse(actdata[1]),
                    content = actdata[2],
                    activity_time = actdata[3],
                    author = actdata[4],
                    Date = dt,
                    Time = t,
                    color_text = color.ToHex()
                });


                
            }
            ActivitiesListView.ItemsSource = Activities;
        }

        //BACK BUTTON EVENT HANDLER
        async void goToPMSelect(object sender, EventArgs e)
        {
            await Navigation.PopAsync();

        }
        async void showSPCBox(object sender, EventArgs e) {
            Button b = (Button)sender;
            spcHeader.Text = b.Text;
            currentSPC_slot = b.Text.Substring(b.Text.Length - 1);
            int slot = Int32.Parse(currentSPC_slot)-1;

            if (checkBoxes[slot].IsChecked)
            {
                await DisplayAlert("SPC", "Cannot change sample. It has already been pulled.", "OK");
                return;
            }
            if (slot != 0 && spcSlots[slot-1].Text.ToString() == "--:--")
            {
                await DisplayAlert("SPC", "The previous pull needs a time.", "OK");
                return;
            }


            popupBG.IsVisible = true;
            spcBox.IsVisible = true;
            spcEntry.Text = "";
            
            spcEntry.TextChanged += OnTextChanged;


        }
        async void openActivityPopup(object sender, EventArgs e)
        {

            popupBG.IsVisible = true;
            actBox.IsVisible = true;
            actContent.Text = "";
            Button b = (Button)sender;
            actTimeEntry.TextChanged += OnTextChanged;


        }



        async void checkOffSPC(object sender, EventArgs e)
        {

            if (!initMode && !inChkRecoveryState)
            {
                CheckBox c = (CheckBox)sender;
                int pos = findCHK(c);
                //checks if SPC slot is next in line and is not being unchecked
                //if not next in line, turn it back off
                if (c.IsChecked && pos != currentSPCOpen)
                {
                    inChkRecoveryState = true;
                    c.IsChecked = false;

                    await DisplayAlert("Invalid Check", "You can only pull the very next sample.", "OK");

                }
                //if being turned off and is not the current slots predecessor, turn back on
                else if (!c.IsChecked && pos != (currentSPCOpen - 1))
                {
                    inChkRecoveryState = true;
                    c.IsChecked = true;

                    await DisplayAlert("Invalid Check", "You can only undo the very last sample.", "OK");

                }
                //else continue with operation
                else
                {
                    if (spcSlots[pos].Text.ToString() == "--:--")
                    {
                        inChkRecoveryState = true;
                        c.IsChecked = !c.IsChecked;
                        await DisplayAlert("SPC", "This Sample# has no time!", "OK");
                    }
                    else
                    {
                        string isChecked = "1";
                        string message = "Pulling Sample#" + (pos + 1) + "?";
                        string action = "Pulled";
                        if (c.IsChecked == false) { message = "Undo Sample#" + (pos + 1) + "?"; action = "Undid"; }
                        var answer = await DisplayAlert("SPC", message, "Yes", "No");

                        if (answer)
                        {
                            if (c.IsChecked == false)
                            {
                                isChecked = "0";
                                currentSPCOpen--;
                            }
                            else
                            {
                                currentSPCOpen++;
                            }
                            var url = "https://jax-apps.com/api.php";
                            var formContent = new FormUrlEncodedContent(new[]
                            {
                new KeyValuePair<string, string>("spcCHK", "yes"),
                new KeyValuePair<string, string>("pm", pm_number),
                new KeyValuePair<string, string>("slot", (pos+1).ToString()),
                new KeyValuePair<string, string>("isChecked", isChecked)

            });
                            var myHttpClient = new HttpClient();

                            // get outgoing packet size
                            outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

                            //start stopwatch
                            stopwatch.Restart();

                            HttpResponseMessage response = new HttpResponseMessage();
                            try
                            {
                                response = await myHttpClient.PostAsync(url, formContent);
                            }
                            catch (System.Exception ex)
                            {
                                retryCount++;
                                if (retryCount < 11)
                                {
                                    checkOffSPC(sender, e);
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
                            retryCount = 0;
                            if (response.Content == null)
                            {
                                Device.BeginInvokeOnMainThread(async () => {
                                    await DisplayAlert("Server Error", "Unable to reach server", "OK");
                                });
                                return;
                            }
                            //get incoming packet size
                            incoArraySize = (await response.Content.ReadAsByteArrayAsync()).Length;
                            time = stopwatch.ElapsedMilliseconds;
                            string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
                            await DisplayAlert("Check Off SPC", diag, "OK");
                            //post to recent activities
                            string msg = System.String.Format("{0} SPC#{1} ", action, (pos + 1).ToString());
                            postACT(DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"),DateTime.Now.ToString("ss"), msg, false);
                        }
                        else
                        {
                            inChkRecoveryState = true;
                            c.IsChecked = !c.IsChecked;
                            await DisplayAlert("SPC", "Changes Reverted.", "OK");
                        }
                    }
                }
                inChkRecoveryState = false;

            }
        }

        private int findCHK(CheckBox c)
        {
            int i = -1;
            foreach (CheckBox ch in checkBoxes) {
                i++;
                if (ch == c) { return i; }
            }
            return -1;

        }

        void OnTextChanged(object sender, EventArgs e)
        {
            Entry entry = sender as Entry;
            string val = entry.Text; //Get Current Text
            if (val.Length > 5 || val.Contains(".") || val.Contains("-"))//If it is more than your character restriction or contains invalid strings
            {
                val = val.Remove(val.Length - 1);// Remove Last character 
                entry.Text = val; //Set the Old value
            }
            
            if ((val.Length == 2) && (oldTxtLn == 0 || oldTxtLn == 1))//If first two numbers have been input
            {
                entry.Text = val + ":";
                
            }
            oldTxtLn = val.Length;
        }
        async void exitSPC(object sender, EventArgs e)
        {
            popupBG.IsVisible = false;
            spcBox.IsVisible = false;
        }
        async void sendSPC(object sender, EventArgs e)
        {
            //assume valid input. input is invalid if missing or not a real time
            bool invalid = false;
            string t = spcEntry.Text.ToString();
            int hour = 60;
            int min = 60;
            string hourStr = t.Substring(0, 2);
            string minStr = t.Substring(t.Length - 2);
            //try to fetch input, if error flag as invalid
            try
            {
                hour = Int32.Parse(hourStr);
                min = Int32.Parse(minStr);
            }
            catch {
                invalid = true;
            }

            //handle invalid input
            if (invalid || hour > 23 || min > 59)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Invalid Time", "Please enter a valid time.", "OK");
                });
            }
            //else update database
            else
            {
                var url = "https://jax-apps.com/api.php";
                var formContent = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("spcPOST", "yes"),
                new KeyValuePair<string, string>("hour", hour.ToString()),
                new KeyValuePair<string, string>("min", min.ToString()),
                new KeyValuePair<string, string>("pm", pm_number),
                new KeyValuePair<string, string>("slot", currentSPC_slot )
            });
                var myHttpClient = new HttpClient();
                // get outgoing packet size
                outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

                //start stopwatch
                stopwatch.Restart();
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await myHttpClient.PostAsync(url, formContent);
                }
                catch (System.Exception ex)
                {
                    retryCount++;
                    if (retryCount < 11)
                    {
                        sendSPC(sender, e);
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
                retryCount = 0;
                if (response.Content == null)
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Server Error", "Unable to reach server", "OK");
                    });
                    return;
                }
                //get incoming packet size
                incoArraySize = (await response.Content.ReadAsByteArrayAsync()).Length;
                time = stopwatch.ElapsedMilliseconds;
                string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
                await DisplayAlert("Send SPC", diag, "OK");

                //change timeslot
                spcSlots[Int32.Parse(currentSPC_slot) - 1].Text = spcEntry.Text.ToString();
                //post to recent activities
                string msg = System.String.Format("Updated SPC#{0} to {1}:{2}",currentSPC_slot, hourStr,minStr);
                postACT(DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"), msg, false);
                //close the popup
                popupBG.IsVisible = false;
                spcBox.IsVisible = false;
            }


            //refresh screen
            /*Navigation.InsertPageBefore(new PMViewPage(name, c), this);
            await Navigation.PopAsync();*/


        }
        async void clearSPC(object sender, EventArgs e)
        {
            int slot = Int32.Parse(currentSPC_slot) ;
            if (spcSlots[slot].Text.ToString() != "--:--") {
                DisplayAlert("SPC", "Cannot clear. The next pull has a time.", "OK");
                return;
            }
            var answer = await DisplayAlert("SPC", "Clear this timeslot?", "Yes", "No");
            if (answer)
            {
                
                // update database
                
                    var url = "https://jax-apps.com/api.php";
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string, string>("spcCLR", "yes"),
                new KeyValuePair<string, string>("pm", pm_number),
                new KeyValuePair<string, string>("slot", currentSPC_slot )
            });
                    var myHttpClient = new HttpClient();

                // get outgoing packet size
                outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

                //start stopwatch
                stopwatch.Restart();
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await myHttpClient.PostAsync(url, formContent);
                }
                catch (System.Exception ex)
                {
                    retryCount++;
                    if (retryCount < 11)
                    {
                        clearSPC(sender, e);
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async () => {
                            await DisplayAlert("Connection Error", ex.GetType().ToString(), "OK");
                        });
                    }
                }

                //get elapsed time and display diagnostic data
                stopwatch.Stop();
                retryCount = 0;
                if (response.Content == null)
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Server Error", "Unable to reach server", "OK");
                    });
                    return;
                }
                //get incoming packet size
                incoArraySize = (await response.Content.ReadAsByteArrayAsync()).Length;
                time = stopwatch.ElapsedMilliseconds;
                string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
                await DisplayAlert("Clear SPC", diag, "OK");
                //change timeslot
                spcSlots[Int32.Parse(currentSPC_slot) - 1].Text = "--:--";

                    //close the popup
                    popupBG.IsVisible = false;
                    spcBox.IsVisible = false;
                
            }

        }





        async void exitACT(object sender, EventArgs e) {
            popupBG.IsVisible = false;
            actBox.IsVisible = false;
        }
        async void sendACT(object sender, EventArgs e) {
            //assume valid input. input is invalid if missing or not a real time
            bool invalid = false;
            string time = "--:--";
            int hour = 60;
            int min = 60;

            //try to fetch input, if error flag as invalid
            try
            {
                time = actTimeEntry.Text.ToString();
                hour = Int32.Parse(time.Substring(0, 2));
                min = Int32.Parse(time.Substring(time.Length - 2));
            }
            catch
            {
                invalid = true;
            }
            if (string.IsNullOrEmpty(actContent.Text.ToString()))
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Invalid Entry", "Please enter an entry", "OK");
                });
                return;
            }
            //handle invalid input
            else if (invalid || hour > 23 || min > 59)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Invalid Time", "Please enter a valid time.", "OK");
                });
                return;
            }

            //else update database
            else
            {
                postACT(hour.ToString(), min.ToString(),"00",  actContent.Text.ToString(),false);
                //close the popup
                popupBG.IsVisible = false;
                actBox.IsVisible = false;
            }
        }
        public static async void postACT(string h, string m,string s,  string content, bool fromScan) {
                var url = "https://jax-apps.com/api.php";
                var formContent = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("actPOST", "yes"),
                new KeyValuePair<string, string>("hour", h),
                new KeyValuePair<string, string>("min", m),
                new KeyValuePair<string, string>("sec", s),
                new KeyValuePair<string, string>("pm_no", pm_number),
                new KeyValuePair<string, string>("content", content),
                new KeyValuePair<string, string>("author", LoginViewModel.user.getUsername().ToUpper() )
            });
                var myHttpClient = new HttpClient();
            // get outgoing packet size
            outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

            //start stopwatch
            stopwatch.Restart();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await myHttpClient.PostAsync(url, formContent);
            }
            catch (System.Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    postACT(h, m, s, content, fromScan);
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
            retryCount = 0;
            if (response.Content == null)
            {
                Device.BeginInvokeOnMainThread(async () => {
                    await App.Current.MainPage.DisplayAlert("Server Error", "Unable to reach server", "OK");
                });
                return;
            }
            //get incoming packet size
            incoArraySize = (await response.Content.ReadAsByteArrayAsync()).Length;
            time = stopwatch.ElapsedMilliseconds;
            string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
            Device.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.DisplayAlert("Post Act", diag, "OK");
            });



            if (!fromScan) { generateList(c); }
            
        }
        async void clearACT(object sender, EventArgs e) {
            actTimeEntry.Text = "";
            actContent.Text = "";
        }
        async void openRecAct(object sender, ItemTappedEventArgs e) {
            Activities.ElementAt(e.ItemIndex).BackgroundColor = Color.Gray;
            ActivitiesList.ItemsSource = Activities;
            //var selectedItem = e.Item;
            //selectedItem.GetType();
        }
        
        public async void getRM() {
            rmAssigns = new List<MaterialAssignment>();
            var uri = new Uri("https://jax-apps.com/api.php");
            HttpClient myClient = MsgPage.client;
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("rmGET", "blah"),
                new KeyValuePair<string, string>("pm", pm_number)
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
            catch (System.Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    getRM();
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
            retryCount = 0;
            if (result.Content == null)
            {
                Device.BeginInvokeOnMainThread(async () => {
                    await DisplayAlert("Server Error", "Unable to reach server", "OK");
                });
                return;
            }
            //get incoming packet size
            incoArraySize = (await result.Content.ReadAsByteArrayAsync()).Length;
            time = stopwatch.ElapsedMilliseconds;
            string diag = string.Format("Outgoing Packet Length:{1}{0}Incoming Packet Length:{2}{0}Time (ms):{3}", Environment.NewLine, outgoArraySize, incoArraySize, time);
            await DisplayAlert("Get RM", diag, "OK");
            var content = await result.Content.ReadAsStringAsync();
            if (content == null || content.Length == 0) { return; } 
            JArray a = JArray.Parse(content);
            foreach (JObject O in a.Children<JObject>())
            {
                if (O == null) { continue; }
                List<string> rmdata = new List<string>();
                foreach (JProperty P in O.Properties())
                {

                    rmdata.Add((string)P.Value);

                }
                rmAssigns.Add(new MaterialAssignment
                {
                    pm = rmdata[0],
                    material = rmdata[1],
                    batchno = rmdata[2],
                    bagno = rmdata[3],
                    assigndate = rmdata[4],
                    assigntime = rmdata[5],
                    expdate = rmdata[6],
                    exptime = rmdata[7],
                    iscurrent = rmdata[8]

                });



            }
            foreach (MaterialAssignment m in rmAssigns) {
                switch (m.material) {
                    case "FT32": getFT32(m);break;
                    case "Phosphate": getPhosp(m);break;
                    case "Saline-14": getSaline(m);break;
                    default:
                        DisplayAlert("RM Retrieval Error", m.toString(), "OK");break;
                }
            }
        }

        //GETS FT32 DATA FROM BACKEND ***NOTE**: CHANGE TO POST ASYNC
        public async Task getFT32(MaterialAssignment ma)
        {   
                
                
                FT32_Batch.Text = ma.batchno;
                FT32_Bag.Text = ma.bagno;
                FT32_ExpDate.Text = ma.expdate;
                FT32_ExpTime.Text = ma.exptime;
               
            
        }

        //GETS PHOSP DATA FROM BACKEND ***NOTE**: CHANGE TO POST ASYNC
        public async Task getPhosp(MaterialAssignment ma)
        {
            
                Phosp_Batch.Text = ma.batchno;
                Phosp_Bag.Text = ma.bagno;
                Phosp_ExpDate.Text = ma.expdate;
                Phosp_ExpTime.Text = ma.exptime;
            }
        

        //GETS SALINE DATA FROM BACKEND ***NOTE**: CHANGE TO POST ASYNC
        public async Task getSaline(MaterialAssignment ma)
        {
                Saline_Batch.Text = ma.batchno;
                Saline_Bag.Text = ma.bagno;
                Saline_ExpDate.Text = ma.expdate;
                Saline_ExpTime.Text = ma.exptime;
            
        }
    }
}