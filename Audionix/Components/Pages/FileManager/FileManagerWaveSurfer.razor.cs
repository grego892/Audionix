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

            if (percent == 100)
            {
                Log.Information("--- FileManagerWaveSurfer - WaveLoading() -- Wave load progress complete.  Percent: " + percent);
                Task.Run(async () =>
                {
                    await Task.Delay(2000); // Wait for 2 seconds before resetting the progress
                    progress = 0;
                    await InvokeAsync(StateHasChanged); // Notify Blazor about the state change
                });
            }
        }

        public void WavePlayerPlay()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerPlay()");
            if (wavePlayer != null)
            {
                wavePlayer.Play();
            }
        }
        public void WavePlayerPause()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerPause()");
            if (wavePlayer != null)
            {
                wavePlayer.Pause();
            }
        }
        public void WavePlayerStop()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerStop()");
            if (wavePlayer != null)
            {
                wavePlayer.Stop();
            }
        }
        public void WavePlayerMute()
        {
            Log.Information("--- FileManagerWaveSurfer - WavePlayerMute()");
            if (wavePlayer != null)
            {
                wavePlayer.ToggleMute();
            }
        }
        public async Task GetWavePlayerCurrentPosition()
        {
            if (wavePlayer != null)
            {
                var currentTimeInSeconds = await wavePlayer.GetCurrentTime() ?? 0;
                WavePlayerCurrentPosition = TimeSpan.FromSeconds(currentTimeInSeconds);
            }
        }
        public async Task SetIntro()
        {
            if (wavePlayer != null)
            {
                var currentTime = await wavePlayer.GetCurrentTime();
                float? duration = await wavePlayer.GetDuration();

                IEnumerable<WavesurferRegion>? regions = await wavePlayer.RegionList();

                if (regions != null)
                {
                    var segueRegion = regions.FirstOrDefault(r => r.Id == "Intro");

                    if (segueRegion != null)
                    {
                        segueRegion.End = (float)currentTime;
                        await wavePlayer.RegionListUpdate(regions);
                    }
                    else
                    {
                        // Add a new "Segue" region to the wavePlayer
                        await wavePlayer.RegionAddRegion(
                            new WavesurferRegion()
                            {
                                Start = 0,
                                End = (float)currentTime,
                                Resize = true,
                                Color = "rgba(10,200,25,0.3)",
                                Drag = true,
                                Id = "Intro"
                            }
                        );
                    }
                }
            }
        }

        public async Task SetSegue()
        {
            if (wavePlayer != null)
            {
                float? currentTime = await wavePlayer.GetCurrentTime();
                float? duration = await wavePlayer.GetDuration();

                IEnumerable<WavesurferRegion>? regions = await wavePlayer.RegionList();

                if (regions != null)
                {
                    var segueRegion = regions.FirstOrDefault(r => r.Id == "Segue");

                    if (segueRegion != null)
                    {
                        segueRegion.Start = (float)currentTime;
                        await wavePlayer.RegionListUpdate(regions);
                    }
                    else
                    {
                        // Add a new "Segue" region to the wavePlayer
                        await wavePlayer.RegionAddRegion(
                            new WavesurferRegion()
                            {
                                Start = (float)currentTime,
                                End = (float)duration,
                                Resize = true,
                                Color = "rgba(200,10,25,0.3)",
                                Drag = true,
                                Id = "Segue"
                            }
                        );
                    }
                }
            }
        }

        public async Task GotoBeginning()
        {
            if (wavePlayer != null)
            {
                await wavePlayer.SeekTo(0);
            }
        }


        private double WavePlayerZoom = 0;
        //public TimeSpan? WavePlayerCurrentPosition { get; private set; } = TimeSpan.Zero;

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
