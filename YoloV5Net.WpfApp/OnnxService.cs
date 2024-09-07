using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models;
using static System.Formats.Asn1.AsnWriter;
namespace YoloV5Net.WpfApp;

public class OnnxService
{
    private readonly string _modelPath;

    public OnnxService(string modelPath)
    {
        _modelPath = modelPath;
    }

    public async Task RunInference(string imagePath,string outputImagePath)
    {
        using var image = await Image.LoadAsync<Rgba32>(imagePath);
        {
            using var scorer = new YoloScorer<YoloV5YB>(_modelPath);
            {
                var predictions = scorer.Predict(image);

                var font = new Font(new FontCollection().Add("C:/Windows/Fonts/consola.ttf"), 16);


                foreach (var prediction in predictions) // 绘制预测结果`
                {
                    var score = Math.Round(prediction.Score, 2);
              
                    var (x, y) = (prediction.Rectangle.Left - 3, prediction.Rectangle.Top - 23);


                    image.Mutate(a => a.DrawText($"{prediction.Label} ({score})",
                        font, Color.Red, new PointF(x, y)));
                    image.Mutate(a => a.DrawPolygon(new SolidPen(Color.Red, 1),
                          new PointF(prediction.Rectangle.Left, prediction.Rectangle.Top),
                          new PointF(prediction.Rectangle.Right, prediction.Rectangle.Top),
                          new PointF(prediction.Rectangle.Right, prediction.Rectangle.Bottom),
                          new PointF(prediction.Rectangle.Left, prediction.Rectangle.Bottom)
                      ));
                }
                await image.SaveAsync(outputImagePath); 
            }
        }
    }

}

