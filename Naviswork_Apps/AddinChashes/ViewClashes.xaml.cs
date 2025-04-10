using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Syncfusion.Licensing;

namespace Naviswork_Apps
{
    /// <summary>
    /// Lógica de interacción para ViewMain.xaml
    /// </summary>
    public partial class ViewClashes : Window
    {
        public ViewClashes()
        {
            SyncfusionLicenseProvider
                .RegisterLicense("MzgxMDAwOUAzMjM5MmUzMDJlMzAzYjMyMzkzYmpuRXNpM0NwYmd0UHZPdHpDTGdJUmFuV1dBZkxveTJrVDJONFBTQlFXNVU9");
            InitializeComponent();
        }        
    }
}
