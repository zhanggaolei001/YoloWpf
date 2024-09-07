using Yolov5Net.Scorer.Models.Abstract;

namespace YoloV5Net.WpfApp;

public record YoloV5YB() : YoloModel(
    224,
    224,
    3,

    10,

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

    new[] { "num_dets", "boxes","scores","labels" },

    new()
    {
        new(0, "bxs"),//0
        new(1, "red"),
        new(2, "green"), 
        new(3, "none"), //3 
        new(4, "damaged"), 
    }, 
    true
);