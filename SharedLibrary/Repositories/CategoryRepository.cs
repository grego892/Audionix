﻿using SharedLibrary.Data;
using SharedLibrary.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace SharedLibrary.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public CategoryRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Category>> GetCategoriesAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Categories.Where(c => c.StationId == stationId).ToListAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var category = await context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Categories.FindAsync(categoryId);
        }

        public async Task<List<string>> GetCategoryNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Categories.Select(c => c.CategoryName!).ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var categories = new List<Category>();

            foreach (var patternId in musicPatterns)
            {
                var patternCategories = await context.PatternCategories
                    .AsNoTracking()
                    .Where(pc => pc.MusicPatternId == patternId)
                    .Include(pc => pc.Category)
                    .ToListAsync();

                foreach (var patternCategory in patternCategories)
                {
                    if (patternCategory.Category != null)
                    {
                        categories.Add(patternCategory.Category);
                    }
                }
            }

            return categories;
        }
    }
}