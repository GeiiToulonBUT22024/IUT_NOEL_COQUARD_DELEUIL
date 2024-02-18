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
            SciChartSurface.SetRuntimeLicenseKey("gb0C0XMSVKQTRujYiqtw5Y0g/YGRqHQJlcb3iOTmc/BE4o2XP/4W0e+v0ndSbqdzMNTSLOrrN5Kc9JQFGDrsi7H8Od5bnhruGuyKlDqZN/GcqAlFIEZnEcVAhHUtQHy/VwXBIMw/xVoqUnXKq8CUQhcPJq3cuu0qiHH8uzROZdJv2kU97Psf+pd0qsfFxv7T7yjOdknQu6uTJv0RbqfXyOpkZsClLBTDiNgiHQi633FVL77qpksREF9tCZq6ETlk5SmEdi2dH4gnzUfhzP6A5qVPE2mNWJj6ZiXSU6Ojaew2+IzDkmVhXXD60PgcxdR+5JMLomv6tB32B8hXNY+d8WyhjkNQ7bppaOpicppwPduyqKvZzbcqB/QUv93TjVAt0DmmXOKdNZ76j6hB4zZTGzh6nHH1y5ETAlaQwv8xwB/Bilu0y+0HX/fNgI/4gUGoyF9fZ2Ex192+EPu15t+uQjdlIWm+r5hADuUvzgX8nmnOuyY3nQHDUyIFZemFSTH4ValkYjWG");
        }
    }
}
