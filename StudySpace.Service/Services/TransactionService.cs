using StudySpace.Common;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface ITransactionService
    {
        Task<IBusinessResult> CalculateStoreIncome(int storeId);
        Task<IBusinessResult> CalculateTotalTransaction();
        Task<IBusinessResult> CalculateMonthlyTransactions();
    }

    public class TransactionService : ITransactionService
    {
        private readonly UnitOfWork _unitOfWork;

        public TransactionService()
        {
            _unitOfWork ??= new UnitOfWork();
        }
        public async Task<IBusinessResult> CalculateStoreIncome(int storeId)
        {
            var store = await _unitOfWork.StoreRepository.GetByIdAsync(storeId);

            if (store == null)
            {
                return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
            }

            var transactions = _unitOfWork.TransactionRepository.FindByCondition(t => t.BookingId != null && t.Booking.Room.StoreId == storeId).ToList();

            double totalAmount = transactions.Sum(t => t.Amount ?? 0);

            var response = new
            {
                StoreName = store.Name,
                TotalIncome = totalAmount
            };

            return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
        }

        public async Task<IBusinessResult> CalculateTotalTransaction()
        {
            try
            {
                var userBookingTransactions = _unitOfWork.TransactionRepository
                    .FindByCondition(t => t.BookingId != null && t.UserId != null)
                    .ToList();

                var supPackageTransactions = _unitOfWork.TransactionRepository
                    .FindByCondition(t => t.PackageId != null && t.StoreId != null)
                    .ToList();

                double totalUserBookingAmount = userBookingTransactions.Sum(t => t.Amount ?? 0);

                double totalSupPackageAmount = supPackageTransactions.Sum(t => t.Amount ?? 0);

                double totalTransactionAmount = totalUserBookingAmount + totalSupPackageAmount;

                var response = new
                {
                    TotalUserBookingAmount = totalUserBookingAmount,
                    TotalSupPackageAmount = totalSupPackageAmount,
                    TotalTransactionAmount = totalTransactionAmount
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> CalculateMonthlyTransactions()
        {
            try
            {
                var transactions = await _unitOfWork.TransactionRepository.GetAllAsync();

                if (transactions == null || !transactions.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "No transactions found.");
                }

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

                var responseList = monthlyData.Select(m => new
                {
                    Month = $"{m.Month}/{m.Year}",
                    TotalTransactions = m.TotalTransactions,
                    TotalAmount = m.TotalAmount
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

    }
}
