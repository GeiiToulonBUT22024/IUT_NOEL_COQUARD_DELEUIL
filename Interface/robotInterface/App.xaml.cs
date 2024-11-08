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
            SciChartSurface.SetRuntimeLicenseKey("hx1QRWH+tH8q1KVyOLKGdFbd6lVdHcx84Wcrn0gZTf+mkkzLXAXhuV7ZMbCdRe9MhxS7dCSYe6lTEsAni+OdWpr5u3p3DA3hIjZC2aWOa0CL1RaKkMbz/CoZQ/pV5GUFeGiMyaLkt6/57zVoCyjinBEXRLFeRW1XbWpEhoHL6C78JuSkhbedoplwHvrQakM1mz3MHwLn8s8XkVpya7R7qhlEJklVlnbiGBMNRxXw9L9WBHogdHKhPFyUyFYl97+kE8ki2zkjGSL2grf46ie9GKBcswPgsjteg7WSM7N+sFqbPsoZPxvOxKyn/6bJ4ciCFgq7gUotpC6BndJAeHmyzaISLNgCQpybkNoyW3O8Wf8CJlgAd28lXpJXBfVfPAqGvKsHzjNXjoGx1wqqUgoAfPf9pBnAn8vYjW32+W4sEqSCzMDVHtcd23+7V9INFkPF3xc1nNgmuuxr05zIMOROVZq0gPK3RS9uKrW8HPs80EQ9ZnVbdYhfIpduqXJZoUTxy3qkUC2n");
        }
    }
}
