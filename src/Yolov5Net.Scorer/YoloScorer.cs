﻿using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Yolov5Net.Scorer.Extensions;
using Yolov5Net.Scorer.Models.Abstract;
using static System.Formats.Asn1.AsnWriter;

namespace Yolov5Net.Scorer;
 
public class YoloYbResult
{

    public YoloYbResult(Box box, float score, int label, string labelText)
    {
        Rectangle = box;
        Score = score;
        Label = label;
        LabelText = labelText;
    }
    public Box Rectangle { get; set; }
    public double Score { get; set; }
    public int Label { get; set; } 
    public string LabelText { get; set; } 
}
public class Box : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private float originalWidth;
    private float originalHeight;
    private float left;
    private float top;
    private float width;
    private float height;
    public static float ParentWidth { get; set; }
    public static float ParentHeight { get; set; }
    public Box(float[] arr, float originalWidth, float originalHeight)
    {

        // 确保原始尺寸不为零，避免除以零的错误
        if (originalWidth == 0 || originalHeight == 0)
        {
            throw new ArgumentException("Original width and height must be greater than zero.");
        }

        // 计算缩放比例
        float scaleWidth = originalWidth / 224;
        float scaleHeight = originalHeight / 224;

        // 应用缩放比例
        // Left 和 Top 直接是左上角的坐标
        X = arr[0] * scaleWidth;
        Y = arr[1] * scaleHeight;
        // Width 和 Height 是通过右下角坐标减去左上角坐标计算得出
        Width = (arr[2] - arr[0]) * scaleWidth;
        Height = (arr[3] - arr[1]) * scaleHeight;

        // 保存原始图像尺寸
        this.originalWidth = originalWidth;
        this.originalHeight = originalHeight;
    }
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public float X { get => left; set => SetField(ref left, value); }
    public float Y { get => top; set => SetField(ref top, value); }
    public float Width { get => width; set => SetField(ref width, value); }
    public float Height { get => height; set => SetField(ref height, value); }

    // 可以添加方法来获取原始图像尺寸
    public float GetOriginalWidth()
    {
        return originalWidth;
    }

    public float GetOriginalHeight()
    {
        return originalHeight;
    }
}/// <summary>
/// Yolov5 scorer.
/// </summary>
public class YoloScorer<T> : IDisposable where T : YoloModel
{
    private readonly T _model;

    private readonly InferenceSession _inferenceSession;

    /// <summary>
    /// Outputs value between 0 and 1.
    /// </summary>
    private static float Sigmoid(float value)
    {
        return 1 / (1 + (float)Math.Exp(-value));
    }

    /// <summary>
    /// Converts xywh bbox format to xyxy.
    /// </summary>
    private float[] Xywh2xyxy(float[] source)
    {
        var result = new float[4];

        result[0] = source[0] - (source[2] / 2f);
        result[1] = source[1] - (source[3] / 2f);
        result[2] = source[0] + (source[2] / 2f);
        result[3] = source[1] + (source[3] / 2f);

        return result;
    }

    /// <summary>
    /// Returns value clamped to the inclusive range of min and max.
    /// </summary>
    public float Clamp(float value, float min, float max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }

    /// <summary>
    /// Extracts pixels into tensor for net input.
    /// </summary>
    private Tensor<float> ExtractPixels(Image<Rgba32> image)
    {
        var tensor = new DenseTensor<float>(new[] { 1, 3, _model.Height, _model.Width });

        Parallel.For(0, image.Height, y =>
        {
            Parallel.For(0, image.Width, x =>
            {
                tensor[0, 0, y, x] = image[x, y].R / 255.0F; // r
                tensor[0, 1, y, x] = image[x, y].G / 255.0F; // g
                tensor[0, 2, y, x] = image[x, y].B / 255.0F; // b
            });
        });

        return tensor;
    }

