using Blish_HUD;
using Gw2Sharp.WebApi;
using System.Globalization;
namespace Nekres.Stream_Out
{
    public static class OverlayServiceExtensions
    {
        public static CultureInfo CultureInfo(this OverlayService overlay)
        {
            return overlay.UserLocale.Value switch
            {
                Locale.English => new CultureInfo("en-US"),
                Locale.Spanish => new CultureInfo("es-ES"),
                Locale.German => new CultureInfo("de-DE"),
                Locale.French => new CultureInfo("fr-FR"),
                Locale.Korean => new CultureInfo("ko-KR"),
                Locale.Chinese => new CultureInfo("zh-CN"),
                _ => System.Globalization.CultureInfo.CurrentCulture
            };
        }
    }
}
