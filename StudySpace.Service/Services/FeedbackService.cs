using Microsoft.EntityFrameworkCore;
using StudySpace.Common;
using StudySpace.Data.Helper;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IFeedbackService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(Feedback acc);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(FeedbackRequestModel acc);
        Task<IBusinessResult> GetFeedback(int roomId, int pageNumber, int pageSize);


    }

    public class FeedbackService : IFeedbackService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IFirebaseService _firebaseService;
        public FeedbackService(IFirebaseService firebaseService)
        {
            _unitOfWork ??= new UnitOfWork();
            _firebaseService = firebaseService;
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var obj = await _unitOfWork.FeedbackRepository.GetByIdAsync(id);
                if (obj != null)
                {

                    var result = await _unitOfWork.FeedbackRepository.RemoveAsync(obj);
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

        public async Task<IBusinessResult> GetAll()
        {
            try
            {
                #region Business rule
                #endregion

                var objs = await _unitOfWork.FeedbackRepository.GetAllAsync();

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

        public async Task<IBusinessResult> GetFeedback(int roomId, int pageNumber, int pageSize)
        {
            try
            {
                var bookings = _unitOfWork.BookingRepository.FindByCondition(b => b.Room.Id == roomId).ToList();
                var bookingIds = bookings.Select(b => b.Id).ToList();

                var totalFeedback = _unitOfWork.FeedbackRepository.FindByCondition(f => bookingIds.Contains(f.Booking.Id)).Count();

                var totalPages = (int)Math.Ceiling((double)totalFeedback / pageSize);

                var feedbacks = _unitOfWork.FeedbackRepository
                    .FindByCondition(f => bookingIds.Contains(f.Booking.Id))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var list = new List<FeedbackResponseModel>();

                if (feedbacks != null && feedbacks.Any())
                {
                    foreach (var item in feedbacks)
                    {
                        var images = _unitOfWork.ImageFeedbackRepository
                            .FindByCondition(i => i.FeedbackId == item.Id)
                            .Select(i => i.ImageUrl)
                            .ToList();

                        var user = _unitOfWork.AccountRepository.GetById(item.UserId ?? 0);
                        var bookedDate = _unitOfWork.BookingRepository
                            .FindByCondition(b => b.UserId == user.Id && b.RoomId == roomId)
                            .Select(b => b.BookingDate)
                            .FirstOrDefault();

                        var feedback = new FeedbackResponseModel
                        {
                            Avatar = user.AvatarUrl,
                            ReviewText = item.ReviewText,
                            Images = images,
                            BookingDate = bookedDate,
                            Star = item.Rating,
                            UserName = user.Name
                        };

                        list.Add(feedback);

                       
                    }
                    var result = new FeedbackModel
                    {
                        FeedbackResponses = list,
                        TotalPage = totalPages,
                        TodalFeedback = totalFeedback
                    };
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_CREATE_MSG, result);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }
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
                #region Business rule
                #endregion


                var obj = await _unitOfWork.FeedbackRepository.GetByIdAsync(id);

                if (obj == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, obj);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }


        public async Task<IBusinessResult> Save(FeedbackRequestModel feedback)
        {
            try
            {
                var checkHasBooking = _unitOfWork.BookingRepository.FindByCondition(b => b.UserId == feedback.UserId && b.RoomId == feedback.BookingId).FirstOrDefault();
                if (checkHasBooking == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                var newFeedback = new Feedback
                {
                    UserId = feedback.UserId,
                    BookingId = feedback.BookingId,
                    ReviewDate = DateTime.Now,
                    ReviewText = feedback.ReviewText,
                    Rating = feedback.Rating
                };

                _unitOfWork.FeedbackRepository.PrepareCreate(newFeedback);

                var imageUrls = feedback.Files;
                if (imageUrls != null)
                {
                    foreach (var file in imageUrls)
                    {
                        var newFeedbackImage = new ImageFeedback
                        {
                            Feedback = newFeedback
                        };
                        var imagePath = FirebasePathName.RATING + Guid.NewGuid().ToString();
                        var imageUploadResult = await _firebaseService.UploadImageToFirebaseAsync(file, imagePath);
                        newFeedbackImage.ImageUrl = imageUploadResult;
                        _unitOfWork.ImageFeedbackRepository.PrepareCreate(newFeedbackImage);
                    }

                }
                _unitOfWork.FeedbackRepository.Save();
                _unitOfWork.ImageFeedbackRepository.Save();



                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);

            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> Update(Feedback acc)
        {
            try
            {

                int result = await _unitOfWork.FeedbackRepository.UpdateAsync(acc);
                if (result > 0)
                {
                    return new BusinessResult(Const.FAIL_UDATE, Const.SUCCESS_UDATE_MSG);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UDATE, Const.FAIL_UDATE_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }
    }
}