using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace YoloV5Net.WpfApp;

public class MainViewModel : INotifyPropertyChanged
{
    private string _imagePath;
    public string ImagePath
    {
        get { return _imagePath; }
        set { SetField(ref _imagePath, value); }
    }

    private string _resultImage;
    public string ResultImagePath
    {
        get { return _resultImage; }
        set { SetField(ref _resultImage, value); }
    }

    public ICommand ProcessImageCommand { get; }

    private readonly OnnxService _onnxService;

    public MainViewModel()
    {
        _onnxService = new OnnxService("epoch_1000.onnx");
        ProcessImageCommand = new RelayCommand(ExecuteProcessImage, CanExecuteProcessImage);
    }

    private async void ExecuteProcessImage(object parameter)
    {
        if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            var outputPath = "result.jpg";
            FileInfo fi = new FileInfo(outputPath);
            Console.WriteLine(fi.FullName);
            await _onnxService.RunInference(ImagePath, fi.FullName);
            ResultImagePath = fi.FullName;
        }
    }

    private bool CanExecuteProcessImage(object parameter)
    {
        return !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath);
    }



    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}