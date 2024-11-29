

using Audionix.Models;
using MudBlazor;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio : IDisposable
    {
        private static Func<ProgramLogItem, int, string> RowStyleFunc => (x, i) =>
        {

            if (x.Progress > 0 && x.Progress <= 100)
            {
                return $"background: linear-gradient(to right, #4caf50 {x.Progress}%, #ebebeb {x.Progress}%);";
            }

            return x.Category switch
            {
                "SONG" => ($"background: {Colors.Indigo.Lighten1}; "),
                "AUDIO" => ($"background: {Colors.Blue.Lighten1}; "),
                "MACRO" => ($"background: {Colors.BlueGray.Lighten1}; "),
                "SPOT" => ($"background: {Colors.Green.Lighten1}; "),
                _ => "background-image: #000000)",
            };

            return "";

        };        
    }
}
