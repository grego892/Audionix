using Audionix.Models.MusicSchedule;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Categories : IDisposable
    {
        private List<Category> categories = new();
        private List<Category> filteredCategories = new();
        private string? newCategoryName;
        [Inject] private AppDatabaseService StationService { get; set; }
        //[Inject] private AppStateService AppStateService { get; set; }

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
                categories = await StationService.GetCategoriesAsync(AppStateService.station.StationId);
            }
        }

        private void FilterCategories()
        {
            if (AppStateService.station != null)
            {
                filteredCategories = categories.Where(c => c.StationId == AppStateService.station.StationId).ToList();
            }
        }

        private async Task AddCategory()
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(newCategoryName))
            {
                var category = new Category
                {
                    CategoryName = newCategoryName,
                    StationId = AppStateService.station.StationId,
                    CategoryId = Guid.NewGuid()
                };
                await StationService.AddCategoryAsync(category);
                categories.Add(category);
                FilterCategories();
                newCategoryName = string.Empty;
            }
        }

        private async Task DeleteCategory(Guid categoryId)
        {
            await StationService.DeleteCategoryAsync(categoryId);
            categories.RemoveAll(c => c.CategoryId == categoryId);
            FilterCategories();
        }

        public void Dispose()
        {
            AppStateService.OnStationChanged -= HandleStationChanged;
        }
    }
}
