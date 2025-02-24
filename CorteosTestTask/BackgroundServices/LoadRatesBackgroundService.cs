using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorteosTestTask.IService;
using CorteosTestTask.Service;
using Microsoft.Extensions.Hosting;

namespace CorteosTestTask.BackgroundServices
{
    public class LoadRatesBackgroundService : BackgroundService
    {
        private readonly IRateService Rate;
        public LoadRatesBackgroundService(IRateService rateService) => Rate = rateService;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Rate.LoadRate();


                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
