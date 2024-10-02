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

    }
}