    /// <summary>
    /// Runs inference session.
    /// </summary>
    private List<YoloYbResult> Inference(Image<Rgba32> image)
    {
        var w = image.Width;
        var h = image.Height;
        Box.ParentHeight = h;
        Box.ParentWidth = w;
        if (image.Width != _model.Width || image.Height != _model.Height)
        {
            image.Mutate(x => x.Resize(_model.Width, _model.Height)); // fit image size to specified input size
        }

        var inputs = new List<NamedOnnxValue> // add image as input
        {
            NamedOnnxValue.CreateFromTensor("images", ExtractPixels(image))
        };

        var result = _inferenceSession.Run(inputs);
        var num_dets = ((result.First(x => x.Name == "num_dets").Value as DenseTensor<long>))[0];
        var boxes = (result.First(x => x.Name == "boxes").Value as DenseTensor<float>).ToArray();
        var scores = (result.First(x => x.Name == "scores").Value as DenseTensor<float>).ToArray();
        var labels = (result.First(x => x.Name == "labels").Value as DenseTensor<int>).ToArray();
        List<YoloYbResult> rs = new List<YoloYbResult>();
        for (int i = 0; i < num_dets; i++)
        {
            var box = new Box(boxes.Skip(i * 4).Take(4).ToArray(), w, h);
            var score = scores.Skip(i * 1).Take(1).ToArray()[0];
            var label = labels.Skip(i * 1).Take(1).ToArray()[0];
            rs.Add(new YoloYbResult(box, score, label,label.ToString()));
        }
        return rs;
        //  YoloYBResult[] outputs = new YoloYBResult[num_dets[0]];

    }

    /// <summary>
    /// Parses net output (detect) to predictions.
    /// </summary>
    private List<YoloPrediction> ParseDetect(DenseTensor<float> output, (int Width, int Height) image)
    {
        var result = new ConcurrentBag<YoloPrediction>();

        var (xGain, yGain) = (_model.Width / (float)image.Width, _model.Height / (float)image.Height); // x, y gains

        var (xPadding, yPadding) = ((_model.Width - (image.Width * xGain)) / 2, (_model.Height - (image.Height * yGain)) / 2); // left, right pads

        Parallel.For(0, (int)output.Length / _model.Dimensions, i =>
        {
            if (output[0, i, 4] <= _model.Confidence) return; // skip low obj_conf results

            Parallel.For(5, _model.Dimensions, j => output[0, i, j] *= output[0, i, 4]);

            Parallel.For(5, _model.Dimensions, k =>
            {
                if (output[0, i, k] <= _model.MulConfidence) return; // skip low mul_conf results

                var xMin = (output[0, i, 0] - (output[0, i, 2] / 2) - xPadding) / xGain; // unpad bbox tlx to original
                var yMin = (output[0, i, 1] - (output[0, i, 3] / 2) - yPadding) / yGain; // unpad bbox tly to original
                var xMax = (output[0, i, 0] + (output[0, i, 2] / 2) - xPadding) / xGain; // unpad bbox brx to original
                var yMax = (output[0, i, 1] + (output[0, i, 3] / 2) - yPadding) / yGain; // unpad bbox bry to original

                xMin = Clamp(xMin, 0, image.Width - 0); // clip bbox tlx to boundaries
                yMin = Clamp(yMin, 0, image.Height - 0); // clip bbox tly to boundaries
                xMax = Clamp(xMax, 0, image.Width - 1); // clip bbox brx to boundaries
                yMax = Clamp(yMax, 0, image.Height - 1); // clip bbox bry to boundaries

                var label = _model.Labels[k - 5];

                var prediction = new YoloPrediction(label, output[0, i, k], new(xMin, yMin, xMax - xMin, yMax - yMin));

                result.Add(prediction);
            });
        });

        return result.ToList();
    }

