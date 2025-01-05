using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class SongCategories : IDisposable
    {
        private List<SongCategory> songCategories = new();
        private List<SongCategory> filteredSongCategories = new();
        private string? newSongCategoryName;
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private ISongCategoryRepository? SongCategoryRepository { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadCategoriesAsync();
            FilterCategories();
            AppStateService.OnStationChanged += HandleStationChanged;
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            await LoadCategoriesAsync();
            FilterCategories();
            StateHasChanged();
        }

        private async Task LoadCategoriesAsync()
        {
            if (AppStateService.station != null)
            {
                songCategories = await SongCategoryRepository.GetSongCategoriesAsync(AppStateService.station.StationId);
            }
        }

        private void FilterCategories()
        {
            if (AppStateService.station != null)
            {
                filteredSongCategories = songCategories.Where(c => c.StationId == AppStateService.station.StationId).ToList();
            }
        }

        private async Task AddSongCategory()
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(newSongCategoryName))
            {
                var songCategory = new SongCategory
                {
                    SongCategoryName = newSongCategoryName,
                    StationId = AppStateService.station.StationId,
                    SongCategoryId = Guid.NewGuid()
                };
                await SongCategoryRepository.AddSongCategoryAsync(songCategory);
                songCategories.Add(songCategory);
                FilterCategories();
                newSongCategoryName = string.Empty;
            }
        }

        private async Task DeleteSongCategory(Guid songCategoryId)
        {
            await SongCategoryRepository.DeleteSongCategoryAsync(songCategoryId);
            songCategories.RemoveAll(c => c.SongCategoryId == songCategoryId);
            FilterCategories();
        }

        public void Dispose()
        {
            AppStateService.OnStationChanged -= HandleStationChanged;
        }
    }
}
