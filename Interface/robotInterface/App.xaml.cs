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
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("FrhwEfO1tQSRiIaUyQMfZrHFABbuO/Z52vpBXondJFlafiDTfSUmPD7CsYOCGPq/iOz3h4iHytcf7rBwGjS7lWlmXFAkTkXDQeXHkHDlxfz376WpMfWO903Uw3seHthEnLTC1Isn7WieQggwyp3/QNaEBUMrML1WApSKRUtuEsNo+4j7g6bcX8WzkOozsRQrCe7YDY5iA2KfQF89QDspTGp3Olr42uE0urFESiEEf2NYObOg0fhVsukVspIVEatc+B4Wp3AUve4oT6cabf4li8f+M94DuQLMA/KVAKB8ebzcsaZZwF0LjySIsF2tP9lVsxrmSIoCJH+7w0fQMaed+yNgFxe0F0xgaDRzqCeVvRO7DlyKyJzVruOv2hFTHtV8Zp60bO5xOjn2HixUWP2iLb5dJN0A4WNGRqpc1V5+r9O8ijEZX3pisPM6zzQHNJqk1aoBeH5niv2U0cNPaSLS5taTw3z+minBjk99jEOhvtT+PkfDbMPpj4EUH/R/CfxrnV7Gja9j");

            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NAaF5cWWRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWX1feHVdQ2NeVU1xX0c=");
             }
    }
}
