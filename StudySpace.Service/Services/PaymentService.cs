using Google.Apis.Http;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Org.BouncyCastle.Utilities;
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
        Task<IBusinessResult> CreatePaymentWithPayOS(CreatePaymentRequest request);
    }

    public class PaymentService : IPaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly static string cancelURL = "";
        private readonly static string returnURL = "";

        public PaymentService(IConfiguration configuration)
        {
            _unitOfWork = new UnitOfWork();
            _configuration = configuration;
        }

        public async Task<IBusinessResult> CreatePaymentWithPayOS(CreatePaymentRequest request)
        {
            try
            {
                var bookingExisted = await _unitOfWork.BookingRepository.GetByIdAsync(request.BookingId);
                if (bookingExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "No booking was found !");
                }

                bookingExisted.Status = StatusBookingEnums.PENDING.ToString();
                bookingExisted.PaymentMethod = "PayOS";
                _unitOfWork.BookingRepository.PrepareUpdate(bookingExisted);
                var result = await _unitOfWork.BookingRepository.SaveAsync();

                var payload = new
                {
                    client_id = _configuration["PayOS:ClientID"],
                    order_id = bookingExisted.Id.ToString(),
                    amount = bookingExisted.Fee,
                    currency = "VND",
                    description = request.Description,
                    return_url = $"{_configuration["AppSettings:BaseUrl"]}/api/payment/return",
                };

                PayOS payOS = new PayOS(_configuration["PayOS:ClientID"], _configuration["PayOS:ApiKey"], _configuration["PayOS:ChecksumKey"]);
                ItemData room = new ItemData("RoomName", 1, (int)bookingExisted.Fee.Value);
                List<ItemData> items = new List<ItemData>();
                items.Add(room);

                PaymentData paymentData = new PaymentData(bookingExisted.Id, (int)bookingExisted.Fee.Value, request.Description, items, cancelURL, returnURL);
                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, createPayment);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
    }
}
