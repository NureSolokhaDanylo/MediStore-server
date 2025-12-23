using System.Collections.Generic;

using Domain.Models;

using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal class AppSettingsRepository(AppDbContext context) :  IAppSettingsRepository
    {
        public Task<AppSettings?> GetAsync()
        {
            return context.AppSettings.SingleAsync()!;
        }
        public async Task UpdateAsync(AppSettings appSettings) => context.AppSettings.Update(appSettings);
        public Task AddAsync(AppSettings appSettings) => context.AppSettings.AddAsync(appSettings).AsTask();

        //{
        //    var exist = context.AppSettings.Single();
        //    appSettings.Id = exist.Id;
        //    context.Entry(exist).CurrentValues.SetValues(appSettings);
        //}
    }
}
       