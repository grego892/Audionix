﻿using Audionix.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace Audionix.Services
{
    public class AudionixService : IHostedService
    {
        private readonly AppSettings _appSettings;
        //private readonly AppSettingsService _appSettingsService;

        public AudionixService(AppSettings appSettings, AppSettingsService appSettingsService)
        {
            _appSettings = appSettings;
            //_appSettingsService = appSettingsService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("--- AudionixService - StartAsync() -- Audionix Service starting");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("--- AudionixService - StopAsync() -- Audionix Service stopping");
            return Task.CompletedTask;
        }
    }

}
