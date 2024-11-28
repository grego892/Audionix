using Audionix.Models.MusicSchedule;
using Audionix.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Patterns : IDisposable
    {
        private string? selectedMusicPatternName;
        private string newMusicPatternName = string.Empty;
        private List<MusicPattern> filteredMusicPatternNames = new();
        private List<string> MusicPatternNames = new List<string>();
        private List<Category> MusicPatternDataList = new List<Category>();
        private List<Category> selectedPatternCategories = new();
        private Guid? selectedCategoryId;
        private List<Category> filteredCategories = new();
        private List<string> categories = new(); // Changed from List<Category> to List<string>
        [Inject] private AppStateService AppStateService { get; set; } = default!;
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private IMusicPatternRepository? MusicPatternRepository { get; set; }
        [Inject] private ICategoryRepository? CategoryRepository { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            await FilterPatterns();
            await FilterCategories();
            AppStateService.OnStationChanged += HandleStationChanged;
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            selectedMusicPatternName = null;
            MusicPatternNames.Clear();
            selectedCategoryId = null;
            await LoadDataAsync();
            await FilterPatterns();
            await FilterCategories();
            StateHasChanged();
        }

        private async Task LoadDataAsync()
        {
            if (MusicPatternRepository != null)
            {
                MusicPatternNames = await MusicPatternRepository.GetMusicPatternNamesAsync();
            }
            if (CategoryRepository != null)
            {
                categories = await CategoryRepository.GetCategoryNamesAsync();
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
                    StationId = AppStateService.station.StationId,
                    PatternId = Guid.NewGuid()
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
                selectedPatternCategories = new List<Category>();
            }
        }

        private async Task AddCategoryToPattern()
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && selectedCategoryId.HasValue && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.AddCategoryToPatternAsync(musicPattern.PatternId, selectedCategoryId.Value);
                    selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(musicPattern.PatternId);
                    StateHasChanged();
                }
            }
        }


        private async Task FilterCategories()
        {
            if (AppStateService.station != null && CategoryRepository != null)
            {
                filteredCategories = await CategoryRepository.GetCategoriesAsync(AppStateService.station.StationId);
            }
        }

        private void OnCategoryChanged(Guid? categoryId)
        {
            selectedCategoryId = categoryId;
        }

        public async Task RemoveCategoryFromPattern(Category category)
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.RemoveCategoryFromPatternAsync(musicPattern, category);
                    selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(musicPattern.PatternId);
                    StateHasChanged();
                }
            }
        }

        public async Task MoveCategoryUp(Category category)
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.MoveCategoryUpAsync(musicPattern, category);
                    selectedPatternCategories = await MusicPatternRepository.GetSelectedPatternCategoriesAsync(musicPattern.PatternId);
                    StateHasChanged();
                }
            }
        }

        public async Task MoveCategoryDown(Category category)
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && MusicPatternRepository != null)
            {
                var musicPattern = await MusicPatternRepository.GetMusicPatternByNameAsync(selectedMusicPatternName);
                if (musicPattern != null)
                {
                    await MusicPatternRepository.MoveCategoryDownAsync(musicPattern, category);
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
