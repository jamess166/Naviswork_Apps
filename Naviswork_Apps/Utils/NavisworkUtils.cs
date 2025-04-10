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
using Application = Autodesk.Navisworks.Api.Application;
using Autodesk.Navisworks.Api.Interop;
namespace Naviswork_Apps.Utils
{
    public class NavisworkUtils
    {
        public static void CreateXYSectionPlane(BoundingBox3D bounds, bool zoomIt = true)
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

            clipColl.CreatePlane(1);

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
            
            // Aplicar zoom
            if (zoomIt)
            {
                state.ZoomInCurViewOnCurSel();                

                //var selectedItems = Application.ActiveDocument.CurrentSelection.SelectedItems;
                //var comSelection = ComApiBridge.ToInwOpSelection(selectedItems);
                //state.ZoomInCurViewOnSel(comSelection);                
            }

            //Autodesk.Navisworks.Api.Interop.LcRmFrameworkInterface.ExecuteCommand(
            //"Viewpoint.ZoomToSelectedItems", 0);

            //Application.ActiveDocument.Views.ExecuteCommand("Viewpoint.ZoomToSelectedItems");
            //state.ZoomInCurViewOnBoundingBox((InwLBox3f)bounds, 1.2);
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
