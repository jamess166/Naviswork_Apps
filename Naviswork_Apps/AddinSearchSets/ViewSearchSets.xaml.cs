using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Autodesk.Navisworks.Api;
using Application = Autodesk.Navisworks.Api.Application;

namespace Naviswork_Apps.AddinSearchSets
{
    /// <summary>
    /// Ventana para elegir de que modelos se crean las busquedas por PR-Elemento.
    /// </summary>
    public partial class ViewSearchSets : Window
    {
        private readonly ObservableCollection<ModelRow> _rows = new ObservableCollection<ModelRow>();

        public ViewSearchSets()
        {
            InitializeComponent();

            Document doc = Application.ActiveDocument;
            if (doc != null && !doc.IsClear)
            {
                foreach (Model model in doc.Models)
                {
                    string name = SearchSets.GetModelName(model);
                    _rows.Add(new ModelRow
                    {
                        IsSelected = false,
                        Name = name,
                        Prefix = SearchSets.GetSixthPart(name),
                        Model = model
                    });
                }
            }

            lstModels.ItemsSource = _rows;
            lblStatus.Text = _rows.Count + " modelo(s) en el documento.";
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e) { SetAll(true); }

        private void SelectNone_Click(object sender, RoutedEventArgs e) { SetAll(false); }

        private void SetAll(bool value)
        {
            foreach (ModelRow row in _rows) row.IsSelected = value;
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            Run(preview: true);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            Run(preview: false);
        }

        private void Run(bool preview)
        {
            Document doc = Application.ActiveDocument;
            if (doc == null || doc.IsClear)
            {
                MessageBox.Show("No hay un documento activo.", "Aviso");
                return;
            }

            List<Model> selected = _rows.Where(r => r.IsSelected).Select(r => r.Model).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un modelo.", "Aviso");
                return;
            }

            btnPreview.IsEnabled = false;
            btnCreate.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                SearchSetsResult result = preview
                    ? SearchSets.BuildPlan(doc, selected)
                    : SearchSets.CreateForModels(doc, selected);

                txtLog.Text = result.ToLog(preview);
                lblStatus.Text = preview
                    ? string.Format("Faltan {0} busqueda(s)   Ya existen: {1}   Omitidos: {2}",
                        result.TotalMissing, result.TotalExisting, result.TotalSkipped)
                    : string.Format("Carpetas creadas: {0}   Busquedas creadas: {1}   Ya existian: {2}   Omitidos: {3}",
                        result.CreatedFolders, result.CreatedSets, result.TotalExisting, result.TotalSkipped);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error");
            }
            finally
            {
                Mouse.OverrideCursor = null;
                btnPreview.IsEnabled = true;
                btnCreate.IsEnabled = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public class ModelRow : INotifyPropertyChanged
        {
            private bool _isSelected;

            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if (_isSelected == value) return;
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }

            public string Name { get; set; }
            public string Prefix { get; set; }
            public Model Model { get; set; }

            public string PrefixLabel
            {
                get
                {
                    return string.IsNullOrEmpty(Prefix)
                        ? "(nombre sin 6 partes)"
                        : "->  carpeta \"" + Prefix + "\"";
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
