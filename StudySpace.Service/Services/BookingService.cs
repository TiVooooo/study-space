using StudySpace.Common;
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
        Task<IBusinessResult> GetAllBookingInSup(int storeId);
    }

    public class BookingService : IBookingService
    {
        private readonly UnitOfWork _unitOfWork;
        public BookingService()
        {
            _unitOfWork = new UnitOfWork();
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
    }
}
