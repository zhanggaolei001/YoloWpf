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
        private void FireExtinguisherDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string filePath = files[0];
                var viewModel = DataContext as MainViewModel;
                viewModel.TabItemlViewModel.ImagePath = filePath;
                viewModel.TabItemlViewModel.DetectCommand.Execute(filePath); 
            }
        }
    }
}