﻿using AutoMapper;
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
        Task<IBusinessResult> Save(int storeId, CreateAmityRequestModel model);
        Task<IBusinessResult> Update(int amityId, int storeId, CreateAmityRequestModel model);
        Task<IBusinessResult> UnactiveAmity(int amityId);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> GetBySupId(int supId);

        Task<IBusinessResult> DeleteAmityInRoom(int roomId, int amityID);
       Task<IBusinessResult> AddAmityToRoom(int roomId, int amityID, int quantity);
        Task<IBusinessResult> UpdateQuantityAmityInRoom(int roomId, int amityID, int quantity);


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

        public async Task<IBusinessResult> Save(int storeId, CreateAmityRequestModel model)
        {
            try
            {
                var existedStore = await _unitOfWork.StoreRepository.FindByConditionAsync(c => c.Id == storeId);

                if (existedStore.Count() == 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Store is not existed !");
                }

                var newAmity = new Amity
                {
                    Name = model.Name,
                    Type = model.Type,
                    Status = true,
                    Quantity = model.Quantity,
                    Description = model.Description,
                    StoreId = storeId
                };

                _unitOfWork.AmityRepository.PrepareCreate(newAmity);

                int result = await _unitOfWork.AmityRepository.SaveAsync();

                var amityRes = new AmitiesDetailsResponse
                {
                    AmityId = newAmity.Id,
                    AmityName = newAmity.Name,
                    AmityType = newAmity.Type,
                    AmityStatus = newAmity.Status == true ? "Active" : "Inactive",
                    Quantity = newAmity.Quantity,
                    Description = newAmity.Description,
                };

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, amityRes);
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

        public async Task<IBusinessResult> Update(int amityId, int storeId, CreateAmityRequestModel model)
        {
            try
            {
                var existed = await _unitOfWork.AmityRepository.FindByConditionAsync(c => c.Id == amityId && c.StoreId == storeId);
                if (existed.Count() == 0)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Amity not found or Store does not have permission");
                }

                var updatedAmity = await _unitOfWork.AmityRepository.GetByIdAsync(amityId);

                updatedAmity.Name = model.Name;
                updatedAmity.Type = model.Type;
                updatedAmity.Quantity = model.Quantity;
                updatedAmity.Description = model.Description;

                _unitOfWork.AmityRepository.PrepareUpdate(updatedAmity);

                int result = await _unitOfWork.AmityRepository.SaveAsync();

                var amityRes = new AmitiesDetailsResponse
                {
                    AmityId = updatedAmity.Id,
                    AmityName = updatedAmity.Name,
                    AmityType = updatedAmity.Type,
                    AmityStatus = updatedAmity.Status == true ? "Active" : "Inactive",
                    Quantity = updatedAmity.Quantity,
                    Description = updatedAmity.Description,
                };

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UDATE, Const.SUCCESS_UDATE_MSG, amityRes);
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
                    return new BusinessResult(Const.WARNING_NO_DATA, "Store not found");
                }

                var amities = await _unitOfWork.AmityRepository.GetAllAmitiesByStoreId(supId);

                var amitiesDetails = amities.Select(a => new AmitiesDetailsResponse
                {
                    AmityId = a.Id,
                    AmityName = a.Name,
                    AmityType = a.Type,
                    AmityStatus = a.Status == true ? "Active" : "Inactive",
                    Quantity = a.Quantity,
                    Description = a.Description
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

        public async Task<IBusinessResult> DeleteAmityInRoom (int roomId, int amityID)
        {
            try
            {
                var roomAmity = _unitOfWork.RoomAminitiesRepository.FindByCondition(r => r.AmitiesId == amityID && r.RoomId == roomId).FirstOrDefault();

                _unitOfWork.RoomAminitiesRepository.Remove(roomAmity);

                var amity = _unitOfWork.AmityRepository.GetById(roomAmity.AmitiesId);

                amity.Quantity += roomAmity.Quantity;

                _unitOfWork.AmityRepository.PrepareUpdate(amity);
                _unitOfWork.AmityRepository.Save();
                return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
            }

            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> AddAmityToRoom(int roomId, int amityID, int quantity)
        {
            try
            {

                var room = _unitOfWork.RoomRepository.GetById (roomId);
                var amity = _unitOfWork.AmityRepository.GetById(amityID);
                if (amity.Quantity >= quantity)
                {
                    amity.Quantity -= quantity;
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE, "There is not enough Amity in stock!");
                }

                _unitOfWork.AmityRepository.PrepareUpdate(amity);

                var existedRoomAmity = await _unitOfWork.RoomAminitiesRepository.FindByConditionAsync(ra => ra.RoomId == roomId && ra.AmitiesId == amityID);
                if(existedRoomAmity.Count() != 0)
                {
                    var old = existedRoomAmity.FirstOrDefault();
                    old.Quantity += quantity;
                    _unitOfWork.RoomAminitiesRepository.PrepareUpdate(old);
                } else
                {
                    var amityRoom = new RoomAmity
                    {
                        RoomId = roomId,
                        AmitiesId = amityID,
                        Quantity = quantity
                    };
                    _unitOfWork.RoomAminitiesRepository.PrepareCreate(amityRoom);
                }
                
                await _unitOfWork.RoomAminitiesRepository.SaveAsync();
                _unitOfWork.AmityRepository.PrepareUpdate(amity);
                var result = _unitOfWork.AmityRepository.Save();

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
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
        
        public async Task<IBusinessResult> UpdateQuantityAmityInRoom(int roomId, int amityID, int quantity)
        {
            try
            {
                if(quantity < 0)
                {
                    return new BusinessResult(Const.FAIL_UDATE, "Invalid Quantity");
                }

                var room = _unitOfWork.RoomRepository.GetById (roomId);
                var amity = _unitOfWork.AmityRepository.GetById(amityID);
                var existedRoomAmity = await _unitOfWork.RoomAminitiesRepository.FindByConditionAsync(ra => ra.RoomId == roomId && ra.AmitiesId == amityID);

                if (existedRoomAmity.Count() == 0)
                {
                    return new BusinessResult(Const.FAIL_UDATE, "There are not any amities that your input in your room input !");
                }

                var old = existedRoomAmity.FirstOrDefault();
                var chenhLech = Math.Abs(old.Quantity.Value - quantity);

                if (old.Quantity < quantity)
                {
                    if (amity.Quantity >= quantity)
                    {
                        amity.Quantity -= chenhLech;
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UDATE, "There is not enough Amity in stock!");
                    }
                } else
                {
                    amity.Quantity += chenhLech;
                }

                old.Quantity = quantity;

                _unitOfWork.RoomAminitiesRepository.PrepareUpdate(old);
                await _unitOfWork.RoomAminitiesRepository.SaveAsync();
 
                _unitOfWork.AmityRepository.PrepareUpdate(amity);
                var result = await _unitOfWork.AmityRepository.SaveAsync();

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UDATE, Const.SUCCESS_UDATE_MSG);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UDATE, Const.FAIL_UDATE_MSG);
                }   
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

    }
}
