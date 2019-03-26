using SkiaSharp;

namespace PortraitBuilder.Model.Portrait
{
    public static class ColorHelper
    {
        /// <summary>
        /// Linear interpolation function.
        /// </summary>
        /// <param name="l">Left hand colour.</param>
        /// <param name="r">Right hand colour.</param>
        /// <param name="inter">Interval of the interpolation. Uses a range of 0-255</param>
        /// <returns>Returns an interpolation colour.</returns>
        public static SKColor Lerp(SKColor l, SKColor r, double inter)
        {
            var red = (byte)(l.Red + ((inter / 255d) * (r.Red - l.Red)));
            var green = (byte)(l.Green + ((inter / 255d) * (r.Green - l.Green)));
            var blue = (byte)(l.Blue + ((inter / 255d) * (r.Blue - l.Blue)));

            return new SKColor(red, green, blue);
        }

        /// <summary>
        /// Clamps an input value to a range of 0-255
        /// </summary>
        public static double Clamp(double i)
        {
            if (i > 255) return 255;
            if (i < 0) return 0;
            return i;
        }
    }
}
