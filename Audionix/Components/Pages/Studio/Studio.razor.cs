using Audionix.Data.StationLog;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using Serilog;

namespace Audionix.Components.Pages.Studio
{
    partial class Studio
    {
        private readonly string TEMPORARYLOGPATH = AppDomain.CurrentDomain.BaseDirectory + "Data\\log.xml";

        public IEnumerable<ProgramLogItem> ProgramLog = [];

        private void LoadTodaysLog()
        {
            Log.Information("--- Studio - LoadTodaysLog() -- Loading Today's Log");
            var loadedLog = LoadLog.LoadLogFromXML(TEMPORARYLOGPATH);
            ProgramLog = loadedLog ?? Enumerable.Empty<ProgramLogItem>();
        }

        private static Func<ProgramLogItem, int, string> RowStyleFunc => (x, i) =>
        {
            return x.Category switch
            {
                "SONG" => ($"background: {Colors.Indigo.Lighten1}; "),
                "AUDIO" => ($"background: {Colors.Blue.Lighten1}; "),
                "MACRO" => ($"background: {Colors.BlueGray.Lighten1}; "),
                "SPOT" => ($"background: {Colors.Green.Lighten1}; "),
                _ => "background-image: #000000)",
            };
        };

        protected override Task OnInitializedAsync()
        {
            Log.Information("--- Studio - OnInitializedAsync() -- Initializing Studio Page");
            LoadTodaysLog();
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}