    /// <summary>
    /// Parses net outputs (sigmoid) to predictions.
    /// </summary>
    private List<YoloPrediction> ParseSigmoid(DenseTensor<float>[] output, (int Width, int Height) image)
    {
        var result = new ConcurrentBag<YoloPrediction>();

        var (xGain, yGain) = (_model.Width / (float)image.Width, _model.Height / (float)image.Height); // x, y gains

        var (xPadding, yPadding) = ((_model.Width - (image.Width * xGain)) / 2, (_model.Height - (image.Height * yGain)) / 2); // left, right pads

        Parallel.For(0, output.Length, i => // iterate model outputs
        {
            var shapes = _model.Shapes[i]; // shapes per output

            Parallel.For(0, _model.Anchors[0].Length, a => // iterate anchors
            {
                Parallel.For(0, shapes, y => // iterate shapes (rows)
                {
                    Parallel.For(0, shapes, x => // iterate shapes (columns)
                    {
                        var offset = ((shapes * shapes * a) + (shapes * y) + x) * _model.Dimensions;

                        var buffer = output[i].Skip(offset).Take(_model.Dimensions).Select(Sigmoid).ToArray();

                        if (buffer[4] <= _model.Confidence) return; // skip low obj_conf results

                        var scores = buffer.Skip(5).Select(b => b * buffer[4]).ToList(); // mul_conf = obj_conf * cls_conf

                        var mulConfidence = scores.Max(); // max confidence score

                        if (mulConfidence <= _model.MulConfidence) return; // skip low mul_conf results

                        var rawX = ((buffer[0] * 2) - 0.5f + x) * _model.Strides[i]; // predicted bbox x (center)
                        var rawY = ((buffer[1] * 2) - 0.5f + y) * _model.Strides[i]; // predicted bbox y (center)

                        var rawW = (float)Math.Pow(buffer[2] * 2, 2) * _model.Anchors[i][a][0]; // predicted bbox w
                        var rawH = (float)Math.Pow(buffer[3] * 2, 2) * _model.Anchors[i][a][1]; // predicted bbox h

                        var xyxy = Xywh2xyxy(new[] { rawX, rawY, rawW, rawH });

                        var xMin = Clamp((xyxy[0] - xPadding) / xGain, 0, image.Width - 0); // unpad, clip tlx
                        var yMin = Clamp((xyxy[1] - yPadding) / yGain, 0, image.Height - 0); // unpad, clip tly
                        var xMax = Clamp((xyxy[2] - xPadding) / xGain, 0, image.Width - 1); // unpad, clip brx
                        var yMax = Clamp((xyxy[3] - yPadding) / yGain, 0, image.Height - 1); // unpad, clip bry

                        var label = _model.Labels[scores.IndexOf(mulConfidence)];

                        var prediction = new YoloPrediction(label, mulConfidence, new(xMin, yMin, xMax - xMin, yMax - yMin));

                        result.Add(prediction);
                    });
                });
            });
        });

        return result.ToList();
    }

    /// <summary>
    /// Parses net outputs (sigmoid or detect layer) to predictions.
    /// </summary>
    private List<YoloPrediction> ParseOutput(DenseTensor<float>[] output, (int Width, int Height) image)
    {
        return _model.UseDetect ? ParseDetect(output[0], image) : ParseSigmoid(output, image);
    }

    /// <summary>
    /// Removes overlapped duplicates (nms).
    /// </summary>
    private List<YoloPrediction> Suppress(List<YoloPrediction> items)
    {
        var result = new List<YoloPrediction>(items);

        foreach (var item in items) // iterate every prediction
        {
            foreach (var current in result.ToList().Where(current => current != item)) // make a copy for each iteration
            {
                var (rect1, rect2) = (item.Rectangle, current.Rectangle);

                var intersection = RectangleF.Intersect(rect1, rect2);

                var intArea = intersection.Area(); // intersection area
                var unionArea = rect1.Area() + rect2.Area() - intArea; // union area
                var overlap = intArea / unionArea; // overlap ratio

                if (overlap >= _model.Overlap)
                {
                    if (item.Score >= current.Score)
                    {
                        result.Remove(current);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Runs object detection.
    /// </summary>
    public List<YoloYbResult> Predict(Image<Rgba32> image)
    {
        return Inference(image.Clone());
    }

    /// <summary>
    /// Creates new instance of YoloScorer.
    /// </summary>
    public YoloScorer()
    {
        _model = Activator.CreateInstance<T>();
    }

    /// <summary>
    /// Creates new instance of YoloScorer with weights path and options.
    /// </summary>
    public YoloScorer(string weights, SessionOptions opts = null) : this()
    {
        _inferenceSession = new InferenceSession(File.ReadAllBytes(weights), opts ?? new SessionOptions());
    }

    /// <summary>
    /// Creates new instance of YoloScorer with weights stream and options.
    /// </summary>
    public YoloScorer(Stream weights, SessionOptions opts = null) : this()
    {
        using (var reader = new BinaryReader(weights))
        {
            _inferenceSession = new InferenceSession(reader.ReadBytes((int)weights.Length), opts ?? new SessionOptions());
        }
    }

    /// <summary>
    /// Creates new instance of YoloScorer with weights bytes and options.
    /// </summary>
    public YoloScorer(byte[] weights, SessionOptions opts = null) : this()
    {
        _inferenceSession = new InferenceSession(weights, opts ?? new SessionOptions());
    }

    /// <summary>
    /// Disposes YoloScorer instance.
    /// </summary>
    public void Dispose()
    {
        _inferenceSession.Dispose();
    }
}
