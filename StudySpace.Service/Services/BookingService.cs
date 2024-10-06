using Microsoft.EntityFrameworkCore;
using StudySpace.Common;
using StudySpace.Common.Enums;
using StudySpace.Data.Helper;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IBookingService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetAllBookingInSup(int storeId);
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(CreateBookingRequestModel booking);
    }

    public class BookingService : IBookingService
    {
        private readonly UnitOfWork _unitOfWork;
        public BookingService()
        {
            _unitOfWork = new UnitOfWork();
        }

        public async Task<IBusinessResult> GetAll()
        {
            try
            {
                #region Business rule
                #endregion

                var objs = await _unitOfWork.BookingRepository.GetAllAsync();

                if (objs == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, objs);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllBookingInSup(int storeId)
        {
            try
            {
                var store = _unitOfWork.StoreRepository.GetById(storeId);
                if (store == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }
                var listRoom = _unitOfWork.RoomRepository.FindByCondition(x => x.StoreId == storeId).ToList();
                var result = new List<BookingInSupModel>();
                foreach (var room in listRoom) 
                { 
                    var booking  = _unitOfWork.BookingRepository.FindByCondition(b=>b.RoomId == room.Id).ToList();
                    foreach (var item in booking)
                    {

                        var user = _unitOfWork.AccountRepository.GetById(item.UserId ?? 0);
                        var bookingModel = new BookingInSupModel
                        {
                            RoomName = room.RoomName,
                            BookingDate = item.BookingDate?.Date,
                            StartTime = item.StartTime?.TimeOfDay,
                            EndTime = item.EndTime?.TimeOfDay,
                            Fee = item.Fee,
                            Status = item.Status ,
                            UserName = user.Name
                        };
                        result.Add(bookingModel);
                    }
                }
                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG,result);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);


            }
        }

        public async Task<IBusinessResult> GetById(int id)
        {
            try
            {
                var obj = await _unitOfWork.BookingRepository.GetBookingDetails().FirstOrDefaultAsync(b => b.Id == id);

                
                if (obj == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    var bookingDetails = new BookingDetailsResponseDTO
                    {
                        Id = id,
                        UserId = obj.UserId,
                        RoomId = obj.RoomId,
                        StartTime = obj.StartTime,
                        EndTime = obj.EndTime,
                        Status = obj.Status,
                        Fee = obj.Fee,
                        BookingDate = obj.BookingDate,
                        PaymentMethod = obj.PaymentMethod,
                        Checkin = obj.Checkin,
                        Note = obj.Note,
                        FeedbackIds = obj.Feedbacks.Select(f => f.Id).ToList(),
                        TransactionId = obj.Transactions.Select(t => t.Id).ToList(),
                    };
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, bookingDetails);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var obj = await _unitOfWork.BookingRepository.GetByIdAsync(id);
                if (obj != null)
                {

                    var result = await _unitOfWork.BookingRepository.RemoveAsync(obj);
                    if (result)
                    {
                        return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
                    }
                }
                else
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> Save(CreateBookingRequestModel booking)
        {
            try
            {
                var userExisted = await _unitOfWork.AccountRepository.GetByIdAsync(booking.UserId.Value);
                if(userExisted == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid UserID");
                }

                var roomExisted = await _unitOfWork.RoomRepository.GetByIdAsync(booking.RoomId.Value);
                if(roomExisted == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid RoomID");
                }

                var newBooking = new Booking
                {
                    UserId = booking.UserId,
                    RoomId = booking.RoomId,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    Status = StatusBookingEnums.PENDING.ToString(),
                    Fee = booking.Fee,
                    BookingDate = DateTime.Now,
                    PaymentMethod = booking.PaymentMethod,
                    Checkin = false,
                    Note = booking.Note
                };

                _unitOfWork.BookingRepository.PrepareCreate(newBooking);

                var result = await _unitOfWork.BookingRepository.SaveAsync();
                if(result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newBooking);
                } else
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
    }
}
