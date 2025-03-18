using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                MessageBox.Show("No se encontraron IDs válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            return idsList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Obtener los IDs ingresados por el usuario
            List<string> ids = GetIds();
            if (ids == null || ids.Count == 0) return;

            // Ejecutar la lógica en FocusIds
            FocusIds.HighlightElements(ids);
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            // Obtener los IDs ingresados por el usuario
            List<string> ids = GetIds();
            if (ids == null || ids.Count == 0) return;

            if (index < ids.Count-1)
                index = index + 1;

            // Ejecutar la lógica en FocusIds
            FocusIds.HighlightElements(ids, index);
        }

        private void btn_preview_Click(object sender, RoutedEventArgs e)
        {
            // Obtener los IDs ingresados por el usuario
            List<string> ids = GetIds();
            if (ids == null || ids.Count == 0) return;

            if (index > 0)
                index = index - 1;

            // Ejecutar la lógica en FocusIds
            FocusIds.HighlightElements(ids, index);
        }

        private void btn_ActiveSection_Click(object sender, RoutedEventArgs e)
        {
            FocusIds.ActivateSection();
        }
    }
}
