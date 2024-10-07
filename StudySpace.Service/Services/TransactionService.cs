using StudySpace.Common;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;

namespace StudySpace.Service.Services
{
    public interface ITransactionService
    {
        Task<IBusinessResult> GetAllTransactionInUser(int userId);
    }

    public class TransactionService : ITransactionService
    {
        private readonly UnitOfWork _unitOfWork;

        public TransactionService()
        {
            _unitOfWork ??= new UnitOfWork();
        }
        

        public async Task<IBusinessResult> GetAllTransactionInUser(int userId)
        {
            try
            {
                var transaction = _unitOfWork.TransactionRepository.FindByCondition(t=>t.UserId == userId).ToList();
                var result = new List<TransactionUserModel>();
                foreach (var transactionUser in transaction)
                {
                    var booking  = _unitOfWork.BookingRepository.GetById(transactionUser.BookingId??0);
                    var trans = new TransactionUserModel
                    {
                        Id = transactionUser.Id,
                        Date = transactionUser.Date,
                        Fee = transactionUser.Amount,
                        PaymentMethod = booking.PaymentMethod,
                        Status = booking.Status
                    };
                    result.Add(trans);
                }
                return new BusinessResult (Const.SUCCESS_READ,Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);

            }
        }

    }
}
