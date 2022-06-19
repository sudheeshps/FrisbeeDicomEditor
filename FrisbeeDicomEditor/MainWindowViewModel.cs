using CommunityToolkit.Mvvm.ComponentModel;
using Dicom;
using Dicom.Imaging;
using FrisbeeDicomEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using FrisbeeDicomEditor.Services;
using FrisbeeDicomEditor.Views;
using System.Threading.Tasks;

namespace FrisbeeDicomEditor
{
    class MainWindowViewModel : ObservableObject
    {
        private const string DicomEditorName = "Frisbee DICOM Editor";
        private string _windowTitle = DicomEditorName;

        #region public
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        private WriteableBitmap _dicomImage;
        public WriteableBitmap DicomImage
        {
            get => _dicomImage;
            set => SetProperty(ref _dicomImage, value);
        }
        private bool _imageLoaded = false;
        public bool ImageLoaded
        {
            get => _imageLoaded;
            set => SetProperty(ref _imageLoaded, value);
        }
        private int _headerRowSpan = 1;
        public int HeaderRowSpan
        {
            get => _headerRowSpan;
            set => SetProperty(ref _headerRowSpan, value);
        }
        private ObservableCollection<TreeViewItem> _sequences;
        public ObservableCollection<TreeViewItem> Sequences
        {
            get => _sequences;
            set => SetProperty(ref _sequences, value);
        }
        public bool IsDirty { get; set; } = false;
        private DicomDataService _dicomDataService;
        private List<DicomSequence> CurrentSequences { get; } = new List<DicomSequence>();
        public MainWindowViewModel(DicomDataService dicomDataService)
        {
            _dicomDataService = dicomDataService;

            _dicomDataService.FileOpenSuccess += OnFileOpened;
            _dicomDataService.FileOpenFailed += OnFileOpenFailed;
            _dicomDataService.FileSaveSuccess += OnFileSaved;
            _dicomDataService.FileSaveFailed += OnFileSaveFailed;
            _dicomDataService.ReplaceImageSuccess += OnImageReplaced;
            _dicomDataService.ReplaceImageFailed += OnImageReplaceFailed;
            _dicomDataService.DicomItemRead += OnDicomItemRead;
            _dicomDataService.DicomDatasetLoadStarted += OnDicomDatasetLoadStarted;
            _dicomDataService.DicomDatasetLoadCompleted += OnDicomDatasetLoadCompleted;
            ImageManager.SetImplementation(WPFImageManager.Instance);
        }

        

        public ObservableCollection<Models.DicomItem> DicomItems { get; } = new ObservableCollection<Models.DicomItem>();
        public ObservableCollection<Models.DicomItem> ImageAttributes { get; } = new ObservableCollection<Models.DicomItem>();

