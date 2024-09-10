using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Yolov5Net.Scorer;
using YoloV5Net.WpfApp.Model;
namespace YoloV5Net.WpfApp.Service;

public class FireExtinguisherDetectService:DefectsService<YoloV5FireExtinguisher>
{
    public FireExtinguisherDetectService(string modelPath) : base(modelPath)
    {
    }
}