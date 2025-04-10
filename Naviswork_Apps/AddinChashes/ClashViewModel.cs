using Autodesk.Navisworks.Api;
using Naviswork_Apps.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Naviswork_Apps.AddinChashes
{
    public class ClashViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ClashData> ClashList { get; set; }
        public Document document { get; set; }

        private ClashData _selectedClash;
        public ClashData SelectedClash
        {
            get => _selectedClash;
            set
            {
                _selectedClash = value;
                OnPropertyChanged(nameof(SelectedClash));
                HandleSelectionChanged(_selectedClash);
            }
        }

        public ClashViewModel()
        {
            ClashList = new ObservableCollection<ClashData>(ClashUtils.GetInterferences());

            document = Application.MainDocument;
            if (!ClashUtils.IsValidDocument(document)) return;
        }

        private void HandleSelectionChanged(ClashData selected)
        {
            if (selected == null) return;

            //BoundingBox3D bounds = selected.ClashResults[0]?.BoundingBox;


            //ModelItemCollection modelItems = new ModelItemCollection();
            //foreach (var item in selected.ModelItems)
            //{
            //    modelItems.Add(item);
            //}

            document.CurrentSelection.Clear();
            document.CurrentSelection.CopyFrom(selected.ModelItems);

            if (selected.BoundingBoxResults3D != null)
                NavisworkUtils.CreateXYSectionPlane(selected.BoundingBoxResults3D, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
