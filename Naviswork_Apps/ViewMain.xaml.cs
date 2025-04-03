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

namespace Naviswork_Apps
{
    /// <summary>
    /// Lógica de interacción para ViewMain.xaml
    /// </summary>
    public partial class ViewMain : Window
    {
        public int index { get; set; }

        public ViewMain()
        {
            InitializeComponent();
        }

        private List<string> GetIds()
        {
            try
            {
                string input = txtIds.Text.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    MessageBox.Show("Ingrese al menos un ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                // Dividir los IDs por ';' y limpiar espacios
                var idsList = input.Split(';')
                                   .Select(id => id.Trim())
                                   .Where(id => !string.IsNullOrEmpty(id))
                                   .ToList();

                if (idsList.Count == 0)
                {
                    //MessageBox.Show("No se encontraron IDs válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    lblMessage.Content = "No se encontraron IDs válidos";
                    return null;
                }

                return idsList;

            }
            catch
            {
                return new List<string>();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Obtener los IDs ingresados por el usuario
            List<string> ids = GetIds();
            if (ids == null || ids.Count == 0) return;

            // Ejecutar la lógica en FocusIds
            int selected = FocusIds.HighlightElements(ids, index);
            UpdateButtonStates(ids.Count, selected);

            lblMessage.Content = "Seleccionados: " + selected.ToString();
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            List<string> ids = GetIds();
            if (ids == null || ids.Count == 0) return;

            if (index < ids.Count - 1)
            {
                index++;
            }

            int selected = FocusIds.HighlightElements(ids, index);
            UpdateButtonStates(ids.Count, selected);

            lblIndex.Content = "Index : " + index.ToString();
            lblMessage.Content = "Seleccionados: " + selected.ToString();
        }

        private void btn_preview_Click(object sender, RoutedEventArgs e)
        {
            List<string> ids = GetIds();
            if (ids == null || ids.Count == 0) return;

            if (index > 0)
            {
                index--;
            }

            int selected = FocusIds.HighlightElements(ids, index);
            UpdateButtonStates(ids.Count, selected);

            lblIndex.Content = "Index : " + index.ToString();
            lblMessage.Content = "Seleccionados: " + selected.ToString();
        }

        private void UpdateButtonStates(int count, int selected)
        {
            btn_preview.IsEnabled = index > 0;
            btn_Next.IsEnabled = index < count - 1;

            //btn_ActiveSection.IsEnabled = selected == 1;
        }

        private void btn_ActiveSection_Click(object sender, RoutedEventArgs e)
        {
            FocusIds.ActivateSection();
        }
    }
}
