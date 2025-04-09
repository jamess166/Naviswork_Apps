using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using System.Collections.Generic;

public class ClashData
{
    public string TestName { get; set; }
    public string Status { get; set; }
    public string AssignedTo { get; set; }
    public List<ModelItem> ModelItems { get; set; }
    public List<ClashResult> ClashResults { get; set; }
    public string ElementIds { get; set; }  // Para almacenar los ElementIds de los ModelItems

    public ClashData()
    {
        ModelItems = new List<ModelItem>();
        ClashResults = new List<ClashResult>();
    }

    public void AddClashResult(ClashResult clashResult)
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
}


