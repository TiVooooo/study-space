using Google.Apis.Http;
using Microsoft.Extensions.Configuration;
using StudySpace.Common;
using StudySpace.Common.Enums;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IPaymentService
    {

    }

    public class PaymentService : IPaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public PaymentService(IConfiguration configuration)
        {
            _unitOfWork = new UnitOfWork();
            _configuration = configuration;
        }

        //public async Task<IBusinessResult> CreatePaymentWithPayOS(CreatePaymentRequest request)
        //{
        //    try
        //    {
        //        var bookingExisted = await _unitOfWork.BookingRepository.GetByIdAsync(request.BookingId);
        //        if (bookingExisted == null)
        //        {
        //            return new BusinessResult(Const.WARNING_NO_DATA, "No booking was found !");
        //        }

        //        bookingExisted.Status = StatusBookingEnums.PENDING.ToString();
        //        bookingExisted.PaymentMethod = "PayOS";
        //        _unitOfWork.BookingRepository.PrepareUpdate(bookingExisted);
        //        var result = await _unitOfWork.BookingRepository.SaveAsync();

        //        var payload = new
        //        {
        //            client_id = _configuration["PayOS:ClientId"],
        //            order_id = bookingExisted.Id.ToString(),
        //            amount = bookingExisted.Fee,
        //            currency = "VND",
        //            description = request.Description,
        //            return_url = $"{_configuration["AppSettings:BaseUrl"]}/api/payment/return",
        //        };

                
        //    } 
        //    catch (Exception ex)
        //    {
        //        return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
        //    }
        //}
    }
}