        private ICollectionView _dicomItemsView = null;
        public ICollectionView DicomItemsView
        {
            get => _dicomItemsView;
            set => SetProperty(ref _dicomItemsView, value);
        }
        private ICollectionView _imageAttributesView = null;
        public ICollectionView ImageAttributesView
        {
            get => _imageAttributesView;
            set => SetProperty(ref _imageAttributesView, value);
        }
        private ICollectionView _sequenceItemsView = null;
        public ICollectionView SequenceItemsView
        {
            get => _sequenceItemsView;
            set => SetProperty(ref _sequenceItemsView, value);
        }
        private Dictionary<DicomTag, ObservableCollection<Models.DicomItem>> _sequenceTable = new Dictionary<DicomTag, ObservableCollection<Models.DicomItem>>();
        private ObservableCollection<Models.DicomItem> _selectedSequenceItems;
        public ObservableCollection<Models.DicomItem> SelectedSequenceItems
        {
            get => _selectedSequenceItems;
            set => SetProperty(ref _selectedSequenceItems, value);
        }
        private RelayCommand _openDicomFileCommand;
        public ICommand OpenDicomFileCommand
        {
            get
            {
                if (_openDicomFileCommand == null)
                {
                    _openDicomFileCommand = new RelayCommand(OpenDicomFile);
                }
                return _openDicomFileCommand;
            }
        }
        private RelayCommand _saveDicomFileCommand;
        public ICommand SaveDicomFileCommand
        {
            get
            {
                if (_saveDicomFileCommand == null)
                {
                    _saveDicomFileCommand = new RelayCommand(SaveDicomFileCommandExecutorAsync);
                }
                return _saveDicomFileCommand;
            }
        }
        private RelayCommand<string> _headerSearchTextChangedCommand;
        public ICommand HeaderSearchTextChangedCommand
        {
            get
            {
                if (_headerSearchTextChangedCommand == null)
                {
                    _headerSearchTextChangedCommand = new RelayCommand<string>(text => OnHeaderSearchTextChanged(text));
                }
                return _headerSearchTextChangedCommand;
            }
        }
        private void OnHeaderSearchTextChanged(string text)
        {
            SearchText = text;
            DicomItemsView.Refresh();
        }
        private RelayCommand<string> _imageAttributesSearchTextChangedCommand;
        public ICommand ImageAttributesSearchTextChangedCommand
        {
            get
            {
                if (_imageAttributesSearchTextChangedCommand == null)
                {
                    _imageAttributesSearchTextChangedCommand = new RelayCommand<string>(text => OnImageAttributesSearchTextChanged(text));
                }
                return _imageAttributesSearchTextChangedCommand;
            }
        }
        private void OnImageAttributesSearchTextChanged(string text)
        {
            ImageAttributesSearchText = text;
            ImageAttributesView.Refresh();
        }
        private RelayCommand<string> _sequenceItemsSearchTextChangedCommand;
        public ICommand SequenceItemsSearchTextChangedCommand
        {
            get
            {
                if (_sequenceItemsSearchTextChangedCommand == null)
                {
                    _sequenceItemsSearchTextChangedCommand = new RelayCommand<string>(text => OnSequenceItemsSearchTextChanged(text));
                }
                return _sequenceItemsSearchTextChangedCommand;
            }
        }
        private void OnSequenceItemsSearchTextChanged(string text)
        {
            SequenceItemsSearchText = text;
            SequenceItemsView.Refresh();
        }
        private RelayCommand _replaceImageCommand;
        public ICommand ReplaceImageCommand
        {
            get
            {
                if (_replaceImageCommand == null)
                {
                    _replaceImageCommand = new RelayCommand(ReplaceImage);
                }

                return _replaceImageCommand;
            }
        }
        private RelayCommand<ObservableCollection<Models.DicomItem>> _deleteItemHeaderItemCommand;
        public ICommand DeleteHeaderItemCommand
        {
            get
            {
                if (_deleteItemHeaderItemCommand == null)
                {
                    _deleteItemHeaderItemCommand = new RelayCommand<ObservableCollection<Models.DicomItem>>(DeleteHeaderItems);
                }

                return _deleteItemHeaderItemCommand;
            }
        }

        private void DeleteHeaderItems(ObservableCollection<Models.DicomItem> dicomItems)
        {
            DeleteItems(dicomItems);
            DicomItemsView?.Refresh();
        }

        private RelayCommand<ObservableCollection<Models.DicomItem>> _deleteImageAttributesCommand;
        public ICommand DeleteImageAttributesCommand
        {
            get
            {
                if (_deleteImageAttributesCommand == null)
                {
                    _deleteImageAttributesCommand = new RelayCommand<ObservableCollection<Models.DicomItem>>(DeleteImageAttributes);
                }

                return _deleteImageAttributesCommand;
            }
        }
        private void DeleteImageAttributes(ObservableCollection<Models.DicomItem> dicomItems)
        {
            DeleteItems(dicomItems);
            ImageAttributesView?.Refresh();
        }
        private RelayCommand<ObservableCollection<Models.DicomItem>> _deleteSequenceItemCommand;

        public ICommand DeleteSequenceItemCommand
        {
            get
            {
                if (_deleteSequenceItemCommand == null)
                {
                    _deleteSequenceItemCommand = new RelayCommand<ObservableCollection<Models.DicomItem>>(DeleteSequenceItems);
                }

                return _deleteSequenceItemCommand;
            }
        }
        private void DeleteSequenceItems(ObservableCollection<Models.DicomItem> dicomItems)
        {
            DeleteItems(dicomItems);
            SequenceItemsView?.Refresh();
        }
        public string SearchText { get; private set; }
        public string ImageAttributesSearchText { get; private set; }
        public string SequenceItemsSearchText { get; private set; }
        #endregion
        #region internal
        internal void SetSelectedSequence(TreeViewItem selectedTreeViewItem)
        {
            if (_sequenceTable.ContainsKey((DicomTag)selectedTreeViewItem?.Tag))
            {
                SelectedSequenceItems = _sequenceTable[(DicomTag)selectedTreeViewItem.Tag];
                SequenceItemsView = CollectionViewSource.GetDefaultView(SelectedSequenceItems);
                SequenceItemsView.Filter = SequenceItemsSearchFilter;
            }
        }
        internal async Task SaveDicomFileAsync()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "Save DICOM File",
                Filter = "DICOM Files|*.dcm"
            };

