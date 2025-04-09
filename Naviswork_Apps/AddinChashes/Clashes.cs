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
            //ViewMain viewMain = new ViewMain();
            //IntPtr handle = Application.Gui.MainWindow.Handle;
            //new WindowInteropHelper(viewMain).Owner = handle;
            //viewMain.Show();

            ListarInterferencias();

            return 0;

        }

        public static void ListarInterferencias()
        {
            Autodesk.Navisworks.Api.Document doc = Application.MainDocument;

            if (doc == null || doc.IsClear)
            {
                return;
            }

            var clashTests = doc.GetClash().TestsData;

            if (clashTests == null || clashTests.Tests.Count == 0)
            {
                return;
            }

            List<ClashData> clashDataList = new List<ClashData>();

            foreach (var test in clashTests.Tests)
            {
                ClashTest clashTest = test as ClashTest;
                if (clashTest == null) continue;

                Console.WriteLine($"Test: {clashTest.DisplayName}");

                foreach (var result in clashTest.Children)
                {
                    // Crear el ClashData para este ClashTest
                    var clashData = new ClashData
                    {
                        TestName = clashTest.DisplayName,
                    };

                    if (result is ClashResult clashResult)
                    {
                        var status = clashResult.Status;
                        // Solo agregamos los resultados que no estén resueltos ni aprobados
                        if (status != ClashResultStatus.Resolved && status != ClashResultStatus.Approved)
                        {
                            string statusStr = status.ToString();
                            string name = clashResult.DisplayName;
                            string assignedTo = clashResult.AssignedTo;

                            // Crear un string con los ElementIds de los elementos involucrados
                            //string elementIds = GetSingleElementId(clashResult.Item1) + ";" + GetSingleElementId(clashResult.Item2);

                            // Agregar el ClashResult al ClashData
                            clashData.AddClashResult(clashResult);

                            // Si lo deseas, puedes guardar el ElementId de los elementos involucrados
                            //clashData.ElementIds = elementIds;

                            // Crear el plano de sección (opcional, si lo necesitas)
                            //var bounds = clashResult.BoundingBox;
                            //NavisworkUtils.CreateXYSectionPlane(bounds);

                            // Agregar el resultado a la selección activa
                            //var selection = new ModelItemCollection();
                            //selection.Add(clashResult.CompositeItem1);
                            //selection.Add(clashResult.CompositeItem2);
                            //Application.ActiveDocument.CurrentSelection.CopyFrom(selection);
                        }
                    }
                    else if (result is GroupItem groupItem)
                    {
                        Debug.WriteLine(groupItem.DisplayName);
                        var group = groupItem.Children;

                        foreach (var clash in group)
                        {
                            if (clash is ClashResult clashResultInGroup)
                            {
                                var status = clashResultInGroup.Status;

                                if (status != ClashResultStatus.Resolved && status != ClashResultStatus.Approved)
                                {
                                    string statusStr = clashResultInGroup.Status.ToString();
                                    string assignedTo = clashResultInGroup.AssignedTo;

                                    // Crear un string con los ElementIds de los elementos involucrados
                                    //string elementIds = GetSingleElementId(clashResultInGroup.Item1) + ";" + GetSingleElementId(clashResultInGroup.Item2);

                                    // Agregar el ClashResult al ClashData
                                    clashData.AddClashResult(clashResultInGroup);

                                    // Si lo deseas, puedes guardar el ElementId de los elementos involucrados
                                    //clashData.ElementIds = elementIds;

                                    // Crear el plano de sección (opcional, si lo necesitas)
                                    //var bounds = clashResultInGroup.BoundingBox;
                                    //NavisworkUtils.CreateXYSectionPlane(bounds);

                                    // Agregar el resultado a la selección activa
                                    //var selection = new ModelItemCollection();
                                    //selection.Add(clashResultInGroup.CompositeItem1);
                                    //selection.Add(clashResultInGroup.CompositeItem2);
                                    //Application.ActiveDocument.CurrentSelection.CopyFrom(selection);
                                }
                            }
                        }
                    }
                    clashDataList.Add(clashData);
                }

                // Al final de cada ClashTest, agregar el ClashData a la lista
               
            }

            //foreach (var test in clashTests.Tests)
            //{
            //    ClashTest clashTest = test as ClashTest;
            //    if (clashTest == null) continue;

            //    Console.WriteLine($"Test: {clashTest.DisplayName}");

            //    foreach (var result in clashTest.Children)
            //    {
            //        if (result is ClashResult clashResult)
            //        {
            //            var status = clashResult.Status;
            //            if (status != ClashResultStatus.Resolved ||
            //                status != ClashResultStatus.Approved)
            //            {
            //                string name = clashResult.DisplayName;

            //                var item1 = clashResult.Item1;
            //                var item2 = clashResult.Item2;
            //                var compoItem1 = clashResult.CompositeItem1;
            //                var compoItem2 = clashResult.CompositeItem2;                            

            //                string asing = clashResult.AssignedTo;

            //                string statusStr = status.ToString();

            //                //string ids = GetSingleElementId(item1) + ";" + GetSingleElementId(item2);

            //                var bounds = clashResult.BoundingBox;
            //                NavisworkUtils.CreateXYSectionPlane(bounds);

            //                var selection = new ModelItemCollection();
            //                selection.Add(compoItem1);
            //                selection.Add(compoItem2);

            //                Application.ActiveDocument.CurrentSelection.CopyFrom(selection);

            //            }
            //        }
            //        else if (result is GroupItem groupItem)
            //        {
            //            Debug.WriteLine(groupItem.DisplayName);
            //            var group = groupItem.Children;

            //            foreach (var clash in group)
            //            {
            //                string ids = string.Empty;
            //                if(clash is ClashResult clashResultInGroup)
            //                {
            //                    var item1 = clashResultInGroup.Item1;
            //                    var item2 = clashResultInGroup.Item2;
            //                    var compoItem1 = clashResultInGroup.CompositeItem1;
            //                    var compoItem2 = clashResultInGroup.CompositeItem2;
            //                    //ids = ids +";" + GetSingleElementId(item1) + ";" + GetSingleElementId(item2);

            //                    var bounds = clashResultInGroup.BoundingBox;
            //                    NavisworkUtils.CreateXYSectionPlane(bounds);

            //                    var selection = new ModelItemCollection();
            //                    selection.Add(compoItem1);
            //                    selection.Add(compoItem2);
            //                    Application.ActiveDocument.CurrentSelection.CopyFrom(selection);
            //                }                           
            //            }

            //        }
            //    }
            //}
        }

        private static string GetElementIdsFromItem(ModelItem item)
        {
            var ids = new List<string>();

            //if (item is GroupItem group)
            //{
            //    // Si es un grupo, iteramos todos los sub-items
            //    foreach (var subItem in group.Children)
            //    {
            //        string id = GetSingleElementId(subItem);
            //        if (!string.IsNullOrEmpty(id))
            //            ids.Add(id);
            //    }
            //}
            //else
            //{
            //    // Es un único elemento
            //    string id = GetSingleElementId(item);
            //    if (!string.IsNullOrEmpty(id))
            //        ids.Add(id);
            //}

            return string.Join(";", ids);
        }

        private static string GetSingleElementId(ModelItem item)
        {
            foreach (var category in item.PropertyCategories)
            {
                foreach (var prop in category.Properties)
                {
                    if (prop.DisplayName.ToLower().Equals("id"))
                    {
                        return prop.Value.ToDisplayString();
                    }
                }
            }
            return item.InstanceGuid.ToString(); // fallback
        }

        private static string GetLevelFromItem(ModelItem item)
        {
            // Buscar categoría que contiene el nivel, comúnmente "Element" o "Identity Data"
            foreach (var category in item.PropertyCategories)
            {
                foreach (var prop in category.Properties)
                {
                    if (prop.DisplayName.ToLower().Contains("level") || prop.DisplayName.ToLower().Contains("nivel"))
                    {
                        return prop.Value.ToDisplayString();
                    }
                }
            }
            return "N/A";
        }

        private static string GetGridsFromItem(ModelItem item)
        {
            // Buscar algo relacionado a ejes, por ejemplo "Grid Location" o "Ejes"
            foreach (var category in item.PropertyCategories)
            {
                foreach (var prop in category.Properties)
                {
                    if (prop.DisplayName.ToLower().Contains("grid") || prop.DisplayName.ToLower().Contains("eje"))
                    {
                        return prop.Value.ToDisplayString();
                    }
                }
            }
            return "N/A";
        }

    }
}
