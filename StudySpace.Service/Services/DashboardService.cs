using StudySpace.Common;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IDashboardService
    {
        Task<IBusinessResult> CalculateAll();
    }
    
    public class DashboardService : IDashboardService
    {
        private readonly UnitOfWork _unitOfWork;

        public DashboardService()
        {
            _unitOfWork ??= new UnitOfWork();
        }
        public async Task<IBusinessResult> CalculateAll()
        {
            try
            {
                var admins = _unitOfWork.AccountRepository
                    .FindByCondition(a => a.Role.RoleName == "Admin")
                .ToList();

                var users = _unitOfWork.AccountRepository
                    .FindByCondition(a => a.Role.RoleName == "User")
                .ToList();
                
                var stores = _unitOfWork.StoreRepository
                    .GetAll()
                .ToList();

                int totalAdmins = admins.Count;
                int totalUsers = users.Count;
                int totalStores = stores.Count;

                int totalAccounts = totalAdmins + totalUsers + totalStores;

                var transactions = await _unitOfWork.TransactionRepository.GetAllAsync();

                double totalIncomes = transactions.Sum(t => t.Amount ?? 0);

                var bookings = await _unitOfWork.BookingRepository.GetAllAsync();
                int totalBookings = bookings.Count;

                int totalTransactions = transactions.Count;

                var monthlyData = transactions
                    .Where(t => t.Date.HasValue)
                    .GroupBy(t => new
                    {
                        Month = t.Date.Value.Month,
                        Year = t.Date.Value.Year
                    })
                    .Select(group => new
                    {
                        Month = group.Key.Month,
                        Year = group.Key.Year,
                        TotalTransactions = group.Count(),
                        TotalAmount = group.Sum(t => t.Amount ?? 0)
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToList();

                var response = new
                {
                    Accounts = new {
                        TotalAdmins = totalAdmins,
                        TotalUsers = totalUsers,
                        TotalStores = totalStores,
                        TotalAccounts = totalAccounts,
                    },

                    MonthlyIncome = monthlyData.Select(m => new
                    {
                        Month = $"{m.Month}/{m.Year}",
                        TotalTransactions = m.TotalTransactions,
                        TotalAmount = m.TotalAmount
                    }).ToList(),

                    TotalIncome = totalIncomes,

                    TotalTransactions = totalTransactions,

                    TotalBookings = totalBookings
            };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
    }
}
