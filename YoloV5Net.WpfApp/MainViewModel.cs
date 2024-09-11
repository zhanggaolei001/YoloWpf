using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Yolov5Net.Scorer;
using YoloV5Net.WpfApp.Service;

namespace YoloV5Net.WpfApp;
public class MainViewModel : INotifyPropertyChanged
{
  Dictionary<int,string> fire=  new Dictionary<int, string>()
    {
       {0, "bxs"},
       {1, "red" },
       {2, "green" },
       {3, "none" }, //3 
       {4, "damaged" }
    };
    Dictionary<int, string> tire = new Dictionary<int, string>()
    {
       {0, "a1"},
       {1, "o1" },
       {2, "c1" }
    };
    public MainViewModel()
    {
        TabItemlViewModel  = new(new FireExtinguisherDetectService("epoch_1000.onnx"),fire);
        TabItem2ViewModel  = new(new TireDefectsService("tire.onnx"),tire);
    }
    public TabItemViewModel TabItemlViewModel { get; set; }
    public TabItemViewModel TabItem2ViewModel { get; set; } 

    public string Tab1Icon { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img1.jpg");
    public string Tab2Icon { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img2.jpg");
    public string Tab3Icon { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img3.jpg");

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

public class TabItemViewModel : INotifyPropertyChanged
{
    public TabItemViewModel(IDefectService defectService, Dictionary<int, string>  labelDic)
    {
        _detectService = defectService;
        LabelTextDic= labelDic;
    }
 
    public   Dictionary<int, string> LabelTextDic { get; set; }
    public ICommand DetectCommand => new RelayCommand(default, CanExecuteProcessImage);
    public async void Detect()
    {
        if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            var r = await _detectService.Detect(ImagePath);
            PredictResults.Clear();
            foreach (var item in r)
            {
                item.LabelText = LabelTextDic[item.Label];  
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
            }
            OnPropertyChanged(nameof(CurrentWidth));
            OnPropertyChanged(nameof(WindowHeight));
            OnPropertyChanged(nameof(CurrentWidth));
            UpdateXW();
            OnPropertyChanged(nameof(CurrentHeight));
            UpdateHY();
        }
    }
    private bool CanExecuteProcessImage()
    {
        return !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath);
    }




    private readonly IDefectService _detectService;


    public string ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<YoloYbResult> PredictResults { get; } = new ObservableCollection<YoloYbResult>();

    private double _currentWidth = 1000;

    public double CurrentWidth
    {
        get => _currentWidth;
        set
        {
            SetField(ref _currentWidth, value);

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
            SetField(ref _currentHeight, value);

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

    public double ScaleX { get => scaleX; set => SetField(ref scaleX, value); }

    private double scaleY;
    private double windowHeight = 500;
    private string _imagePath;

    public double ScaleY { get => scaleY; set => SetField(ref scaleY, value); }
    public double OriginalWidth { get; set; }
    public double OriginalHeight { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}