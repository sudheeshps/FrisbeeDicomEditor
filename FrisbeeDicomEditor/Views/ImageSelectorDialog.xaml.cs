using FrisbeeDicomEditor.Models;
using FrisbeeDicomEditor.ViewModels;
using Dicom.Imaging;
using System;
using System.Windows;

namespace FrisbeeDicomEditor.Views
{
    
    /// <summary>
    /// Interaction logic for ImageSelectorDialog.xaml
    /// </summary>
    public partial class ImageSelectorDialog : HandyControl.Controls.Window
    {
        private ImageSelectorDialogViewModel _imageSelectorDialogViewModel;
        public ImageSelectorDialog()
        {
            InitializeComponent();
            _imageSelectorDialogViewModel = new ImageSelectorDialogViewModel();
            DataContext = _imageSelectorDialogViewModel;
        }
        public SelectedImageInfo GetSelectedImageInfo()
        {
            return new SelectedImageInfo()
            {
                PhotometricInterpretation = _imageSelectorDialogViewModel.SelectedPhotometricInterpretation,
                BitsAllocated = _imageSelectorDialogViewModel.BitsAllocated,
                BitsStored = _imageSelectorDialogViewModel.BitsStored,
                HighBit = _imageSelectorDialogViewModel.HighBit,
                SamplesPerPixel = _imageSelectorDialogViewModel.SamplesPerPixel,
                PixelRepresentation = _imageSelectorDialogViewModel.SelectedPixelRepresentation,
                PlanarConfiguration = _imageSelectorDialogViewModel.SelectedPlanarConfiguration,
                ImagePath = _imageSelectorDialogViewModel.ImageFilePath
            };
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidFile(_imageSelectorDialogViewModel.ImageFilePath))
            {
                MessageBox.Show("Please choose a valid file (jpeg, png or bmp)","Image not valid!",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private bool IsValidFile(string imageFilePath)
        {
            return !string.IsNullOrEmpty(imageFilePath) &&
                (imageFilePath.EndsWith(".jpg") ||
                imageFilePath.EndsWith(".jpeg") ||
                imageFilePath.EndsWith(".png") ||
                imageFilePath.EndsWith(".bmp"));
        }

        private void Cancel_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
