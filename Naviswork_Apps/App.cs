using Autodesk.Navisworks.Api.Plugins;

namespace Naviswork_Apps
{
    [Plugin("AppCesel", "fCabrera", DisplayName = "AppCesel")]
    [RibbonLayout("Naviswork_Apps.xaml")]
    [RibbonTab("ID_CESEL_TAB", DisplayName = "CESEL")]
    [Command("ID_ButtonFocusIds", DisplayName = "Focus \n Ids", Icon = "Resources\\cesel16x16.png", LargeIcon = "Resources\\cesel32x32.png", ToolTip = "Buscar Ids")]

    public class App: CommandHandlerPlugin
    {
        public override int ExecuteCommand(string name, params string[] parameters)
        {
            switch (name)
            {
                case "ID_ButtonFocusIds":
                    new FocusIds().Execute();
                    break;                
            }
            return 0;
        }
    }
}
