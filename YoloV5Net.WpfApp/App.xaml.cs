using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace YoloV5Net.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //全局未捕获的异常进行捕获处理，弹出警告框即可
        public App()
        { 
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        } 
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
    }

}
