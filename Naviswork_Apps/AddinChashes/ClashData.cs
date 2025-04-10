using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using System;
using System.Collections.Generic;

public class ClashData
{
    public string TestName { get; set; }
    public string ClashName { get; set; }

    public string Status { get; set; }
    public string AssignedTo { get; set; }
    public ModelItemCollection ModelItems { get; set; }
    public List<ClashResult> ClashResults { get; set; }
    public BoundingBox3D BoundingBoxResults3D { get; set; }
    public string ElementIds { get; set; } 

    public ClashData()
    {
        ModelItems = new ModelItemCollection();
        ClashResults = new List<ClashResult>();
    }

    public void AddModelItems(ClashResult clashResult)
    {
        if (clashResult != null)
        {
            ClashResults.Add(clashResult);
            if (clashResult.Item1 != null && !ModelItems.Contains(clashResult.Item1))
            {
                ModelItems.Add(clashResult.Item1);
            }

            if (clashResult.Item2 != null && !ModelItems.Contains(clashResult.Item2))
            {
                ModelItems.Add(clashResult.Item2);
            }
        }
    }

    public void ComputeBoundingBox()
    {
        if (ModelItems == null || ModelItems.Count == 0)
            return;

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        foreach (var modelItem in ModelItems)
        {
            var bbox = modelItem.BoundingBox();
            if (bbox == null) continue;

            var min = bbox.Min;
            var max = bbox.Max;

            minX = Math.Min(minX, min.X);
            minY = Math.Min(minY, min.Y);
            minZ = Math.Min(minZ, min.Z);

            maxX = Math.Max(maxX, max.X);
            maxY = Math.Max(maxY, max.Y);
            maxZ = Math.Max(maxZ, max.Z);
        }

        var finalMin = new Point3D(minX, minY, minZ);
        var finalMax = new Point3D(maxX, maxY, maxZ);

        BoundingBoxResults3D = new BoundingBox3D(finalMin, finalMax);
    }

}


