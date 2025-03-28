using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Audionix.Components.Pages.LogBuilder;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Clocks : IDisposable
    {
        private string? selectedMusicPatternName;
        private string newMusicPatternName = string.Empty;
        private List<MusicPattern> filteredMusicPatternNames = new();
        private List<string> MusicPatternNames = new List<string>();
        private List<SongCategory> MusicPatternDataList = new List<SongCategory>();
        private List<SongCategory> selectedPatternCategories = new();
        private int selectedSongCategoryId;
        private List<SongCategory> filteredSongCategories = new();
        private List<string> songCategories = new();
        [Inject] private AppStateService AppStateService { get; set; } = default!;
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private IMusicPatternRepository? MusicPatternRepository { get; set; }
        [Inject] private ISongCategoryRepository? SongCategoryRepository { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            await FilterPatterns();
            await FilterSongCategories();
            AppStateService.OnStationChanged += HandleStationChanged;
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            selectedMusicPatternName = null;
            MusicPatternNames.Clear();
            await LoadDataAsync();
            await FilterPatterns();
            await FilterSongCategories();
            StateHasChanged();
        }

        private async Task LoadDataAsync()
        {
            if (MusicPatternRepository != null)
            {
                MusicPatternNames = await MusicPatternRepository.GetMusicPatternNamesAsync();
            }
            if (SongCategoryRepository != null)
            {
                songCategories = await SongCategoryRepository.GetSongCategoryNamesAsync();
            }
        }

        private async Task FilterPatterns()
        {
            if (AppStateService.station != null && MusicPatternRepository != null)
            {
                filteredMusicPatternNames = await MusicPatternRepository.GetMusicPatternsAsync(AppStateService.station.StationId);
            }
            else
            {
                filteredMusicPatternNames = new List<MusicPattern>();
            }
        }

        private async Task AddMusicPattern()
        {
            if (AppStateService.station != null && MusicPatternRepository != null)
            {
                var newMusicPattern = new MusicPattern
                {
                    Name = newMusicPatternName,
                    StationId = AppStateService.station.StationId
                };
                await MusicPatternRepository.AddMusicPatternAsync(newMusicPattern);
                newMusicPatternName = string.Empty;

                await FilterPatterns();
                StateHasChanged();
            }
        }

        private async Task RemoveMusicPattern()
        {
            if (!string.IsNullOrEmpty(selectedMusicPatternName) && MusicPatternRepository != null)
            {
                var musicPatternToRemove = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPatternToRemove != null)
                {
                    await MusicPatternRepository.DeleteMusicPatternAsync(musicPatternToRemove);
                    MusicPatternNames.Remove(selectedMusicPatternName);
                    selectedMusicPatternName = null;
                    await FilterPatterns();
                    StateHasChanged();
                }
            }
        }

        private void OpenDialogRemoveMusicPattern()
        {
            var parameters = new DialogParameters();
            parameters.Add("OnConfirm", new Action(async () => await RemoveMusicPattern()));

            var options = new DialogOptions { CloseOnEscapeKey = true };
            DialogService.Show<TemplateDeleteDialog>("Are you sure you want to delete this pattern?", parameters, options);
        }

        private void SelectDefaultItem()
        {
            if (selectedMusicPatternName != null)
            {
                selectedMusicPatternName = MusicPatternNames.FirstOrDefault();
            }
        }

        private async Task OnPatternChanged(string? patternName)
        {
            selectedMusicPatternName = patternName;
            if (!string.IsNullOrEmpty(selectedMusicPatternName) && AppStateService.station != null && MusicPatternRepository != null)
            {
                selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(AppStateService.station.StationId, selectedMusicPatternName);
            }
            else
            {
                selectedPatternCategories = new List<SongCategory>();
            }
        }

        private async Task AddSongCategoryToPattern()
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && selectedSongCategoryId.HasValue && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.AddSongCategoryToPatternAsync(musicPattern.PatternId, selectedSongCategoryId);
                    selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(musicPattern.PatternId);
                    StateHasChanged();
                }
            }
        }


        private async Task FilterSongCategories()
        {
            if (AppStateService.station != null && SongCategoryRepository != null)
            {
                filteredSongCategories = await SongCategoryRepository.GetSongCategoriesAsync(AppStateService.station.StationId);
            }
        }

        private void OnSongCategoryChanged(int songCategoryId)
        {
            selectedSongCategoryId = songCategoryId;
        }

        public async Task RemoveSongCategoryFromPattern(SongCategory songCategory)
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.RemoveSongCategoryFromPatternAsync(musicPattern, songCategory);
                    selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(musicPattern.PatternId);
                    StateHasChanged();
                }
            }
        }

        public async Task MoveSongCategoryUp(SongCategory songCategory)
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.MoveSongCategoryUpAsync(musicPattern, songCategory);
                    selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(musicPattern.PatternId);
                    StateHasChanged();
                }
            }
        }

        public async Task MoveSongCategoryDown(SongCategory songCategory)
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.MoveSongCategoryDownAsync(musicPattern, songCategory);
                    selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(musicPattern.PatternId);
                    StateHasChanged();
                }
            }
        }

        public void Dispose()
        {
            AppStateService.OnStationChanged -= HandleStationChanged;
        }
    }
}
