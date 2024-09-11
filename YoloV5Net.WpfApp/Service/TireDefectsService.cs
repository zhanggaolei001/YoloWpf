using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models.Abstract;
using YoloV5Net.WpfApp.Model;

namespace YoloV5Net.WpfApp.Service;

public interface IDefectService
{
    Task<List<YoloYbResult>> Detect(string imagePath);
}
public class DefectsService<T>: IDefectService where T : YoloModel
{
    private readonly string _modelPath;

    public DefectsService(string modelPath)
    {
        _modelPath = modelPath;
    }
    public async Task<List<YoloYbResult>> Detect(string imagePath)
    {
        using var image = await Image.LoadAsync<Rgba32>(imagePath);
        {
            using var scorer = new YoloScorer<T>(_modelPath);
            {
                var predictions = scorer.Predict(image);
                
                return predictions;
            }
        }
    }
}
public class TireDefectsService: DefectsService<YoloV5TierDefects>
{
    public TireDefectsService(string modelPath) : base(modelPath)
    {
    }
}