using Audionix.Shared.Models;

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

    public event EventHandler? OnStationChanged;

    private void NotifyStationChanged() => OnStationChanged?.Invoke(this, EventArgs.Empty);
}
