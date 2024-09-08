using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Yolov5Net.Scorer;

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
            var r = await _onnxService.RunInference(ImagePath);
            PredictResults.Clear();
            foreach (var item in r)
            {
                PredictResults.Add(item);
            }
            OriginalHeight = Box.ParentHeight;
            OriginalWidth = Box.ParentWidth;
            windowHeight = OriginalHeight + 30;
            _currentHeight = Box.ParentHeight; 
            _currentWidth = Box.ParentWidth;
            double scale = 1;
            if (CurrentHeight > 800)
            {
                windowHeight = 830;
                _currentHeight = 800; 
                scale = OriginalHeight / CurrentHeight;
                _currentWidth = OriginalWidth / scale;
            }     OnPropertyChanged(nameof(CurrentWidth));  
            OnPropertyChanged(nameof(WindowHeight));
          
            OnPropertyChanged(nameof(CurrentWidth));   
            UpdateXW();
            OnPropertyChanged(nameof(CurrentHeight));     
            UpdateHY();
        }
    }
    public ObservableCollection<YoloYBResult> PredictResults { get; } = new ObservableCollection<YoloYBResult>();
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

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        return false;
    }

    private double _currentWidth = 1000;

    public double CurrentWidth
    {
        get => _currentWidth;
        set
        {
            SetProperty(ref _currentWidth, value);
          
        }
    }

    private void UpdateXW()
    {
        ScaleX = _currentWidth / OriginalWidth;
        foreach (var item in PredictResults)
        {
            item.Rectangle.X = (float)(item.Rectangle.X * ScaleX);
            item.Rectangle.Width = (float)(item.Rectangle.Width * ScaleX);
        }
    }
    public double WindowHeight
    {
        get => windowHeight; set
        {
            windowHeight = value;
            OnPropertyChanged();
        }
    }

    private double _currentHeight = 800;

    public double CurrentHeight
    {
        get => _currentHeight;
        set
        {
            SetProperty(ref _currentHeight, value);

        }
    }

    private void UpdateHY()
    {
        ScaleY = _currentHeight / OriginalHeight;
        foreach (var item in PredictResults)
        {
            item.Rectangle.Y = (float)(item.Rectangle.Y * ScaleY);
            item.Rectangle.Height = (float)(item.Rectangle.Height * ScaleY);
        }
    }

    private double scaleX;

    public double ScaleX { get => scaleX; set => SetProperty(ref scaleX, value); }

    private double scaleY;
    private double windowHeight = 500;

    public double ScaleY { get => scaleY; set => SetProperty(ref scaleY, value); }
    public double OriginalWidth { get; set; }
    public double OriginalHeight { get; set; }
}