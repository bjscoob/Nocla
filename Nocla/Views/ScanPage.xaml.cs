using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Nocla.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanPage : ContentPage
    {
        //SCAN PAGE: HANDLES ZEBRA CROSSING SCANNER DATA
        string Title;
        public string pm_num;
        public bool noMoreScan = false;
        private Stopwatch stopwatch = new Stopwatch();
        private long incoArraySize;
        private long time;
        private long outgoArraySize;
        int retryCount = 0;
        public string RMType { get; set; }

        //NEEDS RAW MATERIAL TYPE AND PM NUMBER TO BUIULD PAGE
        public ScanPage(string RMType, string pmNum)
        {
            noMoreScan = false;
            Title = "Scan New "+RMType;
            this.RMType = RMType;
            pm_num = pmNum;
            InitializeComponent();
            titleBar.Title = Title;
        }


        // triggers when scanner reads a qr code **NOTE**:  ADD INVALID PARSING AND REOPEN SCANNER ON FAILURE
        public void scanView_OnScanResult(ZXing.Result result)
        {
            //check if scanner hasnt already read something
            if (!noMoreScan)
            {
                //CLOSE SCANNER
                noMoreScan = true;

                //PARSE QR DATA AND SEND TO BACKEND
                string[] newRMData = result.Text.Split('|');

                //check if label data fits
                if (newRMData.Length < 3)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await App.Current.MainPage.DisplayAlert("Scanned result", "Invalid Label", "OK");

                        //open scanner
                        noMoreScan = false;
                    });
                }
                else
                {
                    sendData(newRMData);
                }
                
            }
        }

        //QR DATA IS MODIFIED INTO DICTIONARY AND SENT TO BACKEND USING HTTPCLIENT
        //QR DATA format batchno|bagno|expdate
        public async void sendData(string[] data)
        {
            
            var url = "https://jax-apps.com/api.php";
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("rm", pm_num),
                new KeyValuePair<string, string>("material",RMType ),
                new KeyValuePair<string, string>("batchno", data[0]),
                new KeyValuePair<string, string>("bagno", data[1]),
                new KeyValuePair<string, string>("expdate", data[2])
            });
            var myHttpClient = MsgPage.client;

            // get outgoing packet size
            outgoArraySize = (await formContent.ReadAsByteArrayAsync()).Length;

            //start stopwatch
            stopwatch.Restart();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await myHttpClient.PostAsync(url, formContent);
            }
            catch (Exception e)
            {
                retryCount++;
                if (retryCount < 11)
                {
                    sendData(data);
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
         
            var content = await response.Content.ReadAsStringAsync();

            //post changes to activity log
            if (content == "Success!")
            {
                string container = "Bag";
                if (RMType == "FT32") { container = "Tank"; }
                string msg = System.String.Format("Changed {0}: {1} {2}#{3} ", RMType, data[0], container, data[1]);
                PMViewPage.postACT(DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"), msg, true);
            }
            else { 
            }
            Device.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.DisplayAlert("Scanned result", content +Environment.NewLine+ diag, "OK");
            });
        }
    
    }
}