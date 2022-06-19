using System;
using System.Windows;

namespace FrisbeeDicomEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //MessageBox.Show($"Dispatcher unhandled exception: {e.Exception}","Unhandled exception!");
            e.Handled = true;

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Unexpected error: {e.ExceptionObject}", "Unexpected error!",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
