using StudySpace.Common;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;

namespace StudySpace.Service.Services
{
    public interface ITransactionService
    {
        Task<IBusinessResult> CalculateStoreIncome(int storeId);
        Task<IBusinessResult> GetAllTransactionInUser(int userId);

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

            var rooms = _unitOfWork.RoomRepository.FindByCondition(r=>r.StoreId == storeId).ToList();
            var bookings = new List<Booking>();

            foreach (var room in rooms) {
                var book = _unitOfWork.BookingRepository.FindByCondition(b=>b.RoomId == room.Id).ToList();
                bookings.AddRange(book);
            }

            var response = new DashboardSupplierModel
            {
                StoreName = store.Name,
                TotalIncome = totalAmount,
                TotalRoom = rooms.Count(),
                TotalBooking = bookings.Count()
            };

            return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
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
