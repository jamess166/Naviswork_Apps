using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Naviswork_Apps.AddinChashes
{
    public class ClashUtils
    {
        public static List<ClashData> GetInterferences()
        {
            var document = Application.MainDocument;
            if (!IsValidDocument(document)) return new List<ClashData>();

            var clashTests = document.GetClash().TestsData;
            if (clashTests == null || clashTests.Tests.Count == 0) return new List<ClashData>();

            return clashTests.Tests
                .OfType<ClashTest>()
                .SelectMany(ProcessClashTest)
                .ToList();
        }

        public static bool IsValidDocument(Autodesk.Navisworks.Api.Document doc)
        {
            return doc != null && !doc.IsClear;
        }

        private static IEnumerable<ClashData> ProcessClashTest(ClashTest clashTest)
        {
            Console.WriteLine($"Test: {clashTest.DisplayName}");
            var clashTestName = clashTest.DisplayName;

            foreach (var result in clashTest.Children)
            {
                var clashData = new ClashData
                {
                    TestName = clashTestName,
                    ClashName = result.DisplayName
                };

                switch (result)
                {
                    case ClashResult clashResult when IsRelevantClash(clashResult):
                        clashData.Status = clashResult.Status.ToString();
                        clashData.AssignedTo = clashResult.AssignedTo;
                        clashData.BoundingBoxResults3D = clashResult.BoundingBox;
                        clashData.AddModelItems(clashResult);
                        //clashData.ComputeBoundingBox();
                        yield return clashData;
                        break;                        
                    case ClashResultGroup groupItem:       
                        clashData.Status = groupItem.Status.ToString();
                        clashData.AssignedTo = groupItem.AssignedTo;
                        clashData.BoundingBoxResults3D = groupItem.BoundingBox;
                        clashData = ProcessGroupItem(groupItem, clashData);
                        //clashData.ComputeBoundingBox();
                        yield return clashData;                      
                        break;
                }
            }
        }

        private static ClashData ProcessGroupItem(ClashResultGroup groupItem, ClashData clashData)
        {
            foreach (var clash in groupItem.Children.OfType<ClashResult>())
            {
                if (IsRelevantClash(clash))
                {
                    clashData.AddModelItems(clash);
                    //if (!string.IsNullOrEmpty(clashData.Status)) continue;

                    //clashData.Status = clash.Status.ToString();                        
                }
            }

            return clashData;
        }
   

        //private static IEnumerable<ClashData> ProcessGroupItem(GroupItem groupItem)
        //{
        //    foreach (var clash in groupItem.Children.OfType<ClashResult>())
        //    {
        //        if (IsRelevantClash(clash))
        //        {
        //            var clashData = new ClashData();
        //            clashData.AddClashResult(clash);
        //            yield return clashData;
        //        }
        //    }
        //}

        private static bool IsRelevantClash(ClashResult clashResult)
        {
            return !(clashResult.Status == ClashResultStatus.Resolved ||
                clashResult.Status == ClashResultStatus.Approved);
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
            return item.InstanceGuid.ToString(); 
        }

        private static string GetLevelFromItem(ModelItem item)
        {
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
