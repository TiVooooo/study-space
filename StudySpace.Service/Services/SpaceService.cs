using StudySpace.Data.UnitOfWork;
using StudySpace.Data.Models;
using StudySpace.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudySpace.Common;
using Microsoft.EntityFrameworkCore;
using StudySpace.Service.BusinessModel;

namespace StudySpace.Service.Services
{
    public interface ISpaceService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(Space space);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(Space space);
        Task<IBusinessResult> GetSpacePopular();
        Task<IBusinessResult> GetAllSpace();


    }

    public class SpaceService : ISpaceService
    {
        private readonly UnitOfWork _unitOfWork;

        public SpaceService()
        {
            _unitOfWork ??= new UnitOfWork();

        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var payment = await _unitOfWork.SpaceRepository.GetByIdAsync(id);
                if (payment != null)
                {

                    var result = await _unitOfWork.SpaceRepository.RemoveAsync(payment);
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

                var payments = await _unitOfWork.SpaceRepository.GetAllAsync();

                if (payments == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, payments);
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


                var payment = await _unitOfWork.SpaceRepository.GetByIdAsync(id);

                if (payment == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, payment);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }


        public async Task<IBusinessResult> Save(Space space)
        {
            try
            {
                int result = await _unitOfWork.SpaceRepository.CreateAsync(space);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> Update(Space space)
        {
            try
            {

                int result = await _unitOfWork.SpaceRepository.UpdateAsync(space);
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



        public async Task<IBusinessResult> GetHighestRoomPriceBySpaceId(int spaceId)
        {
            try
            {
                // Fetch the space with its rooms and get the highest PricePerHour
                var highestPrice = await _unitOfWork.RoomRepository
                    .FindByConditionv2(r => r.SpaceId == spaceId && r.PricePerHour.HasValue)
                    .MaxAsync(r => r.PricePerHour.Value); // Get the maximum PricePerHour

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, highestPrice);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }


        public async Task<IBusinessResult> GetSpacePopular()
        {
            try
            {
                var spaces = _unitOfWork.SpaceRepository
                    .FindByCondition(s => s.SpaceName == "Coffee Space" || s.SpaceName == "Library Space" || s.SpaceName == "Meeting Room")
                    .ToList(); // Load all spaces first

                var spacePopularList = new List<SpacePopularModel>();

                foreach (var space in spaces)
                {
                    var highestPriceResult = await GetHighestRoomPriceBySpaceId(space.Id);
                    double highestPrice = highestPriceResult.Data != null ? (double)highestPriceResult.Data : 0;

                    var spacePopular = new SpacePopularModel
                    {
                        Description = space.Description,
                        Status = space.Status,
                        Type = space.SpaceName,
                        PricePerHour = highestPrice
                    };

                    spacePopularList.Add(spacePopular);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, spacePopularList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }


        public async Task<IBusinessResult> GetAllSpace()
        {
            try
            {
                var spaces = _unitOfWork.SpaceRepository.GetAll().Select(s=>s.SpaceName).Distinct().ToList();
                spaces.Insert(0,"All");
                return new BusinessResult(Const.SUCCESS_READ,Const.SUCCESS_READ_MSG, spaces);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);

            }

        }
    }
}
