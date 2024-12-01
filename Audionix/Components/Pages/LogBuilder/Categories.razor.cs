using Audionix.Models.MusicSchedule;
using Audionix.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Components.Pages.LogBuilder
{
    public partial class Categories : IDisposable
    {
        private List<Category> categories = new();
        private List<Category> filteredCategories = new();
        private string? newCategoryName;
        [Inject] private IStationRepository StationRepository { get; set; }
        [Inject] private ICategoryRepository CategoryRepository { get; set; }

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
                categories = await CategoryRepository.GetCategoriesAsync(AppStateService.station.StationId);
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
                await CategoryRepository.AddCategoryAsync(category);
                categories.Add(category);
                FilterCategories();
                newCategoryName = string.Empty;
            }
        }

        private async Task DeleteCategory(Guid categoryId)
        {
            await CategoryRepository.DeleteCategoryAsync(categoryId);
            categories.RemoveAll(c => c.CategoryId == categoryId);
            FilterCategories();
        }

        public void Dispose()
        {
            AppStateService.OnStationChanged -= HandleStationChanged;
        }
    }
}
