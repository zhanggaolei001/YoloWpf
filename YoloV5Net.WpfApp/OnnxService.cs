using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Yolov5Net.Scorer; 
namespace YoloV5Net.WpfApp;

public class OnnxService
{
    private readonly string _modelPath;

    public OnnxService(string modelPath)
    {
        _modelPath = modelPath;
    }

    public async Task<List<YoloYBResult>> RunInference(string imagePath )
    {
        using var image = await Image.LoadAsync<Rgba32>(imagePath);
        {
            
            using var scorer = new YoloScorer<YoloV5FireExtinguisher>(_modelPath);
            {
                var predictions = scorer.Predict(image); 
                return predictions;
            }
        }
    }

}

