using Autodesk.Navisworks.Api.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop.ComApiAutomation;
using Autodesk.Navisworks.Api.Clash;

using Autodesk.Navisworks.Api.DocumentParts;
using System.Diagnostics;
using Naviswork_Apps.Utils;

namespace Naviswork_Apps.AddinChashes
{
    public class Clashes : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            ViewClashes viewMain = new ViewClashes();
            IntPtr handle = Application.Gui.MainWindow.Handle;
            new WindowInteropHelper(viewMain).Owner = handle;
            viewMain.Show();

            //var interferences = ClashUtils.GetInterferences();

            return 0;

        }      

    }
}
