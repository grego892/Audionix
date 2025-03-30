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
                //return x.EventType switch
                //{
                //EventType.song => $"background: {Colors.Indigo.Lighten1};",
                //EventType.promo => $"background: {Colors.Indigo.Lighten2};",
                //EventType.liner => $"background: {Colors.Blue.Lighten1};",
                //EventType.macro => $"background: {Colors.BlueGray.Lighten1};",
                //EventType.spot => $"background: {Colors.Green.Lighten1};",
                //_ => "background: #000000;"
                //};
            }

            return "background: #000000;";
        };
    }
}
