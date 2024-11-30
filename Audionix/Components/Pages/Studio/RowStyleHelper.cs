using Audionix.Models;
using MudBlazor;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio : IDisposable
    {
        private static Func<ProgramLogItem, int, string> RowStyleFunc => (x, i) =>
        {
            if (x.States == StatesType.hasPlayed)
            {
                return "background: #969696;";
            }

            string progressBackground = "";
            if (x.Progress > 0 && x.Progress <= 100)
            {
                progressBackground = $"background: linear-gradient(to right, #4caf50 {x.Progress}%, transparent {x.Progress}%);";
            }

            string audioTypeBackground = x.AudioType switch
            {
                AudioType.song => $"background: {Colors.Indigo.Lighten1};",
                AudioType.promo => $"background: {Colors.Indigo.Lighten2};",
                AudioType.liner => $"background: {Colors.Blue.Lighten1};",
                AudioType.macro => $"background: {Colors.BlueGray.Lighten1};",
                AudioType.spot => $"background: {Colors.Green.Lighten1};",
                _ => "background: #000000;"
            };

            return $"{audioTypeBackground} {progressBackground}";
        };
    }
}
