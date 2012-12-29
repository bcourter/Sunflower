using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


// totally incomplete right now
namespace Sunflower.Geometry {
    public static class Interpolation {
        public static double Interpolate(double a, double b, double t) {
            return a + (b - a) * t;
        }

        public static double InterpolateRange(double a, double b, double t) {
            return (t - a) / (b - a);
        }

        public static double Clamp(double minInput, double maxInput, double value, double minOutput, double maxOutput) {
            double ratio = (value - minInput) / (maxInput - minInput);
            ratio = Math.Max(0, ratio);
            ratio = Math.Min(1, ratio);
            return minOutput + (maxOutput - minOutput) * ratio;
        }
    }
}
