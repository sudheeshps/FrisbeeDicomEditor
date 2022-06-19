using CommunityToolkit.Mvvm.ComponentModel;
using Dicom;

namespace FrisbeeDicomEditor.Models
{
    public class DicomItem : ObservableObject
    {
        
        public DicomItem()
        {
        }
        public DicomDataset Dataset { get; set; }
        public Dicom.DicomItem SourceDicomItem { get; set; }
        private bool _isSelected = false;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public DicomTag DicomTag { get; set; }
        public ushort Group { get; set; }
        public ushort Element { get; set; }
        public DicomVR DicomVR { get; set; }
        private object _value;
        public object Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        public string Description { get; set; }
    }
}
