using Audionix.Models;
using Serilog;
using System.Xml.Linq;

namespace Audionix.Data.StationLog
{
    public static class LoadLog
    {
        public static IEnumerable<ProgramLogItem>? LoadLogFromXML(string logPath)
        {
            Log.Information($"--- LoadLog - LoadLogFromXML -- Loading log from {logPath}", logPath);
            if (File.Exists(logPath))
            {
                XDocument xDocument = XDocument.Load(logPath);
                IEnumerable<ProgramLogItem> log =
                from element in xDocument.Descendants("TProgramLogItem")
                select new ProgramLogItem
                {
                    Cue = element.Element("Cue")?.Value,
                    Description = element.Element("Description")?.Value,
                    Scheduled = element.Element("Scheduled")?.Value.ToString(),
                    Name = element.Element("Name")?.Value,
                    Length = element.Element("Length")?.Value,
                    Segue = element.Element("Segue")?.Value.ToString(),
                    Category = element.Element("Category")?.Value,
                    Progress = 50
                };
                return log;
            }
            else
            {
                Log.Error($"--- LoadLog - LoadLogFromXML() -- Log file not found at {logPath}", logPath);
                return null;
            }
        }
    }
}