using StudySpace.Common;
using StudySpace.Data.Models;
using StudySpace.Data.Repository;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StudySpace.Service.Services
{

    public interface IRoomService
    {
        Task<IBusinessResult> GetAll(int pageNumber, int pageSize);
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(Room space);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(Room space);
        Task<IBusinessResult> SearchRooms(int pageNumber, int pageSize, string space, string location, string room, int person);
        Task<IBusinessResult> GetRoomWithCondition(string condition);

    }

    public class RoomService : IRoomService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IFeedbackService _feedbackService;

        public RoomService(IFeedbackService feedbackService)
        {
            _unitOfWork ??= new UnitOfWork();
            _feedbackService = feedbackService;
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var payment = await _unitOfWork.RoomRepository.GetByIdAsync(id);
                if (payment != null)
                {

                    var result = await _unitOfWork.RoomRepository.RemoveAsync(payment);
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

        public async Task<IBusinessResult> GetAll(int pageNumber, int pageSize)
        {
            try
            {
                var rooms = await _unitOfWork.RoomRepository.GetAllRoomsAsync();

                var pagedRooms = rooms.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var list = pagedRooms.Select(r => new RoomModel
                {
                    RoomName = r.RoomName,
                    StoreName = r.Store?.Name,
                    Capacity = r.Capacity,
                    PricePerHour = r.PricePerHour,
                    Description = r.Description,
                    Status = r.Status,
                    Area = r.Area,
                    Type = r.Amities.Any() ? r.Amities.First().Type : null
                }).ToList();

                if (!list.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, list);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetRoomWithCondition(string condition)
        {
            try
            {
                var rooms = _unitOfWork.RoomRepository.FindByCondition(r=>r.Space.SpaceName.Equals(condition)).Take(3).ToList();


                var list = rooms.Select(r => new RoomModel
                {
                    RoomName = r.RoomName,
                    StoreName = r.Store?.Name,
                    Capacity = r.Capacity,
                    PricePerHour = r.PricePerHour,
                    Description = r.Description,
                    Status = r.Status,
                    Area = r.Area,
                    Type = r.Amities.Any() ? r.Amities.First().Type : null
                }).ToList();

                if (!list.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, list);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> SearchRooms(int pageNumber, int pageSize, string space, string location, string room, int person)
        {
            try
            {
                var rooms = await _unitOfWork.RoomRepository.GetAllRoomsAsync();

                if (!string.IsNullOrWhiteSpace(space) || !string.IsNullOrWhiteSpace(location) || !string.IsNullOrWhiteSpace(room) || person == 0)
                {
                    rooms = rooms.Where(r =>
                        (space.Equals("All") || (r.Space != null && r.Space.SpaceName.Contains(space, StringComparison.OrdinalIgnoreCase))) &&
                        (location.Equals("All") || (r.Store != null && r.Store.Address.Contains(location, StringComparison.OrdinalIgnoreCase))) &&
                        (r.Capacity >= person)
                    ).ToList();
                }

                var totalCount = rooms.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var pagedRooms = rooms.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var list = pagedRooms.Select(r => new RoomModel
                {
                    RoomName = r.RoomName,
                    StoreName = r.Store?.Name,
                    Capacity = r.Capacity,
                    PricePerHour = r.PricePerHour,
                    Description = r.Description,
                    Status = r.Status,
                    Area = r.Area,
                    Type = r.Amities.Any() ? r.Amities.First().Type : null
                }).ToList();


                if (!list.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                var result = new GetAllRoomModel
                {
                    Rooms = list,
                    TotalCount = totalPages
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
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
                var room = await _unitOfWork.RoomRepository.GetByIdAsync(id);
                if (room == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }


                var listImageOfRoom = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == id).Select(i => i.ImageUrl).ToList();
                var listAminity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == id).Select(a => a.Name).ToList();
                var type = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == id).Select(a => a.Type).FirstOrDefault();

                var feedbackResult = await _feedbackService.GetFeedback(id);

                var spaceName = _unitOfWork.SpaceRepository.GetById(room.SpaceId); // Use room.SpaceId if that's the FK
                

                var relatedRoomsResult = await GetRoomWithCondition(spaceName.SpaceName);
                if (relatedRoomsResult == null || relatedRoomsResult.Data == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Related room information is not available.");
                }

                var storeName = _unitOfWork.StoreRepository.FindByCondition(s => s.Id == room.StoreId).Select(s => s.Name).FirstOrDefault();
                var storeAddress = _unitOfWork.StoreRepository.FindByCondition(s => s.Id == room.StoreId).Select(s => s.Address).FirstOrDefault();

                var result = new RoomDetailModel
                {
                    RoomName = room.RoomName,
                    StoreName = storeName,
                    Capacity = room.Capacity,
                    PricePerHour = room.PricePerHour,
                    Area = room.Area,
                    HouseRule = room.HouseRule,
                    TypeOfRoom = type,
                    ListImages = listImageOfRoom,
                    Description = room.Description,
                    Address = storeAddress,
                    Aminities = listAminity,
                    Feedbacks = feedbackResult.Data as List<FeedbackResponseModel>,
                    RelatedRoom = relatedRoomsResult.Data as List<RoomModel>
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }



        public async Task<IBusinessResult> Save(Room space)
        {
            try
            {
                int result = await _unitOfWork.RoomRepository.CreateAsync(space);
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

        public async Task<IBusinessResult> Update(Room space)
        {
            try
            {

                int result = await _unitOfWork.RoomRepository.UpdateAsync(space);
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
