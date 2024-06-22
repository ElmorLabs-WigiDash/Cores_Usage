using WigiDashWidgetFramework;
using WigiDashWidgetFramework.WidgetUtility;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Task_MgrWidget
{
    public partial class Task_MgrWidgetServer : IWidgetObject
    {

        // Identity
        public Guid Guid => new Guid(GetType().Assembly.GetName().Name);
        public string Name => Task_MgrWidget.Properties.Resources.Task_MgrWidgetServer_Name;

        public string Description => Task_MgrWidget.Properties.Resources.Task_MgrWidget_CheckCores;

        public string Author => "Peter";

        public string Website => "https://www.gskill.com/product/412/415/1702982997/WigiDash";

        public Version Version => new Version(1, 0, 2);

        // Capabilities
        public SdkVersion TargetSdk => WidgetUtility.CurrentSdkVersion;

        public List<WidgetSize> SupportedSizes =>
            new List<WidgetSize>() {
                new WidgetSize(5, 4),
            };

        public Bitmap PreviewImage => new Bitmap(ResourcePath + "preview_5x4.png");

        // Functionality
        public IWidgetManager WidgetManager { get; set; }

        // Error handling
        public string LastErrorMessage { get; set; }


    }

}
