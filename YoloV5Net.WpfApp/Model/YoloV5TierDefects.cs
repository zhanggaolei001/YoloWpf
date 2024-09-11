using Yolov5Net.Scorer.Models.Abstract;

namespace YoloV5Net.WpfApp.Model;

public record YoloV5TierDefects() : YoloModel(
    640,
    640,
    3,

    1,

    new[] { 8, 16, 32, 64 },

    new[]
    {
        new[] { new[] { 016, 017 }, new[] { 027, 027 }, new[] { 027, 047 } },
        new[] { new[] { 038, 041 }, new[] { 047, 46 }, new[] { 55, 62 } },
        new[] { new[] { 83, 76 }, new[] { 114, 110 }, new[] { 162, 164 } }
    },

    new[] { 80, 40, 20 },

    0.20f,
    0.25f,
    0.45f,

    new[] { "num_dets", "boxes", "scores", "labels" },

    new()
    {
        new(0, "a1"),//0
        new(1, "o1"),
        new(2, "c1"), 
    },
    true
);