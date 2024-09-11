using System.Windows;
using System.Windows.Media.Imaging;

namespace YoloV5Net.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void DropArea_Drop(object sender, DragEventArgs e)
        {
            var s = sender as FrameworkElement;
            var vm = s.DataContext as TabItemViewModel;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string filePath = files[0];
                vm.ImagePath = filePath;
                vm.Detect();
            }
        }
    }
}