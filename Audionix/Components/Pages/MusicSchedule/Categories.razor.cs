using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Categories
    {
        private List<Category> categories = new();
        private List<Category> filteredCategories = new();
        private string? newCategoryName;

        protected override async Task OnInitializedAsync()
        {
            categories = await DbContext.Categories.ToListAsync();
            FilterCategories();
            AppStateService.OnStationChanged += HandleStationChanged;
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            FilterCategories();
            StateHasChanged();
        }

        private void FilterCategories()
        {
            filteredCategories = categories.Where(c => c.StationId == AppStateService.station.StationId).ToList();
        }

        private async Task AddCategory()
        {
            var category = new Category { CategoryName = newCategoryName, StationId = AppStateService.station.StationId, CategoryId = Guid.NewGuid() };
            DbContext.Categories.Add(category);
            await DbContext.SaveChangesAsync();
            categories.Add(category);
            FilterCategories();
            newCategoryName = string.Empty;
        }

        private async Task DeleteCategory(Guid categoryId)
        {
            var category = await DbContext.Categories.FindAsync(categoryId);
            if (category != null)
            {
                DbContext.Categories.Remove(category);
                await DbContext.SaveChangesAsync();
                categories.Remove(category);
                FilterCategories();
            }
        }
        public void Dispose()
        {
            AppStateService.OnStationChanged -= HandleStationChanged;
        }
    }
}