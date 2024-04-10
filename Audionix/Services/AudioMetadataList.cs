using Audionix.Models;
using Serilog;
using ATL;
using Microsoft.VisualStudio.TextTemplating;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Audionix.Services
{
    public class AudioMetadataList
    {
        public AudioMetadata GetMetadata(string filepath)
        {
            Track? theTrack = null;

            for (int i = 0; i < 3; i++) // Try 3 times
            {
                Log.Debug("++++++ AudioMetadataList -- GetMetadata() - Attempting to read file: " + filepath + " ** Try #: " + i.ToString());
                try
                {
                    theTrack = new Track(filepath);
                    break; // Success, break the loop
                }
                catch (IOException)
                {
                    if (i == 2) // If this was the last attempt, rethrow the exception
                    {
                        Log.Error("++++++ AudioMetadataList -- GetMetadata() - Error reading file: " + filepath);
                        throw;
                    }

                    // Wait for a bit before trying again
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                }
            }

            if (theTrack != null)
            {
                var additionalFields = theTrack.AdditionalFields;
                var AudioMetadata = new AudioMetadata();

                AudioMetadata.Filename = theTrack.Title;
                AudioMetadata.Artist = theTrack.Artist;

                //if (theTrack.AdditionalFields.TryGetValue("disp.entry[0].value", out var title))
                //{
                //    Console.WriteLine("================================== Title: " + title);
                //    AudioMetadata.Title = title;
                //}

                if (additionalFields.TryGetValue("disp.entry[0].value", out string? title))
                {
                    AudioMetadata.Title = title;
                }

                if (additionalFields.TryGetValue("info.ISRF", out var ISRFfield) && Int16.TryParse(ISRFfield, out var intro))
                {
                    AudioMetadata.Intro = intro;
                }

                if (additionalFields.TryGetValue("info.IMED", out var IMEDfield) && Int16.TryParse(IMEDfield, out var segue))
                {
                    AudioMetadata.Segue = segue;
                }

                AudioMetadata.IntroSeconds = AudioMetadata.Intro * .001;
                AudioMetadata.SegueSeconds = AudioMetadata.Segue * .001;
                AudioMetadata.Duration = theTrack.Duration * 1000;

                Log.Information("--- AudioMetadata -- GetMetadata() - Audio AudioMetadata.IntroSeconds:  " + AudioMetadata.IntroSeconds);
                Log.Information("--- AudioMetadata -- GetMetadata() - AudioMetadata.SegueSeconds:  " + AudioMetadata.SegueSeconds);
                Log.Information("--- AudioMetadata -- GetMetadata() - AudioMetadata.Intro:  " + AudioMetadata.Intro);
                Log.Information("--- AudioMetadata -- GetMetadata() - AudioMetadata.Segue:  " + AudioMetadata.Segue);
                Log.Information("--- AudioMetadata -- GetMetadata() - AudioMetadata.Duration:  " + AudioMetadata.Duration);

                return AudioMetadata;
            }
            else
            {
                Log.Error("++++++ AudioMetadata -- GetMetadata() - Error creating Track from filepath: " + filepath);
                throw new Exception("Unable to create Track from filepath.");
            }
        }
    }
}
