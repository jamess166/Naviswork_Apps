using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Navisworks.Api.Interop.ComApiAutomation;
using Autodesk.Navisworks.Api.DocumentParts;
using System.Xml.Linq;
using Autodesk.Navisworks.Api.Automation;
using Autodesk.Navisworks.Api.Data;
using Autodesk.Windows;
using Autodesk.Navisworks.Api.ComApi;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Naviswork_Apps.Utils;



namespace Naviswork_Apps
{
    //[PluginAttribute("FocusIds",
    //    "ADSK",
    //    ToolTip = "Cesel",
    //    DisplayName = "Focus Ids")]
    //[RibbonLayout("Naviswork_Apps.xaml")]
    //[RibbonTab("ID_CESEL_TAB", DisplayName = "CESEL")]
    //[Command("ID_ButtonFocusIds", DisplayName = "Ejecutar Focus", Icon = "Resources/cesel16x16.png", LargeIcon = "Resources/cesel32x32.png", ToolTip = "Ejecutar comando Focus")]
    public class FocusIds : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            ViewFocusId viewMain = new ViewFocusId();
            IntPtr handle = Autodesk.Navisworks.Api.Application.Gui.MainWindow.Handle;
            new WindowInteropHelper(viewMain).Owner = handle;
            viewMain.Show();

            return 0;

        }

        public static int HighlightElements(List<string> idsList, int index = 0)
        {
            // Obtener el documento activo
            Autodesk.Navisworks.Api.Document doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
            if (doc == null)
            {
                //System.Windows.MessageBox.Show("No hay un documento abierto en Navisworks.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }

            // Crear la búsqueda
            Search search = new Search();

            int idInt;
            int.TryParse(idsList[index], out idInt);
            SearchCondition idCondition = SearchCondition.HasPropertyByDisplayName("Element", "Id")
                                          .EqualValue(VariantData.FromInt32(idInt));

            search.SearchConditions.Add(idCondition);

            //search.SearchConditions.AddGroup(conditions);
            search.Selection.SelectAll();
            search.Locations = SearchLocations.DescendantsAndSelf;

            // Ejecutar la búsqueda
            ModelItemCollection foundItems = search.FindAll(doc, true);
            if (foundItems == null || foundItems.Count == 0)
            {
                //System.Windows.MessageBox.Show("No se encontraron elementos con los IDs especificados.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return 0;
            }

            // Destacar elementos
            doc.CurrentSelection.Clear();
            doc.CurrentSelection.CopyFrom(foundItems);

            return 1;
        }

        public static void ActivateSection()
        {
            Autodesk.Navisworks.Api.Document doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
            if (doc == null)
            {
                System.Windows.MessageBox.Show("No hay un documento abierto en Navisworks.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Obtener los elementos seleccionados
            ModelItemCollection selectedItems = doc.CurrentSelection.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }

            // Obtener el elemento seleccionado
            ModelItem selectedItem = doc.CurrentSelection.SelectedItems[0];

            // Obtener la caja de límites del elemento seleccionado
            BoundingBox3D bounds = selectedItem.BoundingBox();

            // Crear la sección en el plano XY
            NavisworkUtils.CreateXYSectionPlane(bounds);         
        }        
    }
}
