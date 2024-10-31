using Serilog;
using System.Drawing;
using WavesurferBlazorWrapper;

namespace Audionix.Components.Pages.FileManager
{
    partial class FileManager
    {
        private double WavePlayerVolume = 50.0;
        public TimeSpan? WavePlayerCurrentPosition { get; private set; } = TimeSpan.Zero;

        public void WaveLoading(int percent)
        {
            progress = percent;

            Log.Debug($"--- FileManagerWaveSurfer - WaveLoading() -- Wave load progress.  Percent: {percent}");

            if (percent == 100)
            {
                Log.Information($"--- FileManagerWaveSurfer - WaveLoading() -- Wave load progress complete.  Percent: {percent}");
                isUploading = false;
            }
        }

        public void WavePlayerPlay()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerPlay()");
            wavePlayer?.Play();
        }

        public void WavePlayerPause()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerPause()");
            wavePlayer?.Pause();
        }

        public void WavePlayerStop()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerStop()");
            wavePlayer?.Stop();
        }

        public void WavePlayerMute()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerMute()");
            wavePlayer?.ToggleMute();
        }

        public async Task GetWavePlayerCurrentPosition()
        {
            if (wavePlayer != null)
            {
                var currentTimeInSeconds = await wavePlayer.GetCurrentTime() ?? 0;
                WavePlayerCurrentPosition = TimeSpan.FromSeconds(currentTimeInSeconds);
            }
        }

        private async Task UpdateRegion(string regionId, float? currentTime, Action<WavesurferRegion, float?> updateFunc)
        {
            if (wavePlayer != null)
            {
                var regions = await wavePlayer.RegionList();

                if (regions != null)
                {
                    var region = regions.FirstOrDefault(r => r.Id == regionId);

                    if (region != null)
                    {
                        updateFunc(region, currentTime);
                        await wavePlayer.RegionListUpdate(regions);
                    }
                    else
                    {
                        // Add a new region to the wavePlayer
                        await wavePlayer.RegionAddRegion(
                            new WavesurferRegion()
                            {
                                Start = 0,
                                End = currentTime ?? 0,
                                Resize = true,
                                Color = regionId == "Intro" ? "rgba(10,200,25,0.3)" : "rgba(200,10,25,0.3)",
                                Drag = true,
                                Id = regionId
                            }
                        );
                    }
                }
            }
        }

        public async Task SetIntro()
        {
            var currentTime = wavePlayer != null ? await wavePlayer.GetCurrentTime() : null;
            await UpdateRegion("Intro", currentTime, (region, time) => region.End = time ?? 0);
        }

        public async Task SetSegue()
        {
            var currentTime = wavePlayer != null ? await wavePlayer.GetCurrentTime() : null;
            await UpdateRegion("Segue", currentTime, (region, time) => region.Start = time ?? 0);
        }


        public async Task GotoBeginning()
        {
            if (wavePlayer != null)
            {
                await wavePlayer.SeekTo(0);
            }
        }

        private double WavePlayerZoom = 0;

        public async Task SetWavePlayerZoom(double value)
        {
            WavePlayerZoom = value;
            wavePlayer?.Zoom((int)value);
            await Task.CompletedTask;
        }

        public async Task SetWavePlayerVolume(double volume)
        {
            WavePlayerVolume = volume;
            wavePlayer?.SetVolume((float)(volume / 100));
            await Task.CompletedTask;
        }
    }
}
