using SharedLibrary.Models;
using SharedLibrary.Repositories;

namespace Audionix.Services
{
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

        private readonly IAppSettingsRepository _appSettingsRepository;

        public AppStateService(IAppSettingsRepository appSettingsRepository)
        {
            _appSettingsRepository = appSettingsRepository ?? throw new ArgumentNullException(nameof(appSettingsRepository));
        }

        public async Task LoadAppSettingsAsync()
        {
            AppSettings = await _appSettingsRepository.GetAppSettingsAsync();
        }

        public async Task SaveAppSettingsAsync()
        {
            if (AppSettings != null)
            {
                await _appSettingsRepository.SaveAppSettingsAsync(AppSettings);
                NotifyAppSettingsChanged();
            }
        }
    }
}
