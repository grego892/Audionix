using Audionix.Models;
using Audionix.Repositories;
using System;

public class AppStateService
{
    private Station? _station;
    public Station? station
    {
        get => _station;
        set
        {
            if (_station != value)
            {
                _station = value;
                NotifyStationChanged();
            }
        }
    }

    private AppSettings? _appSettings;
    public AppSettings? AppSettings
    {
        get => _appSettings;
        set
        {
            if (_appSettings != value)
            {
                _appSettings = value;
                NotifyAppSettingsChanged();
            }
        }
    }

    public event EventHandler? OnStationChanged;
    public event EventHandler? OnAppSettingsChanged;

    private void NotifyStationChanged() => OnStationChanged?.Invoke(this, EventArgs.Empty);
    private void NotifyAppSettingsChanged() => OnAppSettingsChanged?.Invoke(this, EventArgs.Empty);

    public async Task LoadAppSettingsAsync(IStationRepository stationRepository)
    {
        AppSettings = await stationRepository.GetAppSettingsAsync();
    }

    public async Task SaveAppSettingsAsync(IStationRepository stationRepository)
    {
        if (AppSettings != null)
        {
            await stationRepository.SaveAppSettingsAsync(AppSettings);
            NotifyAppSettingsChanged();
        }
    }
}
