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
    public interface IBookingService
    {
        Task<IBusinessResult> CalculateTotalBooking();
    }

    public class BookingService : IBookingService
    {
        private readonly UnitOfWork _unitOfWork;
        public BookingService()
        {
            _unitOfWork = new UnitOfWork();
        }
        public async Task<IBusinessResult> CalculateTotalBooking()
        {
            try
            {
                var doneBookings = _unitOfWork.BookingRepository
                    .FindByCondition(a => a.Checkin == true)
                    .ToList();

                var pendingBookings = _unitOfWork.BookingRepository
                    .FindByCondition(a => a.Checkin == false)
                    .ToList();


                int totalDoneBookings = doneBookings.Count;
                int totalPendingBookings = pendingBookings.Count;

                int totalBookings = totalDoneBookings + totalPendingBookings;

                var response = new
                {
                    TotalDoneBookings = totalDoneBookings,
                    TotalPendingBookings = totalPendingBookings,
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
