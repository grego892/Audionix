using SharedLibrary.Models;
using MudBlazor;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio : IDisposable
    {
        private static Func<ProgramLogItem, int, string> RowStyleFunc => (x, i) =>
        {
            if (x.Status == StatusType.hasPlayed)
            {
                return "background: #969696;";
            }
            else if (x.Status == StatusType.isPlaying)
            {
                return $"background: linear-gradient(to right, #4caf50 {x.Progress}%, transparent {x.Progress}%);";
            }
            else if (x.Status == StatusType.notPlayed)
            {
                return x.SoundCodeId switch
                {
                    1 => $"background: {Colors.Indigo.Lighten1};", // song
                    2 => $"background: {Colors.Indigo.Lighten2};", // promo
                    3 => $"background: {Colors.Blue.Lighten1};", // liner
                    4 => $"background: {Colors.BlueGray.Lighten1};", // macro
                    5 => $"background: {Colors.Green.Lighten1};", // spot
                    _ => "background: #000000;"
                };
            }

            return "background: #000000;";
        };
    }
}
