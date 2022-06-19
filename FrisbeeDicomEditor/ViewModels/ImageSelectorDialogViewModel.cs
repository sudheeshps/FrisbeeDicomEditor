using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dicom.Imaging;
using System.Collections.Generic;
using System.Windows.Input;

namespace FrisbeeDicomEditor.ViewModels
{
    public class ImageSelectorDialogViewModel : ObservableObject
    {
        public List<PhotometricInterpretation> PhotometricInterpretations { get; } = new List<PhotometricInterpretation>()
        {
            { PhotometricInterpretation.Monochrome1 }, { PhotometricInterpretation.Monochrome2 },
            { PhotometricInterpretation.Rgb}, { PhotometricInterpretation.YbrFull},
            { PhotometricInterpretation.YbrFull422}, { PhotometricInterpretation.YbrIct},
            { PhotometricInterpretation.YbrPartial420}, { PhotometricInterpretation.YbrPartial422},
            { PhotometricInterpretation.YbrRct}
        };

        private PhotometricInterpretation _selectedPhotometricInterpretation = PhotometricInterpretation.YbrFull;
        public PhotometricInterpretation SelectedPhotometricInterpretation
        {
            get => _selectedPhotometricInterpretation;
            set => SetProperty(ref _selectedPhotometricInterpretation, value);
        }

        private int _bitsAllocated = 8;
        public int BitsAllocated { get => _bitsAllocated; set => SetProperty(ref _bitsAllocated, value); }

        private ushort _bitsStored = 8;
        public ushort BitsStored { get => _bitsStored; set => SetProperty(ref _bitsStored, value); }
        private ushort _samplesPerPixel = 3;
        public ushort SamplesPerPixel { get => _samplesPerPixel; set => SetProperty(ref _samplesPerPixel, value); }
        private ushort _highBit = 7;
        public ushort HighBit { get => _highBit; set => SetProperty(ref _highBit, value); }
        public List<PixelRepresentation> PixelRepresentations { get; } =
            new List<PixelRepresentation>() { { Dicom.Imaging.PixelRepresentation.Signed},
                { Dicom.Imaging.PixelRepresentation.Unsigned} };
        private PixelRepresentation _selectedPixelRepresentation = PixelRepresentation.Unsigned;
        public PixelRepresentation SelectedPixelRepresentation
        {
            get => _selectedPixelRepresentation;
            set => SetProperty(ref _selectedPixelRepresentation, value);
        }
        public List<PlanarConfiguration> PlanarConfigurations { get; } = 
            new List<PlanarConfiguration>() { {Dicom.Imaging.PlanarConfiguration.Interleaved }, 
                { Dicom.Imaging.PlanarConfiguration.Planar} };
        private PlanarConfiguration _selectedPlanarConfiguration = PlanarConfiguration.Interleaved;
        public PlanarConfiguration SelectedPlanarConfiguration
        {
            get => _selectedPlanarConfiguration;
            set => SetProperty(ref _selectedPlanarConfiguration, value);
        }


        private string _imageFilePath;
        public string ImageFilePath
        {
            get => _imageFilePath;
            set => SetProperty(ref _imageFilePath, value);
        }

        private RelayCommand _browseImageCommand;

        public ICommand BrowseImageCommand
        {
            get
            {
                if (_browseImageCommand == null)
                {
                    _browseImageCommand = new RelayCommand(BrowseImage);
                }

                return _browseImageCommand;
            }
        }
        private void BrowseImage()
        {
            var openFileDlg = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Open image File",
                Filter = "JPEG File |*.jpg|JPEG File |*.jpeg|" +
                         "PNG File |*.png|Bitmap File|*.bmp"
            };

            var result = openFileDlg.ShowDialog();
            if (result == true)
            {
                ImageFilePath = openFileDlg.FileName;
            }
        }
    }
}