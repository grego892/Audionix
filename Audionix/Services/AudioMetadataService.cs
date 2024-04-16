using Audionix.Models;
using Serilog;
using ATL;

namespace Audionix.Services
{
    public class AudioMetadataService
    {
        public AudioMetadata GetMetadata(string filepath)
        {
            Track? theTrack = null;

            for (int i = 0; i < 3; i++) // Try 3 times
            {
                Log.Debug("++++++ AudioMetadataService -- GetMetadata() - Attempting to read file: " + filepath + " ** Try #: " + i.ToString());
                try
                {
                    theTrack = new Track(filepath);
                    break; // Success, break the loop
                }
                catch (IOException)
                {
                    if (i == 2) // If this was the last attempt, rethrow the exception
                    {
                        Log.Error("++++++ AudioMetadataService -- GetMetadata() - Error reading file: " + filepath);
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

                if (additionalFields.TryGetValue("disp.entry[0].value", out string? title))
                {
                    AudioMetadata.Title = title.Trim('\0');
                }

                if (additionalFields.TryGetValue("info.ISRF", out var ISRFfield) && Int16.TryParse(ISRFfield, out var intro))
                {
                    AudioMetadata.Intro = intro;
                }

                if (additionalFields.TryGetValue("info.IMED", out var IMEDfield) && Int16.TryParse(IMEDfield, out var segue))
                {
                    AudioMetadata.Segue = segue;
                }

                AudioMetadata.Duration = theTrack.Duration;

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
