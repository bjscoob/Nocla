using Nocla.ViewModels;
using System;
using System.ComponentModel;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Nocla.Views
{
    public partial class PMSelectPage : ContentPage
    {
        //PM SELECT PAGE: MANAGES THE 15 PM BUTTONS ON THE SELECTION PAGE
        public PMSelectPage()
        {
          
            InitializeComponent();


       
    }
        //when button is pressed, opens PMView page with the pm number and color
        async void goToPMVIEW(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string pm_name = button.Text;
            Color color = button.BackgroundColor;
            await Navigation.PushAsync(new PMViewPage(pm_name, color));
            
        }

    }
}
