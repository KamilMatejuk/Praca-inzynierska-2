using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Colors {

    public static float RgbToGrayscale(Color color) {
        return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
    }

    public static float GrayscaleToBW(float grayscale, float threshold = 0.5f) {
        return (grayscale > threshold) ? 1f : 0f;
    }
}
