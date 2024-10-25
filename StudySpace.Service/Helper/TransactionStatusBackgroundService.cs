using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudySpace.Data.UnitOfWork;

namespace StudySpace.Service.Helper
{
    public class TransactionStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UnitOfWork _unitOfWork;

        public TransactionStatusBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _unitOfWork = new UnitOfWork();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {

                    var currentDate = DateTime.Now;

                    var expiredTransactions = await _unitOfWork.StorePackageRepository
                        .GetAllAsync();

                    var expired = expiredTransactions.Where(t => t.EndDate <= currentDate && t.Status != false);

                    foreach (var transaction in expired)
                    {
                        transaction.Status = false;
                        _unitOfWork.StorePackageRepository.PrepareUpdate(transaction);
                    }

                    await _unitOfWork.StorePackageRepository.SaveAsync();
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
