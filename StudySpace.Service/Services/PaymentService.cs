using Google.Apis.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Net.payOS;
using Net.payOS.Types;
using Org.BouncyCastle.Utilities;
using StudySpace.Common;
using StudySpace.Common.Enums;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Helper;
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
        Task<IBusinessResult> CreatePaymentSupplierWithPayOS(CreatePaymentSupplierRequest request);
        Task<IBusinessResult> GetPaymentDetailsPayOS(int transactionID);
        Task<IBusinessResult> CancelPayment(int transactionID, string cancelReason);
        Task ProcessWebhook(WebhookType webhookData);
    }

    public class PaymentService : IPaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly string _cancelURL;
        private readonly string _returnURL;

        private readonly string _cancel_adminURL;
        private readonly string _return_adminURL;

        private readonly string _clientID;
        private readonly string _apiKey;
        private readonly string _checkSum;

        private readonly PayOS _payOS;

        public PaymentService(IConfiguration configuration)
        {
            _unitOfWork = new UnitOfWork();
            _clientID = configuration["PayOS:ClientID"];
            _apiKey = configuration["PayOS:ApiKey"];
            _checkSum = configuration["PayOS:ChecksumKey"];
            _payOS = new PayOS(_clientID, _apiKey, _checkSum);
            _cancelURL = configuration["CancelURL"];
            _returnURL = configuration["ReturnURL"];
            _cancel_adminURL = "http://localhost:3000/payment-cancel";
            _return_adminURL = "http://localhost:3000/payment-success";
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

                var orderCode = OrderCodeHashHelper.GenerateOrderCodeHash(bookingExisted.User.Id, request.BookingId, DateTime.Now);

                ItemData room = new ItemData(bookingDetails.Room.RoomName + " " + DateTime.Now, 1, (int) bookingExisted.Fee);
                List<ItemData> items = new List<ItemData>();
                items.Add(room);

                DateTime expirationDate = DateTime.Now.AddMinutes(30);
                long expiredAt = ((DateTimeOffset)expirationDate).ToUnixTimeMilliseconds();
                PaymentData paymentData = new PaymentData(orderCode, request.Amount, request.Description, items, _cancelURL, _returnURL, expiredAt:expiredAt);
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

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

        public async Task<IBusinessResult> CreatePaymentSupplierWithPayOS(CreatePaymentSupplierRequest request)
        {
            try
            {
                var pack_sup = await _unitOfWork.PackageRepository.GetByIdAsync(request.PackageID);

                var pack = await _unitOfWork.StorePackageRepository.GetAllAsync();
                var sup_pack = pack.Where(r => r.StoreId == request.StoreID && r.PackageId == request.PackageID && r.Status == true).ToList();

                if (sup_pack.Count() > 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Supplier already has package !");
                }

                ItemData package = new ItemData(pack_sup.Name + " " + DateTime.Now, 1, request.Amount);
                List<ItemData> items = new List<ItemData>();
                items.Add(package);

                var orderCode = OrderCodeHashHelper.GenerateOrderCodeHash(request.StoreID, request.PackageID, DateTime.Now);

                DateTime expirationDate = DateTime.Now.AddMinutes(30);
                long expiredAt = ((DateTimeOffset)expirationDate).ToUnixTimeMilliseconds();

                PaymentData paymentData = new PaymentData(orderCode, request.Amount, request.Description, items, _cancel_adminURL, _return_adminURL, expiredAt: expiredAt);
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                if (createPayment == null)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Create Payment link fail !");
                }

                var newSupPack = new StorePackage
                {
                    StoreId = request.StoreID,
                    PackageId = request.PackageID,
                    Fee = request.Amount,
                    StartDate = null,
                    Duration = request.Duration,
                    Status = false
                };

                var resultSupPackTrans = await _unitOfWork.StorePackageRepository.CreateAsync(newSupPack);

                if (resultSupPackTrans <= 0)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Fail in saving StorePackage !");
                }

                var trans = new StudySpace.Data.Models.Transaction
                {
                    UserId = null,
                    BookingId = null,
                    PackageId = request.PackageID,
                    StoreId = request.StoreID,
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

                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(paymentCode);

                if(paymentLinkInformation == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Not found link information or have not created payOS link yet !");
                } 
                else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, paymentLinkInformation);
                }
            } 
            catch (Exception ex)
            {
                throw new Exception(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace + "\n" + _apiKey + " \n"+  _checkSum +
                    "\n" + _clientID);
            }
        }

        public async Task<IBusinessResult> CancelPayment(int transactionID, string? cancelReason)
        {
            try
            {
                var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(transactionID);
                if (transaction == null)
                {
                    return new BusinessResult(Const.FAIL_BOOKING, "Transaction does not existed !");
                }

                long paymentCode = long.Parse(transaction.PaymentCode);
                

                PaymentLinkInformation cancelledPaymentLinkInfo =  await _payOS.getPaymentLinkInformation(paymentCode);

                if (string.IsNullOrEmpty(cancelReason))
                {
                    await _payOS.cancelPaymentLink(paymentCode);
                } else
                {
                    await _payOS.cancelPaymentLink(paymentCode, cancelReason);
                }

                transaction.PaymentStatus = StatusBookingEnums.CANCELED.ToString();
                transaction.PaymentDate = DateTime.Now;
                transaction.PaymentUrl = null;

                var result = await _unitOfWork.TransactionRepository.UpdateAsync(transaction);
                if(result <= 0)
                {
                    return new BusinessResult(Const.FAIL_UDATE, "Fail at update transactions");
                }

                return new BusinessResult(Const.SUCCESS_BOOKED, "Cancel Payment Success !");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task ProcessWebhook(WebhookType webhookData)
        {
            try
            {
                _payOS.verifyPaymentWebhookData(webhookData);
                var orderCode = webhookData.data.orderCode;
                var trans = await _unitOfWork.TransactionRepository.GetAllAsync();

                if (webhookData.code == "00")
                {
                    var allTrans = await _unitOfWork.TransactionRepository.GetAllAsync();
                    var whoTrans = allTrans.Where(r => r.StoreId != null && r.PaymentCode == orderCode.ToString()).ToList();

                    if (whoTrans.Count() > 0)
                    {
                        var storeTrans = trans.FirstOrDefault(r => r.PaymentCode == orderCode.ToString());

                        storeTrans.PaymentStatus = "PAID";
                        storeTrans.PaymentDate = DateTime.Now;
                        storeTrans.PaymentUrl = null;
                        
                        await _unitOfWork.TransactionRepository.UpdateAsync(storeTrans);

                        var store_pack = await _unitOfWork.StorePackageRepository.GetAllAsync();

                        var this_store_pack = store_pack.Where(r => r.StoreId == storeTrans.StoreId && r.PackageId == storeTrans.PackageId && r.StartDate == null).FirstOrDefault();
                        
                        this_store_pack.StartDate = DateTime.Now;
                        this_store_pack.EndDate = DateTime.Now.AddMonths((int) this_store_pack.Duration.Value);
                        this_store_pack.Status = true;

                        await _unitOfWork.StorePackageRepository.UpdateAsync(this_store_pack);
                    }
                    else
                    {
                        var cusTrans = trans.FirstOrDefault(r => r.PaymentCode == orderCode.ToString());
                        cusTrans.PaymentDate = DateTime.Now;
                        cusTrans.PaymentStatus = "PAID";
                        cusTrans.PaymentUrl = null;

                        await _unitOfWork.TransactionRepository.UpdateAsync(cusTrans);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing webhook: {ex.Message}");
            }
        }

    }
}
