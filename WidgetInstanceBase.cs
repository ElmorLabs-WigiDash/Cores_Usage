using WigiDashWidgetFramework;
using WigiDashWidgetFramework.WidgetUtility;
using System;

namespace Task_MgrWidget
{

    public partial class Task_MgrWidgetInstance : IWidgetInstance
    {

        // Identity
        private Task_MgrWidgetServer parent;
        public IWidgetObject WidgetObject
        {
            get
            {
                return parent;
            }
        }
        public Guid Guid { get; set; }

        // Location
        public WidgetSize WidgetSize { get; set; }

        // Events
        public event WidgetUpdatedEventHandler WidgetUpdated;

    }
}
