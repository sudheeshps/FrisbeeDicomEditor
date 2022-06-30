using FrisbeeDicomEditor.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Dicom;
using Dicom.Imaging;
using Dicom.IO.Buffer;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using DicomItem = Dicom.DicomItem;

namespace FrisbeeDicomEditor.Services
{
    public class DicomFileStateEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public Exception Exception { get; set; }
    }
    public class DicomDatasetLoadStartedEventArgs : EventArgs
    {
        public DicomDataset DicomDataset { get; set; }
    }
    public enum DicomItemType
    {
        Normal,
        ImageAttribute,
        SequenceItem,
        PixelData
    }
    public class DicomItemReadEventArgs : EventArgs
    {
        public DicomDataset DicomDataset { get; set; }
        public DicomItem DicomItem { get; set; }
        public DicomItemType DicomItemType { get; set; }
    }
    public class DicomDatasetLoadCompletedArgs : EventArgs
    {
        public DicomDataset DicomDataset { get; set; }
    }

    public class DicomDataService : ObservableObject
    {
        private DicomDataset _dataset;

        public EventHandler<DicomFileStateEventArgs> FileOpenSuccess;
        public EventHandler<DicomFileStateEventArgs> FileOpenFailed;
        public EventHandler<DicomFileStateEventArgs> FileSaveSuccess;
        public EventHandler<DicomFileStateEventArgs> FileSaveFailed;
        public EventHandler<DicomFileStateEventArgs> ReplaceImageSuccess;
        public EventHandler<DicomFileStateEventArgs> ReplaceImageFailed;

        public EventHandler<DicomDatasetLoadStartedEventArgs> DicomDatasetLoadStarted;
        public EventHandler<DicomItemReadEventArgs> DicomItemRead;
        public EventHandler<DicomDatasetLoadCompletedArgs> DicomDatasetLoadCompleted;
        public ObservableCollection<Models.DicomItem> DicomItems { get; } = new ObservableCollection<Models.DicomItem>();
        public ObservableCollection<Models.DicomItem> ImageAttributes { get; } = new ObservableCollection<Models.DicomItem>();
        private ObservableCollection<TreeViewItem> _sequences = new ObservableCollection<TreeViewItem>();
        public ObservableCollection<TreeViewItem> Sequences
        {
            get => _sequences;
            set => SetProperty(ref _sequences, value);
        }
        public async Task<bool> LoadDicomFileAsync(string fileName)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    var dicomFile = await DicomFile.OpenAsync(fileStream, FileReadOption.ReadAll);
                    if (dicomFile == null)
                    {
                        FileOpenFailed?.Invoke(this, new DicomFileStateEventArgs() { FileName = fileName });
                        return false;
                    }
                    _dataset = dicomFile.Dataset.Clone();
                    LoadDicomDataset();
                    FileOpenSuccess?.Invoke(this, new DicomFileStateEventArgs() { FileName = fileName });
                    return true;
                }
            }
            catch (Exception ex)
            {
                FileOpenFailed?.Invoke(this, new DicomFileStateEventArgs() { FileName = fileName, Exception = ex });
                return false;
            }
        }
        public async Task<bool> SaveDicomFileAsync(string fileName)
        {
            try
            {
                var dicomFile = new DicomFile(_dataset);
                await dicomFile.SaveAsync(fileName);
                FileSaveSuccess?.Invoke(this, new DicomFileStateEventArgs() { FileName = fileName });
                return true;
            }
            catch (Exception ex)
            {
                FileSaveFailed?.Invoke(this, new DicomFileStateEventArgs() { FileName = fileName, Exception = ex });
                return false;
            }
        }
        public void ReplacePixelData(string fileName, SelectedImageInfo selectedImageInfo)
        {
            try
            {
                var bitmap = new Bitmap(fileName);
                var imageFormat = GetImageFormat(fileName);
                var pixels = GetPixels(bitmap, imageFormat, out var rows, out var columns);
                var buffer = new MemoryByteBuffer(pixels);
                AddOrUpdatePixelTags(selectedImageInfo, rows, columns);
                AddPixelData(selectedImageInfo, rows, columns, buffer);
                LoadDicomDataset();
                ReplaceImageSuccess?.Invoke(this, new DicomFileStateEventArgs() { FileName = fileName });
            }
            catch (Exception ex)
            {
                ReplaceImageFailed?.Invoke(this, new DicomFileStateEventArgs() { Exception = ex });
            }
        }

        private void AddPixelData(SelectedImageInfo selectedImageInfo, int rows, int columns, MemoryByteBuffer buffer)
        {
            var pixelData = DicomPixelData.Create(_dataset, true);
            pixelData.BitsStored = selectedImageInfo.BitsStored;
            pixelData.SamplesPerPixel = selectedImageInfo.SamplesPerPixel;
            pixelData.HighBit = selectedImageInfo.HighBit;
            pixelData.PhotometricInterpretation = selectedImageInfo.PhotometricInterpretation;
            pixelData.PixelRepresentation = selectedImageInfo.PixelRepresentation;
            pixelData.PlanarConfiguration = selectedImageInfo.PlanarConfiguration;
            pixelData.Height = (ushort)rows;
            pixelData.Width = (ushort)columns;
            pixelData.AddFrame(buffer);
        }

        private void AddOrUpdatePixelTags(SelectedImageInfo selectedImageInfo, int rows, int columns)
        {
            _dataset.AddOrUpdate(DicomTag.PhotometricInterpretation,
                                selectedImageInfo.PhotometricInterpretation.Value);
            _dataset.AddOrUpdate(DicomTag.Rows, (ushort)rows);
            _dataset.AddOrUpdate(DicomTag.Columns, (ushort)columns);
            _dataset.AddOrUpdate(DicomTag.BitsAllocated, (ushort)selectedImageInfo.BitsAllocated);
        }

        private System.Drawing.Imaging.ImageFormat GetImageFormat(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg": return System.Drawing.Imaging.ImageFormat.Jpeg;
                case ".bmp": return System.Drawing.Imaging.ImageFormat.Bmp;
                case ".png": return System.Drawing.Imaging.ImageFormat.Png;
            }
            return null;
        }

        private static byte[] GetPixels(Bitmap bitmap, System.Drawing.Imaging.ImageFormat imageFormat,
            out int rows, out int columns)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, imageFormat);
                rows = bitmap.Height;
                columns = bitmap.Width;
                return stream.ToArray();
            }
        }
        private void LoadDicomDataset()
        {
            DicomDatasetLoadStarted?.Invoke(this, new DicomDatasetLoadStartedEventArgs() { DicomDataset = _dataset });
            foreach (var dataItem in _dataset)
            {
                var dicomDataType = DicomItemType.Normal;
                if (dataItem.Tag == DicomTag.Rows || dataItem.Tag == DicomTag.Columns ||
                dataItem.Tag == DicomTag.BitsAllocated || dataItem.Tag == DicomTag.PhotometricInterpretation)
                {
                    dicomDataType = DicomItemType.ImageAttribute;
                }
                if (dataItem.ValueRepresentation == DicomVR.SQ)
                {
                    dicomDataType = DicomItemType.SequenceItem;
                }
                DicomItemRead?.Invoke(this, new DicomItemReadEventArgs()
                {
                    DicomDataset = _dataset,
                    DicomItem = dataItem,
                    DicomItemType = dicomDataType
                });
            }
            DicomDatasetLoadCompleted?.Invoke(this, new DicomDatasetLoadCompletedArgs() { DicomDataset = _dataset });
            if (_dataset.Contains(DicomTag.PixelData))
            {
                DicomItemRead?.Invoke(this, new DicomItemReadEventArgs()
                {
                    DicomDataset = _dataset,
                    DicomItemType = DicomItemType.PixelData
                });
            }
        }
    }
}
