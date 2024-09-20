using SciChart.Charting.Visuals;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace robotInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NAaF5cWWRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWX1feHVdQ2NeVU1xX0c=");
            SciChartSurface.SetRuntimeLicenseKey("Zu2GA4rFuCy+0deMTLob1NlEOdNxvp6jRKD04CHZAPnAwRR2lHlvRDZFZK3MMIvMSIk3IsEBkVydA4C3e7L7mvSQpKTpZ3CS2jKvhnOsGsIAeWqEL3MVcO3rDW/Ehzt3aQ6XE5RAeX5dc3ZUZ8wh+rDatAgzr+608ckpA7kt/LdT0uyJQinVEmM6T5z8F0Dku8xctozpl/RJRv3gjkvM5oyHOgCuUFuViPFRAimEpLjxFHbI7ogjrLoo3lTPuIE6OhKzuArym0WxkBLxOYyqA8+CB/zeYTxBKZAenM0AberPbHmB8rvSPEi9jY4Mtq3EMCb95B4q/2xWmRBCLYPVdPB6SnLtpuL0XqCJ1Wy4Y9mTCKDHMKZoldCG4l/vG4/DhQ8CPSzFLYPjuP6MW3MTNa8wgplqt+IKjBgRlMPh7wCeZa8Mn1/0LCAJVScFMfco2gVNaHgSGTu/zQk7RH1WozD1lGDOVI3h1/a+355EF4/JMNxeHfGPvqVhndIdbGK/bIy8Q1gT");
        }
    }
}
