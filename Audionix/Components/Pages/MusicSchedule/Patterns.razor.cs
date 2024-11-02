using Audionix.Data.StationLog;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
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
        private List<Station> stations = new();
        private Guid? selectedStationId;
        private List<Category> selectedPatternCategories = new();
        private Guid? selectedCategoryId;
        private List<Category> filteredCategories = new();
        private List<Category> categories = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            stations = await DbContext.Stations.ToListAsync();
            MusicPatternNames = await DbContext.MusicPatterns.Select(mp => mp.Name!).ToListAsync();
            categories = await DbContext.Categories.ToListAsync();
            selectedStationId = null;
        }

        private async Task OnStationChanged(Guid? stationId)
        {
            selectedStationId = stationId;
            await FilterPatterns();
            selectedMusicPatternName = null;
            FilterCategories();
        }

        private async Task FilterPatterns()
        {
            if (selectedStationId.HasValue)
            {
                filteredMusicPatternNames = await DbContext.MusicPatterns
                    .Where(c => c.StationId == selectedStationId.Value)
                    .ToListAsync();
            }
            else
            {
                filteredMusicPatternNames = new List<MusicPattern>();
            }
        }

        private async Task AddMusicPattern()
        {
            if (selectedStationId.HasValue)
            {
                var newMusicPattern = new MusicPattern { Name = newMusicPatternName, StationId = selectedStationId.Value, PatternId = Guid.NewGuid() };
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
            if (!string.IsNullOrEmpty(selectedMusicPatternName))
            {
                var selectedPattern = await DbContext.MusicPatterns
                    .Include(mp => mp.PatternCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(mp => mp.Name == selectedMusicPatternName && mp.StationId == selectedStationId);

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
            if (selectedStationId.HasValue && !string.IsNullOrEmpty(selectedMusicPatternName) && selectedCategoryId.HasValue)
            {
                var musicPattern = await DbContext.MusicPatterns
                    .Include(mp => mp.PatternCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(mp => mp.StationId == selectedStationId.Value && mp.Name == selectedMusicPatternName);

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
                            StationId = selectedStationId.Value // Ensure StationId is set
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


        private void FilterCategories()
        {
            filteredCategories = categories.Where(c => c.StationId == selectedStationId).ToList();
        }

        private void OnCategoryChanged(Guid? categoryId)
        {
            selectedCategoryId = categoryId;
        }

        public void Dispose()
        {
            DbContext?.Dispose();
        }

        public void RemoveCategoryFromPattern(Category category)
        {
            var patternCategory = DbContext.PatternCategories.FirstOrDefault(pc => pc.CategoryId == category.CategoryId);
            if (patternCategory != null)
            {
                DbContext.PatternCategories.Remove(patternCategory);
                DbContext.SaveChanges();
                selectedPatternCategories.Remove(category);
                StateHasChanged();
            }
        }

        public void MoveCategoryUp(Category category)
        {
            var currentPatternCategory = DbContext.PatternCategories
                .FirstOrDefault(pc => pc.CategoryId == category.CategoryId && pc.MusicPatternId == selectedPatternCategories.First().PatternCategories.First().MusicPatternId);

            if (currentPatternCategory != null && currentPatternCategory.MusicPatternSortOrder > 1)
            {
                var previousPatternCategory = DbContext.PatternCategories
                    .FirstOrDefault(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId && pc.MusicPatternSortOrder == currentPatternCategory.MusicPatternSortOrder - 1);

                if (previousPatternCategory != null)
                {
                    // Swap the sort orders
                    int tempOrder = currentPatternCategory.MusicPatternSortOrder;
                    currentPatternCategory.MusicPatternSortOrder = previousPatternCategory.MusicPatternSortOrder;
                    previousPatternCategory.MusicPatternSortOrder = tempOrder;

                    DbContext.SaveChanges();

                    // Refresh the list
                    selectedPatternCategories = DbContext.PatternCategories
                        .Where(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId)
                        .OrderBy(pc => pc.MusicPatternSortOrder)
                        .Select(pc => pc.Category)
                        .ToList();

                    StateHasChanged();
                }
            }
        }

        public void MoveCategoryDown(Category category)
        {
            var currentPatternCategory = DbContext.PatternCategories
                .FirstOrDefault(pc => pc.CategoryId == category.CategoryId && pc.MusicPatternId == selectedPatternCategories.First().PatternCategories.First().MusicPatternId);

            if (currentPatternCategory != null)
            {
                var nextPatternCategory = DbContext.PatternCategories
                    .FirstOrDefault(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId && pc.MusicPatternSortOrder == currentPatternCategory.MusicPatternSortOrder + 1);

                if (nextPatternCategory != null)
                {
                    // Swap the sort orders
                    int tempOrder = currentPatternCategory.MusicPatternSortOrder;
                    currentPatternCategory.MusicPatternSortOrder = nextPatternCategory.MusicPatternSortOrder;
                    nextPatternCategory.MusicPatternSortOrder = tempOrder;

                    DbContext.SaveChanges();

                    // Refresh the list
                    selectedPatternCategories = DbContext.PatternCategories
                        .Where(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId)
                        .OrderBy(pc => pc.MusicPatternSortOrder)
                        .Select(pc => pc.Category)
                        .ToList();

                    StateHasChanged();
                }
            }
        }
    }
}