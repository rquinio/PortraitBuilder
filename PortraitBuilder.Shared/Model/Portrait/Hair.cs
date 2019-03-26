using SkiaSharp;

namespace PortraitBuilder.Model.Portrait
{
    public readonly struct Hair
    {
        public readonly SKColor Dark;
        public readonly SKColor Base;
        public readonly SKColor Highlight;

        public Hair(SKColor dark, SKColor @base, SKColor highlight)
        {
            Dark = dark;
            Base = @base;
            Highlight = highlight;
        }
    }
}