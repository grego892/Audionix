using Serilog;
using ATL;
using Audionix.Models;
using Audionix.Services;

namespace Audionix.Services
{
    public class AudioMetadataService
    {
        private readonly AppDatabaseService _databaseService;

        public AudioMetadataService(AppDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<AudioMetadata> GetMetadataAsync(string filepath)
        {
            Track? theTrack = null;

            for (int i = 0; i < 3; i++) // Try 3 times
            {
                Log.Debug($"++++++ AudioMetadataService -- GetMetadata() - Attempting to read file: {filepath} ** Try #: {i}");
                try
                {
                    theTrack = new Track(filepath);
                    break; // Success, break the loop
                }
                catch (IOException)
                {
                    if (i == 2) // If this was the last attempt, rethrow the exception
                    {
                        Log.Error($"++++++ AudioMetadataService -- GetMetadata() - Error reading file: {filepath}");
                        throw;
                    }

                    // Wait for a bit before trying again
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            if (theTrack != null)
            {
                var additionalFields = theTrack.AdditionalFields;
                var audioMetadata = new AudioMetadata
                {
                    Filename = theTrack.Title,
                    Artist = theTrack.Artist,
                    Title = additionalFields.TryGetValue("disp.entry[0].value", out string? title) ? title.Trim('\0') : string.Empty,
                    Intro = additionalFields.TryGetValue("info.ISRF", out var ISRFfield) && Int16.TryParse(ISRFfield, out var intro) ? intro : (short)0,
                    Segue = additionalFields.TryGetValue("info.IMED", out var IMEDfield) && Int16.TryParse(IMEDfield, out var segue) ? segue : (short)0,
                    Duration = theTrack.Duration
                };

                Log.Information($"--- AudioMetadata -- GetMetadata() - Audio AudioMetadata.IntroSeconds:  {audioMetadata.IntroSeconds}");
                Log.Information($"--- AudioMetadata -- GetMetadata() - AudioMetadata.SegueSeconds:  {audioMetadata.SegueSeconds}");
                Log.Information($"--- AudioMetadata -- GetMetadata() - AudioMetadata.Intro:  {audioMetadata.Intro}");
                Log.Information($"--- AudioMetadata -- GetMetadata() - AudioMetadata.Segue:  {audioMetadata.Segue}");
                Log.Information($"--- AudioMetadata -- GetMetadata() - AudioMetadata.Duration:  {audioMetadata.Duration}");

                return audioMetadata;
            }
            else
            {
                Log.Error($"++++++ AudioMetadata -- GetMetadata() - Error creating Track from filepath: {filepath}");
                throw new Exception("Unable to create Track from filepath.");
            }
        }

        public async Task SaveAudioMetadata(AudioMetadata audioMetadata, string fileName, Guid selectedStation)
        {
            // Create a new AudioMetadata instance and set its properties
            var audioMetadataForDb = new AudioMetadata
            {
                Filename = fileName,
                Title = audioMetadata.Title,
                Artist = audioMetadata.Artist,
                Duration = audioMetadata.Duration,
                Intro = audioMetadata.Intro,
                Segue = audioMetadata.Segue,
                Folder = audioMetadata.Folder
            };

            // Find the station with the selected call letters and assign its ID to StationId
            var station = await _databaseService.GetStationByIdAsync(selectedStation);
            if (station != null)
            {
                audioMetadataForDb.StationId = station.StationId;
            }
            else
            {
                Log.Error("Station with ID {StationId} not found", selectedStation);
            }

            await _databaseService.AddAudioFileAsync(audioMetadataForDb);
        }
    }
}
