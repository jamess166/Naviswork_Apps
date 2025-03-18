using System;
using System.Windows.Forms;
using System.Drawing;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using Autodesk.Navisworks.Api.Interop;
using Autodesk.Navisworks.Internal.ApiImplementation;
using Application = Autodesk.Navisworks.Api.Application;
using Color = System.Drawing.Color;
using ComApiBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApiAutomation;
using Autodesk.Navisworks.Api.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;
using System.Linq;

//[Plugin("ElevationTracker", "ACME", ToolTip = "Muestra la elevación bajo el cursor", DisplayName = "Seguimiento de Elevación")]
public class ElevationTracker : AddInPlugin
{
    private Form overlayForm;
    private Label elevationLabel;
    private Timer updateTimer;
    private bool isActive = false;

    public override int Execute(params string[] parameters)
    {
        if (isActive)
        {
            // Si ya está activo, detener el seguimiento
            StopTracking();
            return 0;
        }

        // Comenzar el seguimiento
        StartTracking();
        return 1;
    }

    private void StartTracking()
    {
        // Crear un formulario flotante semitransparente sin bordes
        overlayForm = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            BackColor = Color.FromArgb(40, 40, 40),
            Opacity = 0.8,
            TopMost = true,
            Size = new Size(200, 70),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(10, 10),
            ShowInTaskbar = false
        };

        // Permitir arrastrar el formulario
        overlayForm.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(overlayForm.Handle, 0xA1, 0x2, 0);
            }
        };

        // Botón de cierre
        Button closeButton = new Button
        {
            Text = "X",
            Size = new Size(25, 25),
            Location = new Point(overlayForm.Width - 30, 5),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(60, 60, 60)
        };
        closeButton.Click += (s, e) => StopTracking();
        overlayForm.Controls.Add(closeButton);

        // Etiqueta para mostrar información de elevación
        elevationLabel = new Label
        {
            Text = "Elevación: --",
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(180, 50),
            Location = new Point(10, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };
        overlayForm.Controls.Add(elevationLabel);

        // Etiqueta de título
        Label titleLabel = new Label
        {
            Text = "Seguimiento de Elevación",
            ForeColor = Color.LightBlue,
            AutoSize = false,
            Size = new Size(170, 20),
            Location = new Point(10, 8),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(elevationLabel.Font, FontStyle.Bold)
        };
        overlayForm.Controls.Add(titleLabel);

        // Crear temporizador para actualizar la información
        updateTimer = new Timer
        {
            Interval = 100 // Actualizar cada 100ms
        };
        updateTimer.Tick += UpdateElevation;
        updateTimer.Start();

        overlayForm.Show();
        isActive = true;
    }

    private void UpdateElevation(object sender, EventArgs e)
    {
        //try
        //{
        //    // Obtener el documento activo
        //    Document doc = Application.ActiveDocument;
        //    if (doc == null) return;

        //    // Acceder a la API COM de Navisworks
        //    InwOpState10 state = ComApiBridge.State;
        //    InwViewerViewer viewer = state as InwViewerViewer;
        //    if (viewer == null) return;

        //    // Obtener la posición del cursor en la vista
        //    int x = System.Windows.Forms.Cursor.Position.X;
        //    int y = Cursor.Position.Y;

        //    // Lanzar un rayo desde el cursor
        //    InwLinerRay ray = viewer.Viewer.ScreenToWorld(x, y);
        //    if (ray == null) return;

        //    // Obtener la primera intersección con un objeto
        //    var selection = state.GetSelection();
        //    if (selection == null || selection.Count == 0) return;

        //    // Acceder al primer objeto en la selección
        //    InwOaPath path = selection[1] as InwOaPath;
        //    if (path == null) return;

        //    InwOaFragment3 fragment = path.Objects().FirstOrDefault() as InwOaFragment3;
        //    if (fragment == null) return;

        //    // Obtener la elevación (coordenada Z)
        //    InwVector3f intersectionPoint = fragment.WorldPosition();
        //    double elevation = intersectionPoint.z;

        //    MessageBox.Show($"Elevación: {elevation:F3} m", "Elevación", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //}
        //catch (System.Exception ex)
        //{
        //    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //}
    }



    private string GetItemName(ModelItem item)
    {
        if (item == null)
            return "N/A";

        // Intentar obtener nombre del elemento
        PropertyCategory category = item.PropertyCategories.FindCategoryByDisplayName("Element");
        if (category != null)
        {
            DataProperty nameProp = category.Properties.FindPropertyByDisplayName("Name");
            if (nameProp != null && nameProp.Value != null)
                return nameProp.Value.ToString();

            // Si no hay Name, intentar con Id
            DataProperty idProp = category.Properties.FindPropertyByDisplayName("Id");
            if (idProp != null && idProp.Value != null)
                return "ID: " + idProp.Value.ToString();
        }

        return item.DisplayName;
    }

    private string GetCurrentUnits()
    {
        try
        {
            // Obtener las unidades actuales del documento
            Units units = Application.ActiveDocument.Units;
            return units.ToString();
        }
        catch
        {
            return "m"; // Valor predeterminado si no se pueden determinar las unidades
        }
    }

    private void StopTracking()
    {
        if (updateTimer != null)
        {
            updateTimer.Stop();
            updateTimer.Dispose();
            updateTimer = null;
        }

        if (overlayForm != null)
        {
            overlayForm.Close();
            overlayForm.Dispose();
            overlayForm = null;
        }

        isActive = false;
    }
}

// Clase auxiliar para interacción con Windows para permitir arrastrar la ventana
internal static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
}