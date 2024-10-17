using Google.Apis.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Org.BouncyCastle.Utilities;
using StudySpace.Common;
using StudySpace.Common.Enums;
using StudySpace.Data.Models;
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
        Task<IBusinessResult> GetPaymentDetailsPayOS(int transactionID);
        Task<IBusinessResult> CancelPayment(long bookingId, string cancelReason);
    }

    public class PaymentService : IPaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly static string cancelURL = "http://localhost:3000/payment-cancel";
        private readonly static string returnURL = "http://localhost:3000/payment-success";

        private readonly string _clientID;
        private readonly string _apiKey;
        private readonly string _checkSum;

        public PaymentService(IConfiguration configuration)
        {
            _unitOfWork = new UnitOfWork();
            _clientID = configuration["PayOS:ClientID"];
            _apiKey = configuration["PayOS:ApiKey"];
            _checkSum = configuration["PayOS:ChecksumKey"];
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

                var existingTransaction = _unitOfWork.TransactionRepository.GetAllTransactions()
                                    .FirstOrDefault(t => t.UserId == bookingExisted.UserId && t.BookingId == request.BookingId);

                if (existingTransaction != null)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "A transaction already exists!");
                }

                if (bookingExisted.Status != StatusBookingEnums.NONE.ToString())
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Booking already has a payment transaction!");
                }

                var bookingDetails = _unitOfWork.BookingRepository.GetBookingDetails()
                                                                  .FirstOrDefault(b => b.Id == request.BookingId);

                bookingExisted.Status = StatusBookingEnums.PENDING.ToString();
                bookingExisted.PaymentMethod = "PayOS";
                _unitOfWork.BookingRepository.PrepareUpdate(bookingExisted);

                var results = await _unitOfWork.BookingRepository.SaveAsync();

                if (results <= 0)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Failed to update booking status!");
                }

                PayOS payOS = new PayOS(_clientID, _apiKey, _checkSum);
                ItemData room = new ItemData(bookingDetails.Room.RoomName, 1, (int) bookingExisted.Fee);
                List<ItemData> items = new List<ItemData>();
                items.Add(room);

                PaymentData paymentData = new PaymentData(bookingExisted.Id, request.Amount, request.Description, items, cancelURL, returnURL);
                CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

                if (createPayment == null)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Create Payment link fail !");
                }

                var trans = new StudySpace.Data.Models.Transaction
                {
                    UserId = bookingExisted.UserId,
                    BookingId = request.BookingId,
                    PackageId = null,
                    StoreId = null,
                    Date = DateTime.Now,
                    Amount = request.Amount,
                    PaymentCode = createPayment.orderCode.ToString(),
                    PaymentLink = createPayment.paymentLinkId,
                    PaymentStatus = createPayment.status,
                    PaymentDate = DateTime.Now,
                    PaymentUrl = createPayment.checkoutUrl
                };

                var resultTrans = await _unitOfWork.TransactionRepository.CreateAsync(trans);

                if (resultTrans <= 0)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Fail in saving Transaction !");
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, createPayment);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetPaymentDetailsPayOS(int transactionID)
        {
            try
            {
                var transactions = await _unitOfWork.TransactionRepository.GetByIdAsync(transactionID);
                if (transactions == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Transaction not found !");
                }

                long paymentCode = long.Parse(transactions.PaymentCode);

                PayOS payOS = new PayOS(_clientID, _apiKey, _checkSum);

                PaymentLinkInformation paymentLinkInformation = await payOS.getPaymentLinkInformation(paymentCode);

                if(paymentLinkInformation == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Not found link information or have not created payOS link yet !");
                } else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, paymentLinkInformation);
                }
            } 
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> CancelPayment(long bookingId, string cancelReason)
        {
            try
            {
                PayOS payOS = new PayOS(_clientID, _apiKey, _checkSum);

                PaymentLinkInformation cancelledPaymentLinkInfo = await payOS.cancelPaymentLink(bookingId, cancelReason);

                if (cancelledPaymentLinkInfo == null)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Wrong bookingID");
                }

                return new BusinessResult(Const.SUCCESS_BOOKED, "Cancel Payment Success !", cancelledPaymentLinkInfo);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
    }
}
