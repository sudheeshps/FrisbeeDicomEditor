using HandyControl.Controls;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace FrisbeeDicomEditor.Views
{
    /// <summary>
    /// Interaction logic for DicomTagView.xaml
    /// </summary>
    public partial class DicomTagView : UserControl
    {
        public DicomTagView()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty HeaderProperty = 
            DependencyProperty.Register("Header", typeof(string), typeof(DicomTagView), 
                new PropertyMetadata(default(string)));
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty DicomAttributesProperty =
            DependencyProperty.Register("DicomAttributes", typeof(IEnumerable), typeof(DicomTagView),
                new PropertyMetadata(default(IEnumerable)));
        public IEnumerable DicomAttributes
        {
            get => (IEnumerable)GetValue(DicomAttributesProperty);
            set => SetValue(DicomAttributesProperty, value);
        }

        public static readonly DependencyProperty SearchTextChangedCommandProperty = 
            DependencyProperty.Register("SearchTextChangedCommand", typeof(ICommand), typeof(DicomTagView),
                new PropertyMetadata(default(ICommand)));
        public ICommand SearchTextChangedCommand
        {
            get => (ICommand)GetValue(SearchTextChangedCommandProperty);
            set => SetValue(SearchTextChangedCommandProperty, value);
        }

        public static readonly DependencyProperty DeleteDicomItemCommandProperty =
            DependencyProperty.Register("DeleteDicomItemCommand", typeof(ICommand), typeof(DicomTagView),
                new PropertyMetadata(default(ICommand)));
        public ICommand DeleteDicomItemCommand
        {
            get => (ICommand)GetValue(DeleteDicomItemCommandProperty);
            set => SetValue(DeleteDicomItemCommandProperty, value);
        }

        public static readonly DependencyProperty DeleteDicomItemCommandParamProperty =
            DependencyProperty.Register("DeleteDicomItemCommandParam", typeof(object), typeof(DicomTagView),
                new PropertyMetadata(default(object)));
        public object DeleteDicomItemCommandParam
        {
            get => GetValue(DeleteDicomItemCommandParamProperty);
            set => SetValue(DeleteDicomItemCommandParamProperty, value);
        }
        public static readonly DependencyProperty ValueColumnWidthProperty =
            DependencyProperty.Register("ValueColumnWidth", typeof(int), typeof(DicomTagView),
                new PropertyMetadata(default(int)));
        public int ValueColumnWidth
        {
            get => (int)GetValue(ValueColumnWidthProperty);
            set => SetValue(ValueColumnWidthProperty, value);
        }
    }
}
