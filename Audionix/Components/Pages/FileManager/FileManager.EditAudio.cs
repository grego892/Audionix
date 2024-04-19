using Audionix.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WavesurferBlazorWrapper;
using System;

namespace Audionix.Components.Pages.FileManager
{
    public partial class FileManager
    {
        private async Task EditAudio(AudioMetadata audioMetadata)
        {
            isUploading = true;
            progress = 0;
            Log.Information("--- FileManager - EditAudio() -- EditAudio() START** -aidofile: " + audioMetadata.Filename);

            if (wavePlayer != null)
            {
                await StopAndEmptyWavePlayer();

                var url = await RequestFileFromAPI(audioMetadata);
                if (!string.IsNullOrEmpty(url))
                {
                    await LoadFileIntoWavePlayer(url, audioMetadata);

                    await UpdateWavePlayerRegions(audioMetadata);

                    UpdateEditorFields(audioMetadata);
                }
            }
            else
            {
                Log.Error("++++++ FileManager - EditAudio() -- No wavePlayer found");
            }
        }

        private async Task StopAndEmptyWavePlayer()
        {
            await wavePlayer.Stop();
            await wavePlayer.Empty();
            await wavePlayer.RegionClearRegions();
        }

        private async Task<string> RequestFileFromAPI(AudioMetadata audioMetadata)
        {
            var request = HttpContextAccessor?.HttpContext?.Request;
            if (request != null)
            {
                var host = request.Host.ToUriComponent();
                var scheme = request.Scheme;
                string encodedFilename = System.Net.WebUtility.UrlEncode(audioMetadata.Filename);
                string encodedFoldername = System.Net.WebUtility.UrlEncode(audioMetadata.Folder);
                string url = $"{scheme}://{host}/api/audio/{selectedStation}/{encodedFoldername}/{encodedFilename}";

                Log.Information("--- FileManager - EditAudio() -- EditAudio sending to API: " + url);

                try
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(url);
                    Log.Information("--- FileManager - EditAudio() -- EditAudio response: " + response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        return url;
                    }
                    else
                    {
                        Log.Error("++++++ FileManager - EditAudio() -- Error making HTTP request");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "++++++ FileManager - EditAudio() - FileManager - EditAudio() - Error making HTTP request");
                }
            }
            else
            {
                Log.Error("--- FileManager - EditAudio() -- No HttpContextAccessor.HttpContext?.Request found");
            }

            return string.Empty;
        }

        private async Task LoadFileIntoWavePlayer(string url, AudioMetadata audioMetadata)
        {
            wavePlayer?.Load(url);
            audioMetadata = DbContext.AudioFiles.FirstOrDefault(am => am.Filename == audioMetadata.Filename);
        }

        private async Task UpdateWavePlayerRegions(AudioMetadata audioMetadata)
        {
            if (wavePlayer != null)
            {
                IEnumerable<WavesurferRegion>? regions = await wavePlayer.RegionList();

                if (regions != null)
                {
                    var introRegion = regions.FirstOrDefault(r => r.Id == "Intro");

                    if (introRegion == null)
                    {
                        await wavePlayer.RegionAddRegion(
                            new WavesurferRegion()
                            {
                                Start = 0,
                                End = ((float)audioMetadata.Intro) / 1000,
                                Resize = true,
                                Color = "rgba(10,200,25,0.3)",
                                Drag = true,
                                Id = "Intro"
                            });
                    }
                    else
                    {
                        introRegion.Start = 0;
                        introRegion.End = (float)audioMetadata.IntroSeconds;
                    }
                    await wavePlayer.RegionListUpdate(regions);

                    float? duration = await wavePlayer.GetDuration();

                    var segueRegion = regions.FirstOrDefault(r => r.Id == "Segue");
                    if (segueRegion == null)
                    {
                        await wavePlayer.RegionAddRegion(
                            new WavesurferRegion()
                            {
                                Start = (float)audioMetadata.Duration - ((audioMetadata.Segue) / 1000),
                                End = duration.HasValue ? (float)audioMetadata.Duration : 0,
                                Resize = true,
                                Color = "rgba(200,10,25,0.3)",
                                Drag = true,
                                Id = "Segue"
                            });
                    }
                    else
                    {
                        segueRegion.Start = (float)(duration.HasValue ? ((audioMetadata.Duration - audioMetadata.Segue)) : 0);
                        segueRegion.End = duration.HasValue ? (float)(audioMetadata.Duration) : 0;
                    }
                    await wavePlayer.RegionListUpdate(regions);
                }
            }
            else
            {
                Log.Error("++++++ FileManager - UpdateWavePlayerRegions() -- No wavePlayer found");
            }
        }


        private void UpdateEditorFields(AudioMetadata audioMetadata)
        {
            EditorTitle = audioMetadata?.Title ?? string.Empty;
            EditorArtist = audioMetadata?.Artist ?? string.Empty;
            EditorIntro = audioMetadata?.Intro ?? 0;
            EditorSegue = audioMetadata?.Segue ?? 0;
            StateHasChanged();
        }
    }
}