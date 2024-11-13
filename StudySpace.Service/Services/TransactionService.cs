using Firebase.Auth;
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
        Task<IBusinessResult> GetTransactionOfBooking(int bookingId);
        Task<IBusinessResult> TransactionOfSupplier(int supID);
        Task<IBusinessResult> GetAllTransaction();

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
                var transaction = _unitOfWork.TransactionRepository.FindByCondition(t => t.UserId == userId)
                    .OrderByDescending(t => t.Date)
                    .ToList();
                var result = new List<TransactionUserModel>();
                foreach (var transactionUser in transaction)
                {
                    var booking = _unitOfWork.BookingRepository.GetById(transactionUser.BookingId ?? 0);
                    var type = "Room";
                    if (transactionUser.BookingId == null)
                    {
                        type = "Package";
                    }
                    var trans = new TransactionUserModel
                    {
                        Id = transactionUser.Id,
                        Date = transactionUser.Date,
                        Fee = transactionUser.Amount,
                        PaymentMethod = booking.PaymentMethod,
                        Status = booking.Status,
                        Type = type,
                        Hastag = transactionUser.PaymentCode
                    };
                    result.Add(trans);
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);

            }
        }

        public async Task<IBusinessResult> GetTransactionOfBooking(int bookingId)
        {
            try
            {
                var tran = _unitOfWork.TransactionRepository.FindByCondition(t => t.BookingId == bookingId).FirstOrDefault();
                if (tran == null)
                {
                    return null;
                }

                var book = _unitOfWork.BookingRepository.GetById(bookingId);
                if (book == null)
                {
                    return null;

                }
                var user = _unitOfWork.AccountRepository.GetById(book.UserId ?? 0);
                var room = _unitOfWork.RoomRepository.GetById(book.RoomId ?? 0);

                var checkInDate = book.StartTime.Value.Date;
                var checkOutDate = book.EndTime.Value.Date;

                var transaction = new TransactionInBooking
                {
                    Avatar = user.AvatarUrl,
                    Email = user.Email,
                    Gender = user.Gender,
                    UserName = user.Name,
                    Fee = tran.Amount,
                    UserAddress = user.Address,
                    BookedDate = book.BookingDate,
                    CheckInDate = checkInDate.ToString("yyyy-MM-dd"),
                    CheckOutDate = checkOutDate.ToString("yyyy-MM-dd"),
                    Start = book.StartTime?.TimeOfDay,
                    Status = book.Status,
                    End = book.EndTime?.TimeOfDay,
                    PaymentMethod = book.PaymentMethod,
                    RoomName = room.RoomName,
                    Hastag = tran.PaymentCode
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, transaction);

            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);

            }
        }


        public async Task<IBusinessResult> TransactionOfSupplier(int supID)
        {
            try
            {
                var rooms = _unitOfWork.BookingRepository.FindByCondition(r => r.UserId == supID).ToList();
                double totalOfBookingRoom = 0;
                var list = new List<TransactionUserModel>();
                foreach (var room in rooms)
                {
                    var bookings = _unitOfWork.BookingRepository.FindByCondition(b => b.RoomId == room.Id).ToList();
                    foreach (var b in bookings)
                    {
                        var transaction = _unitOfWork.TransactionRepository.FindByCondition(b => b.BookingId == b.Id).FirstOrDefault();
                        var booking = _unitOfWork.BookingRepository.GetById(b.Id);
                        var trans = new TransactionUserModel
                        {
                            Id = transaction.Id,
                            Date = transaction.Date,
                            Fee = booking.Fee,
                            PaymentMethod = booking.PaymentMethod,
                            Status = booking.Status,
                            Type = "Room",
                            Hastag = transaction.PaymentCode
                        };
                        list.Add(trans);
                    }
                    totalOfBookingRoom += bookings.Sum(b => b.Fee ?? 0);
                }

                var package = _unitOfWork.TransactionRepository.FindByCondition(b => b.StoreId == supID).ToList();

                foreach (var transaction in package)
                {

                    var trans = new TransactionUserModel
                    {
                        Id = transaction.Id,
                        Date = transaction.Date,
                        Fee = transaction.Amount,
                        PaymentMethod = "PayOS",
                        Status = "PAID",
                        Type = "Package",
                        Hastag = transaction.PaymentCode
                    };
                    list.Add(trans);
                }

                var totalCost = package.Sum(b => b.Amount ?? 0);


                var result = new TotalTransaction
                {
                    Transaction = list,
                    TotalRevenue = totalOfBookingRoom,
                    TotalCost = totalCost,
                };
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);

            }
        }


        public async Task<IBusinessResult> GetAllTransaction()
        {
            try
            {
                var allTrans = _unitOfWork.TransactionRepository.GetAllTransactions();
                var result = new List<GetAllTransactionModel>();
                foreach (var transaction in allTrans)
                {
                    if (transaction.StoreId != null)
                    {
                        var user = _unitOfWork.StoreRepository.GetById(transaction.StoreId ?? 0);

                        var package = _unitOfWork.PackageRepository.GetById(transaction.PackageId ?? 0);
                        var trans = new GetAllTransactionModel
                        {
                            Avatar = user.ThumbnailUrl,
                            Date = transaction.Date,
                            Fee =transaction.Amount,
                            Id = transaction.Id,
                            PaymentMethod = "PayOS",
                            Status = "PAID",
                            Type = "Package",
                            UserName = user.Name,
                            PackageName = package.Name,
                            Hastag = transaction.PaymentCode
                        };
                        result.Add(trans);
                    }
                    else
                    {
                        var user = _unitOfWork.AccountRepository.GetById(transaction.UserId ?? 0);

                        var book = _unitOfWork.BookingRepository.GetById(transaction.BookingId ?? 0);
                        var room = _unitOfWork.RoomRepository.GetById(book.RoomId ?? 0);
                        var trans = new GetAllTransactionModel
                        {
                            Avatar = user.AvatarUrl,
                            Date = book.BookingDate,
                            Fee = book.Fee,
                            Id = transaction.Id,
                            PaymentMethod = book.PaymentMethod,
                            RoomName = room.RoomName,
                            Status = book.Status,
                            Type = "Room",
                            UserName = user.Name,
                            Hastag = transaction.PaymentCode
                        };
                        result.Add(trans);

                    }
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);

            }
        }
    }
}
