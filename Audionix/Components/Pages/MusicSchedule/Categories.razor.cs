using Audionix.Data.StationLog;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Categories
    {
        private List<Category> categories = new();
        private List<Category> filteredCategories = new();
        private List<Station> stations = new();
        private string newCategoryName;
        private Guid selectedStationId;
        [Inject]
        private AppStateService appStateService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            stations = await DbContext.Stations.ToListAsync();
            categories = await DbContext.Categories.ToListAsync();
            if (stations.Any())
            {
                selectedStationId = stations.First().StationId;
                FilterCategories();
            }
        }

        private void FilterCategories()
        {
            filteredCategories = categories.Where(c => c.StationId == selectedStationId).ToList();
        }
        private async Task OnStationChanged(Guid stationId)
        {
            appStateService.station = stations.FirstOrDefault(s => s.StationId == stationId);
            FilterCategories();
        }

        private async Task AddCategory()
        {
            var category = new Category { CategoryName = newCategoryName, StationId = selectedStationId, CategoryId = Guid.NewGuid() };
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
    }
}
