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
            ViewMain viewMain = new ViewMain();
            IntPtr handle = Autodesk.Navisworks.Api.Application.Gui.MainWindow.Handle;
            new WindowInteropHelper(viewMain).Owner = handle;
            viewMain.Show();

            return 0;

        }

        public static void HighlightElements(List<string> idsList, int index = 0)
        {
            // Obtener el documento activo
            Autodesk.Navisworks.Api.Document doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
            if (doc == null)
            {
                System.Windows.MessageBox.Show("No hay un documento abierto en Navisworks.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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
                System.Windows.MessageBox.Show("No se encontraron elementos con los IDs especificados.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Destacar elementos
            doc.CurrentSelection.Clear();
            doc.CurrentSelection.CopyFrom(foundItems);
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
            CreateXYSectionPlane(bounds);

            //// Obtener el estado actual de Navisworks
            //ComApi.InwOpState10 state = ComApiBridge.State;

            //// Crear un plano de corte (clip plane)
            //ComApi.InwOaClipPlane clipPlane = (ComApi.InwOaClipPlane)state
            //    .ObjectFactory(ComApi.nwEObjectType.eObjectType_nwOaClipPlane);

            //// Configurar el plano para que sea paralelo al plano XY (vista desde arriba)
            //// Creamos un vector unitario en dirección Z
            //ComApi.InwLUnitVec3f normal = (ComApi.InwLUnitVec3f)state.ObjectFactory(ComApi.nwEObjectType.eObjectType_nwLUnitVec3f);
            //normal.SetValue(0, 0, 1); // Dirección Z+

            //// Creamos un punto para la posición del plano
            //ComApi.InwLPos3f position = (ComApi.InwLPos3f)state.ObjectFactory(ComApi.nwEObjectType.eObjectType_nwLPos3f);
            //position.SetValue(bounds.Center.X, bounds.Center.Y, bounds.Center.Z);




        }

        private static void CreateXYSectionPlane(BoundingBox3D bounds)
        {
            // Obtener el estado de Navisworks
            ComApi.InwOpState10 state = ComApiBridge.State;

            // Crear un vector unitario como la normal del plano de sección (dirección Z+)
            ComApi.InwLUnitVec3f sectionPlaneNormal =
                (ComApi.InwLUnitVec3f)state.ObjectFactory(
                nwEObjectType.eObjectType_nwLUnitVec3f,
                null,
                null);
            sectionPlaneNormal.SetValue(0, 0, -1); // Dirección Z+ (vista desde arriba en plano XY)

            // Crear un plano geométrico
            ComApi.InwLPlane3f sectionPlane =
                (ComApi.InwLPlane3f)state.ObjectFactory(
                nwEObjectType.eObjectType_nwLPlane3f,
                null,
                null);

            // Obtener la colección de planos de recorte
            ComApi.InwClippingPlaneColl2 clipColl =
                (ComApi.InwClippingPlaneColl2)state.CurrentView.ClippingPlanes();

            // Deshabilitar todos los planos de corte existentes
            DisableAllClippingPlanes(clipColl);

            // Crear un nuevo plano de sección
            //clipColl.CreatePlane(clipColl.Count + 1);
            clipColl.CreatePlane(1);

            // Obtener el último plano de sección que acabamos de crear
            //ComApi.InwOaClipPlane clipPlane =
            //(ComApi.InwOaClipPlane)state.CurrentView.ClippingPlanes().Last();

            ComApi.InwOaClipPlane clipPlane =
(ComApi.InwOaClipPlane)state.CurrentView.ClippingPlanes()[1];

            // Calcular la distancia del plano
            // Usamos el centro Z del objeto seleccionado
            double distance = -bounds.Max.Z;

            // Configurar el plano geométrico con el vector normal y la distancia
            sectionPlane.SetValue(sectionPlaneNormal, distance);

            // Asignar el plano geométrico al plano de recorte
            clipPlane.Plane = sectionPlane;

            // Habilitar este plano de sección
            clipPlane.Enabled = true;

            // Opcional: Ajustar la vista para centrarse en el elemento seleccionado
            //AdjustViewToSelection(state, bounds);
        }

        private static void DisableAllClippingPlanes(InwClippingPlaneColl2 clipColl)
        {
            foreach (ComApi.InwOaClipPlane plane in clipColl)
            {
                if (plane != null)
                {
                    plane.Enabled = false;
                }
            }
        }

        private static void AdjustViewToSelection(InwOpState10 state, BoundingBox3D bounds)
        {
            try
            {
                // Crear un cuadro 3D ligeramente más grande que el elemento seleccionado
                double expansion = 0.1; // Factor de expansión (10%)

                // Crear posiciones mínima y máxima para la caja
                ComApi.InwLPos3f minPos = (ComApi.InwLPos3f)state.ObjectFactory(
                    nwEObjectType.eObjectType_nwLPos3f,
                    null,
                    null);
                minPos.data1 = bounds.Min.X - bounds.Size.X * expansion;
                minPos.data2 = bounds.Min.Y - bounds.Size.Y * expansion;
                minPos.data3 = bounds.Min.Z - bounds.Size.Z * expansion;

                ComApi.InwLPos3f maxPos = (ComApi.InwLPos3f)state.ObjectFactory(
                    nwEObjectType.eObjectType_nwLPos3f,
                    null,
                    null);
                maxPos.data1 = bounds.Max.X + bounds.Size.X * expansion;
                maxPos.data2 = bounds.Max.Y + bounds.Size.Y * expansion;
                maxPos.data3 = bounds.Max.Z + bounds.Size.Z * expansion;

                // Crear la caja de límites
                ComApi.InwLBox3f box = (ComApi.InwLBox3f)state.ObjectFactory(
                    nwEObjectType.eObjectType_nwLBox3f,
                    null,
                    null);
                box.min_pos = minPos;
                box.max_pos = maxPos;

                // Ajustar la vista para enfocarse en esta caja
                //state.CurrentView.FitView(box);               

            }
            catch (Exception)
            {
                // Si falla el ajuste de la vista, continuamos sin hacer nada
            }
        }
    }
}
