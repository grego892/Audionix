using Audionix.Shared.Models.MusicSchedule;
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
        private List<Category> categories = new();
        [Inject] private AppStateService AppStateService { get; set; }

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
            MusicPatternNames = await DbContext.MusicPatterns.Select(mp => mp.Name!).ToListAsync();
            categories = await DbContext.Categories.ToListAsync();
        }

        private async Task FilterPatterns()
        {
            if (AppStateService.station != null)
            {
                filteredMusicPatternNames = await DbContext.MusicPatterns
                    .Where(c => c.StationId == AppStateService.station.StationId)
                    .ToListAsync();
            }
            else
            {
                filteredMusicPatternNames = new List<MusicPattern>();
            }
        }

        private async Task AddMusicPattern()
        {
            if (AppStateService.station != null)
            {
                var newMusicPattern = new MusicPattern
                {
                    Name = newMusicPatternName,
                    StationId = AppStateService.station.StationId,
                    PatternId = Guid.NewGuid()
                };
                DbContext.MusicPatterns.Add(newMusicPattern);
                await DbContext.SaveChangesAsync();
                newMusicPatternName = string.Empty;

                await FilterPatterns();
                StateHasChanged();
            }
        }

        private async Task RemoveMusicPattern()
        {
            var musicPatternToRemove = await DbContext.MusicPatterns.FirstOrDefaultAsync(mp => mp.Name == selectedMusicPatternName);
            if (musicPatternToRemove != null)
            {
                DbContext.MusicPatterns.Remove(musicPatternToRemove);
                await DbContext.SaveChangesAsync();
                MusicPatternNames.Remove(selectedMusicPatternName!);
                selectedMusicPatternName = null;
                await FilterPatterns();
                StateHasChanged();
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
            if (!string.IsNullOrEmpty(selectedMusicPatternName) && AppStateService.station != null)
            {
                var selectedPattern = await DbContext.MusicPatterns
                    .Include(mp => mp.PatternCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(mp => mp.Name == selectedMusicPatternName && mp.StationId == AppStateService.station.StationId);

                if (selectedPattern != null)
                {
                    selectedPatternCategories = selectedPattern.PatternCategories
                        .OrderBy(pc => pc.MusicPatternSortOrder)
                        .Select(pc => pc.Category)
                        .ToList();
                }
                else
                {
                    selectedPatternCategories = new List<Category>();
                }
            }
            else
            {
                selectedPatternCategories = new List<Category>();
            }
        }

        private async Task AddCategoryToPattern()
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(selectedMusicPatternName) && selectedCategoryId.HasValue)
            {
                var musicPattern = await DbContext.MusicPatterns
                    .Include(mp => mp.PatternCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(mp => mp.StationId == AppStateService.station.StationId && mp.Name == selectedMusicPatternName);

                if (musicPattern != null)
                {
                    var category = await DbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == selectedCategoryId.Value);

                    if (category != null)
                    {
                        var maxSortOrder = musicPattern.PatternCategories.Any()
                            ? musicPattern.PatternCategories.Max(pc => pc.MusicPatternSortOrder)
                            : 0;

                        var patternCategory = new PatternCategory
                        {
                            MusicPatternId = musicPattern.PatternId,
                            CategoryId = category.CategoryId,
                            CategoryName = category.CategoryName!,
                            MusicPatternSortOrder = maxSortOrder + 1,
                            StationId = AppStateService.station.StationId // Ensure StationId is set
                        };

                        musicPattern.PatternCategories.Add(patternCategory);
                        await DbContext.SaveChangesAsync();

                        selectedPatternCategories = musicPattern.PatternCategories
                            .OrderBy(pc => pc.MusicPatternSortOrder)
                            .Select(pc => pc.Category)
                            .ToList();
                        StateHasChanged();
                    }
                }
            }
        }

        private async Task FilterCategories()
        {
            if (AppStateService.station != null)
            {
                filteredCategories = categories.Where(c => c.StationId == AppStateService.station.StationId).ToList();
            }
        }

        private void OnCategoryChanged(Guid? categoryId)
        {
            selectedCategoryId = categoryId;
        }

        public async Task RemoveCategoryFromPattern(Category category)
        {
            var patternCategory = await DbContext.PatternCategories.FirstOrDefaultAsync(pc => pc.CategoryId == category.CategoryId);
            if (patternCategory != null)
            {
                DbContext.PatternCategories.Remove(patternCategory);
                await DbContext.SaveChangesAsync();
                selectedPatternCategories.Remove(category);
                StateHasChanged();
            }
        }

        public async Task MoveCategoryUp(Category category)
        {
            var currentPatternCategory = await DbContext.PatternCategories
                .FirstOrDefaultAsync(pc => pc.CategoryId == category.CategoryId && pc.MusicPatternId == selectedPatternCategories.First().PatternCategories.First().MusicPatternId);

            if (currentPatternCategory != null && currentPatternCategory.MusicPatternSortOrder > 1)
            {
                var previousPatternCategory = await DbContext.PatternCategories
                    .FirstOrDefaultAsync(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId && pc.MusicPatternSortOrder == currentPatternCategory.MusicPatternSortOrder - 1);

                if (previousPatternCategory != null)
                {
                    // Swap the sort orders
                    int tempOrder = currentPatternCategory.MusicPatternSortOrder;
                    currentPatternCategory.MusicPatternSortOrder = previousPatternCategory.MusicPatternSortOrder;
                    previousPatternCategory.MusicPatternSortOrder = tempOrder;

                    await DbContext.SaveChangesAsync();

                    // Refresh the list
                    selectedPatternCategories = await DbContext.PatternCategories
                        .Where(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId)
                        .OrderBy(pc => pc.MusicPatternSortOrder)
                        .Select(pc => pc.Category)
                        .ToListAsync();

                    StateHasChanged();
                }
            }
        }

        public async Task MoveCategoryDown(Category category)
        {
            var currentPatternCategory = await DbContext.PatternCategories
                .FirstOrDefaultAsync(pc => pc.CategoryId == category.CategoryId && pc.MusicPatternId == selectedPatternCategories.First().PatternCategories.First().MusicPatternId);

            if (currentPatternCategory != null)
            {
                var nextPatternCategory = await DbContext.PatternCategories
                    .FirstOrDefaultAsync(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId && pc.MusicPatternSortOrder == currentPatternCategory.MusicPatternSortOrder + 1);

                if (nextPatternCategory != null)
                {
                    // Swap the sort orders
                    int tempOrder = currentPatternCategory.MusicPatternSortOrder;
                    currentPatternCategory.MusicPatternSortOrder = nextPatternCategory.MusicPatternSortOrder;
                    nextPatternCategory.MusicPatternSortOrder = tempOrder;

                    await DbContext.SaveChangesAsync();

                    // Refresh the list
                    selectedPatternCategories = await DbContext.PatternCategories
                        .Where(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId)
                        .OrderBy(pc => pc.MusicPatternSortOrder)
                        .Select(pc => pc.Category)
                        .ToListAsync();

                    StateHasChanged();
                }
            }
        }

        public void Dispose()
        {
            DbContext?.Dispose();
            AppStateService.OnStationChanged -= HandleStationChanged;
        }
    }
}
