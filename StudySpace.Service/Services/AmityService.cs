using AutoMapper;
using StudySpace.Common;
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
    public interface IAmityService
    {
        Task<IBusinessResult> GetAllAmities();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Save(CreateAmityRequestModel model);
        Task<IBusinessResult> Update(int amityId, CreateAmityRequestModel model);
        Task<IBusinessResult> UnactiveAmity(int amityId);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> GetBySupId(int supId);
    }
    public class AmityService : IAmityService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AmityService(IMapper mapper)
        {
            _unitOfWork ??= new UnitOfWork();

            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetById(int id)
        {
            try
            {
                var obj = await _unitOfWork.AmityRepository.GetByIdAsync(id);

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
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllAmities()
        {
            try
            {
                var amities = _unitOfWork.AmityRepository.GetAll();

                var name = amities.Select(a => a.Name).ToList();
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, name);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> Save(CreateAmityRequestModel model)
        {
            try
            {
                var newAmity = new Amity
                {
                    Name = model.Name,
                    Type = model.Type,
                    Status = true,
                    Quantity = model.Quantity,
                    Description = model.Description
                };

                _unitOfWork.AmityRepository.PrepareCreate(newAmity);

                int result = await _unitOfWork.AmityRepository.SaveAsync();

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newAmity);
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

        public async Task<IBusinessResult> Update(int amityId, CreateAmityRequestModel model)
        {
            try
            {
                var updatedAmity = await _unitOfWork.AmityRepository.GetByIdAsync(amityId);
                if (updatedAmity == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Amity not found.");
                }

                updatedAmity.Name = model.Name;
                updatedAmity.Type = model.Type;
                updatedAmity.Quantity = model.Quantity;
                updatedAmity.Description = model.Description;

                _unitOfWork.AmityRepository.PrepareUpdate(updatedAmity);

                int result = await _unitOfWork.AmityRepository.SaveAsync();

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UDATE, Const.SUCCESS_UDATE_MSG, updatedAmity);
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

        public async Task<IBusinessResult> UnactiveAmity(int amityId)
        {
            try
            {
                var amityUnactive = await _unitOfWork.AmityRepository.GetByIdAsync(amityId);

                if (amityUnactive == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                amityUnactive.Status = !amityUnactive.Status;

                int result = await _unitOfWork.AmityRepository.UpdateAsync(amityUnactive);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UNACTIVATE, Const.SUCCESS_UNACTIVATE_MSG);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UNACTIVATE, Const.FAIL_UNACTIVATE_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var amity = await _unitOfWork.AmityRepository.GetByIdAsync(id);
                if (amity != null)
                {

                    var result = await _unitOfWork.AmityRepository.RemoveAsync(amity);
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

        public async Task<IBusinessResult> GetBySupId(int supId)
        {
            try
            {
                var existedStore = await _unitOfWork.StoreRepository.GetByIdAsync(supId);
                if (existedStore == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                var amities = await _unitOfWork.AmityRepository.GetAllAmitiesByStoreId(supId);

                var amitiesDetails = amities.Select(a => new AmitiesDetailsResponse
                {
                    AmityId = a.Id,
                    AmityName = a.Name,
                    AmityType = a.Type,
                    AmityStatus = a.Status == true ? "Active" : "Inactive",
                    Quantity = a.Quantity
                }).ToList();

                if (amities != null && amities.Count != 0)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, amitiesDetails);
                }
                else
                {
                    return new BusinessResult(Const.SUCCESS_READ, "No amities have been created!");
                }

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
    }
}
