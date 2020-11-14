using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                sendData(newRMData);
                
            }
        }

        //QR DATA IS MODIFIED INTO DICTIONARY AND SENT TO BACKEND USING HTTPCLIENT
        //QR DATA format batchno|bagno|expdate
        public async void sendData(string[] data)
        {
            
            var url = "http://jax-apps.com/api.php";
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("rm", pm_num),
                new KeyValuePair<string, string>("material",RMType ),
                new KeyValuePair<string, string>("batchno", data[0]),
                new KeyValuePair<string, string>("bagno", data[1]),
                new KeyValuePair<string, string>("expdate", data[2])
            });
            var myHttpClient = MsgPage.client;
            var response = await myHttpClient.PostAsync(url, formContent);
            var content = await response.Content.ReadAsStringAsync();
            //post to recent activities
            string container = "Bag";
            if (RMType == "FT32") { container = "Tank"; }
            string msg = System.String.Format("Changed {0}: {1} {2}#{3} ", RMType, data[0], container, data[1]);
            PMViewPage.postACT(DateTime.Now.ToString("HH"), DateTime.Now.ToString("mm"), DateTime.Now.ToString("ss"), msg);
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Scanned result", content, "OK");
            });
        }
    
    }
}