            if (true == saveFileDialog.ShowDialog())
            {
                await Application.Current?.Dispatcher.InvokeAsync(() =>
                _dicomDataService.SaveDicomFileAsync(saveFileDialog.FileName));
            }
        }
        #endregion
        #region private
        private async void OpenDicomFile()
        {
            var openFileDlg = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Open DICOM File",
                Filter = "All Files |*.*|DICOM File |*.dcm"
            };

            var result = openFileDlg.ShowDialog();
            if (result == true)
            {
                await Application.Current?.Dispatcher.InvokeAsync(() =>
                _dicomDataService.LoadDicomFileAsync(openFileDlg.FileName));
            }
        }
        private void OnImageReplaced(object sender, DicomFileStateEventArgs e)
        {
            //MessageBox.Show($"Pixel data updated with the image {e.FileName}", "Successfully updated pixel data!",
            //    MessageBoxButton.OK, MessageBoxImage.Information);
            IsDirty = true;
        }

        private void OnImageReplaceFailed(object sender, DicomFileStateEventArgs e)
        {
            MessageBox.Show($"Failed to replace image: {e.Exception.Message}","Failed to update pixel data!",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnFileOpened(object sender, DicomFileStateEventArgs e)
        {
            WindowTitle = $"{DicomEditorName} - {e.FileName}";
        }

        private void OnFileSaved(object sender, DicomFileStateEventArgs e)
        {
            WindowTitle = $"{DicomEditorName} - {e.FileName}";
        }

        private void OnDicomItemRead(object sender, DicomItemReadEventArgs e)
        {
            if (e.DicomItemType != DicomItemType.PixelData)
            {
                var item = new Models.DicomItem()
                {
                    Dataset = e.DicomDataset,
                    DicomTag = e.DicomItem.Tag,
                    DicomVR = e.DicomItem.ValueRepresentation,
                    Description = e.DicomItem.ToString(),
                    Value = GetValue(e.DicomItem.Tag, e.DicomItem.ValueRepresentation, e.DicomDataset),
                    SourceDicomItem = e.DicomItem
                };
                item.PropertyChanged += Item_PropertyChanged;
                switch (e.DicomItemType)
                {
                    case DicomItemType.Normal: DicomItems.Add(item); break;
                    case DicomItemType.ImageAttribute: ImageAttributes.Add(item); break;
                    case DicomItemType.SequenceItem: CurrentSequences.Add(e.DicomDataset.GetSequence(item.DicomTag)); break;
                    default:
                        break;
                }
            }

            if (e.DicomItemType == DicomItemType.PixelData)
            {
                if (!LoadPixelData(e.DicomDataset))
                {
                    DicomImage = null;
                }
            }
        }

        private void OnFileOpenFailed(object sender, DicomFileStateEventArgs e)
        {
            MessageBox.Show($"{e.FileName} failed to open", "DICOM file loading failed!",
                    MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void OnFileSaveFailed(object sender, DicomFileStateEventArgs e)
        {
            MessageBox.Show($"Cannot save image: {e.Exception.Message}", "Image saving failed!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDicomDatasetLoadStarted(object sender, DicomDatasetLoadStartedEventArgs e)
        {
            ClearDataModels();
        }

        private void OnDicomDatasetLoadCompleted(object sender, DicomDatasetLoadCompletedArgs e)
        {
            ImageLoaded = true;

            TryLoadSequences(CurrentSequences);
            CurrentSequences.Clear();

            DicomItemsView = CollectionViewSource.GetDefaultView(DicomItems);
            DicomItemsView.Filter = HeaderSearchFilter;

            ImageAttributesView = CollectionViewSource.GetDefaultView(ImageAttributes);
            ImageAttributesView.Filter = ImageAttributesSearchFilter;
        }
        private void ClearDataModels()
        {
            DicomItems?.Clear();
            ImageAttributes?.Clear();
            Sequences?.Clear();
        }
        private void TryLoadSequences(List<DicomSequence> sequences)
        {
            if (sequences.Count != 0)
            {
                HeaderRowSpan = 1;
                if (Sequences == null)
                {
                    Sequences = new ObservableCollection<TreeViewItem>();
                }
                LoadSequences(sequences);
            }
            else
            {
                HeaderRowSpan = 3;
                Sequences = null;
                SelectedSequenceItems = null;
                _sequenceTable?.Clear();
            }
        }
        private bool LoadPixelData(DicomDataset dataset)
        {
            try
            {
                var dicomImage = new DicomImage(dataset, 0);
                DicomImage = dicomImage.RenderImage().As<WriteableBitmap>();

                return true;
            }
            catch (Exception ex)
            {
                DicomImage = null;
                MessageBox.Show($"Cannot load image: {ex.Message}", "Image loading failed!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private bool HeaderSearchFilter(object obj)
        {
            return FilterBySearchText(SearchText, obj);
        }
        private bool ImageAttributesSearchFilter(object obj)
        {
            return FilterBySearchText(ImageAttributesSearchText, obj);
        }
        private bool SequenceItemsSearchFilter(object obj)
        {
            return FilterBySearchText(SequenceItemsSearchText, obj);
        }
        private bool FilterBySearchText(string searchText, object obj)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            var item = obj as Models.DicomItem;

            var valueMatches = item.Value != null && item.Value.ToString().ContainsCaseInsensitive(searchText);
            return item.DicomTag.ToString().ContainsCaseInsensitive(searchText) || valueMatches || item.Description.ContainsCaseInsensitive(searchText);
        }
        private void LoadSequences(List<DicomSequence> sequences)
        {
            TreeViewItem firstItem = null;
            foreach (var sequence in sequences)
            {
                var treeItem = new TreeViewItem();
                if (firstItem == null)
                {
                    firstItem = treeItem;
                }
                LoadSequence(treeItem, sequence);
            }
            if (firstItem != null)
            {
                firstItem.IsSelected = true;
            }
        }
        private void LoadSequence(TreeViewItem treeViewItem, DicomSequence sequence)
        {
            foreach (var sqItem in sequence.Items)
            {
                foreach (var childItem in sqItem)
                {
                    if (childItem.ValueRepresentation == DicomVR.SQ)
                    {
                        LoadSequence(treeViewItem, sqItem.GetSequence(childItem.Tag));
                    }

                    if (!_sequenceTable.ContainsKey(sequence.Tag))
                    {
                        _sequenceTable[sequence.Tag] = new ObservableCollection<Models.DicomItem>();
                    }

                    var sequenceItem = new Models.DicomItem()
                    {
                        Dataset = sqItem,
                        SourceDicomItem = childItem,
                        DicomTag = childItem.Tag,
                        DicomVR = childItem.ValueRepresentation,
                        Description = childItem.ToString(),
                        Value = GetValue(childItem.Tag, childItem.ValueRepresentation, sqItem)
                    };
                    sequenceItem.PropertyChanged += Item_PropertyChanged;
                    _sequenceTable[sequence.Tag].Add(sequenceItem);
                }
            }
            treeViewItem.Header = sequence.ToString();
            treeViewItem.Tag = sequence.Tag;
            Sequences.Add(treeViewItem);
        }
        private async void SaveDicomFileCommandExecutorAsync()
        {
            await SaveDicomFileAsync();
            IsDirty = false;
        }
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                var dicomItem = sender as Models.DicomItem;
                var actualDicomItem = dicomItem?.SourceDicomItem;
                SetValue(dicomItem.Dataset, actualDicomItem, dicomItem?.Value);
            }
        }
        private void SetValue(DicomDataset dataset, DicomItem actualDicomItem, object value)
        {
            try
            {
                switch (actualDicomItem.ValueRepresentation.ToString())
                {
                    case "AE":
                        dataset.AddOrUpdate(new DicomApplicationEntity(actualDicomItem.Tag, value.ToString())); break;
                    case "AS":
                        dataset.AddOrUpdate(new DicomAgeString(actualDicomItem.Tag, value.ToString())); break;
                    case "CS":
                        dataset.AddOrUpdate(new DicomCodeString(actualDicomItem.Tag, value.ToString())); break;
                    case "DS":
                        dataset.AddOrUpdate(new DicomDecimalString(actualDicomItem.Tag, decimal.Parse(value.ToString()))); break;
                    case "IS":
                        dataset.AddOrUpdate(new DicomIntegerString(actualDicomItem.Tag, value.ToString())); break;
                    case "LO":
                        dataset.AddOrUpdate(new DicomLongString(actualDicomItem.Tag, value.ToString())); break;
                    case "LT":
                        dataset.AddOrUpdate(new DicomLongText(actualDicomItem.Tag, value.ToString())); break;
                    case "PN":
                        dataset.AddOrUpdate(new DicomPersonName(actualDicomItem.Tag, value.ToString())); break;
                    case "SH":
                        dataset.AddOrUpdate(new DicomShortString(actualDicomItem.Tag, value.ToString())); break;
                    case "ST":
                        dataset.AddOrUpdate(new DicomShortText(actualDicomItem.Tag, value.ToString())); break;
                    case "UI":
                        dataset.AddOrUpdate(new DicomUniqueIdentifier(actualDicomItem.Tag, value.ToString())); break;
                    case "UT":
                        dataset.AddOrUpdate(new DicomUnlimitedText(actualDicomItem.Tag, value.ToString())); break;
                    case "DA":
                        dataset.AddOrUpdate(new DicomDate(actualDicomItem.Tag, value.ToString())); break;
                    case "DT":
                        dataset.AddOrUpdate(new DicomDateTime(actualDicomItem.Tag, value.ToString())); break;
                    case "TM":
                        dataset.AddOrUpdate(new DicomTime(actualDicomItem.Tag, value.ToString())); break;
                    case "AT":
                        dataset.AddOrUpdate(new DicomAttributeTag(actualDicomItem.Tag, DicomTag.Parse(value.ToString()))); break;
                    case "FD":
                        dataset.AddOrUpdate(new DicomFloatingPointDouble(actualDicomItem.Tag, double.Parse(value.ToString()))); break;
                    case "FL":
                        dataset.AddOrUpdate(new DicomFloatingPointSingle(actualDicomItem.Tag, float.Parse(value.ToString()))); break;
                    case "SL":
                        dataset.AddOrUpdate(new DicomSignedLong(actualDicomItem.Tag, int.Parse(value.ToString()))); break;
                    case "SS":
                        dataset.AddOrUpdate(new DicomSignedShort(actualDicomItem.Tag, short.Parse(value.ToString()))); break;
                    case "SV":
                        dataset.AddOrUpdate(new DicomSignedVeryLong(actualDicomItem.Tag, long.Parse(value.ToString()))); break;
                    case "UC":
                        dataset.AddOrUpdate(new DicomUnlimitedCharacters(actualDicomItem.Tag, value.ToString())); break;
                    case "UL":
                        dataset.AddOrUpdate(new DicomUnsignedLong(actualDicomItem.Tag, uint.Parse(value.ToString()))); break;
                    case "UR":
                        dataset.AddOrUpdate(new DicomUniversalResource(actualDicomItem.Tag, value.ToString())); break;
                    case "US":
                        dataset.AddOrUpdate(new DicomUnsignedShort(actualDicomItem.Tag, ushort.Parse(value.ToString()))); break;
                    case "UV":
                        dataset.AddOrUpdate(new DicomUnsignedVeryLong(actualDicomItem.Tag, ulong.Parse(value.ToString()))); break;
                }
                IsDirty = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Failed to set dicom tag value!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private object GetValue(DicomTag dicomTag, DicomVR dicomVR, DicomDataset dataset)
        {
            switch (dicomVR.ToString())
            {
                case "AE":
                case "AS":
                case "CS":
                case "DS":
                case "IS":
                case "LO":
                case "LT":
                case "PN":
                case "SH":
                case "ST":
                case "UI":
                case "UT":
                case "DA":
                case "DT":
                case "TM":
                case "AT":
                case "FD":
                case "FL":
                case "SL":
                case "SS":
                case "SV":
                case "UC":
                case "UL":
                case "UR":
                case "US":
                case "UV": return dataset.GetString(dicomTag);
            }
            return null;
        }
        private async void ReplaceImage()
        {
            var imageSelector = new ImageSelectorDialog();
            if (true == imageSelector.ShowDialog())
            {
                var selectedImageInfo = imageSelector.GetSelectedImageInfo();
                await Application.Current?.Dispatcher.InvokeAsync(() =>
                _dicomDataService.ReplacePixelData(selectedImageInfo.ImagePath, selectedImageInfo));
            }
        }
        private void DeleteItems(ObservableCollection<Models.DicomItem> dicomItems)
        {
            var selectedItems = dicomItems.Where(item => item.IsSelected);
            foreach (var item in selectedItems.ToList())
            {
                item.Dataset.Remove(item.DicomTag);
                dicomItems.Remove(item);
            }
        }
        #endregion
    }
}
