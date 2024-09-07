using System.Globalization;
using System.Windows.Data;

namespace YoloV5Net.WpfApp;

public class ScaleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        float originalSize = (float)value;
        float currentSize = 224; // 假设当前大小是224
        return originalSize / currentSize;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
