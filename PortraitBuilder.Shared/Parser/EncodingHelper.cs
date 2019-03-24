using System.Text;

namespace PortraitBuilder.Parser
{
    internal static class EncodingHelper
    {
        static EncodingHelper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static Encoding WesternEncoding => Encoding.GetEncoding(1252);
    }
}
