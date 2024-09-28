using AutoMapper;
using Firebase.Auth;
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

        Task<IBusinessResult> GetAllBookedRoomInUser(int userId);

        Task<IBusinessResult> GetAllBookedRoomInSup(int supID);



    }

    public class RoomService : IRoomService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IFeedbackService _feedbackService;
        private readonly IMapper _mapper;


        public RoomService(IFeedbackService feedbackService, IMapper mapper)
        {
            _unitOfWork ??= new UnitOfWork();
            _feedbackService = feedbackService;
            _mapper = mapper;
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
                var rooms = _unitOfWork.RoomRepository.FindByCondition(r => r.Space.SpaceName.Equals(condition)).Take(3).ToList();


                var list = new List<RoomModel>();

                foreach (var r in rooms)
                {
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId);
                    var amity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == r.Id).Select(a => a.Type).FirstOrDefault();
                    var roomModel = new RoomModel
                    {
                        RoomName = r.RoomName,
                        StoreName = store.Name,
                        Capacity = r.Capacity,
                        PricePerHour = r.PricePerHour,
                        Description = r.Description,
                        Status = r.Status,
                        Area = r.Area,
                        Type = amity
                    };
                    list.Add(roomModel);
                }



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
                var allRooms = await _unitOfWork.RoomRepository.GetAllAsync();
                var rooms = allRooms.Where(x => x.Status == true).ToList();

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
                var list = new List<RoomModel>();
                foreach (var r in pagedRooms)
                {
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId);
                    var amity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == r.Id).Select(a => a.Type).FirstOrDefault();

                    var roomModel = new RoomModel
                    {
                        RoomName = r.RoomName,
                        StoreName = store.Name,
                        Capacity = r.Capacity,
                        PricePerHour = r.PricePerHour,
                        Description = r.Description,
                        Status = r.Status,
                        Area = r.Area,
                        Type = amity
                    };
                    list.Add(roomModel);
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
                else if (!room.Status)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                var listImageOfRoom = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == id).Select(i => i.ImageUrl).ToList();
                var listAminity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == id).Select(a => a.Name).ToList();
                var type = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == id).Select(a => a.Type).FirstOrDefault();


                var spaceName = _unitOfWork.SpaceRepository.GetById(room.SpaceId);


                var relatedRoomsResult = await GetRoomWithCondition(spaceName.SpaceName);
                if (relatedRoomsResult == null || relatedRoomsResult.Data == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Related room information is not available.");
                }

                var store = _unitOfWork.StoreRepository.FindByCondition(s => s.Id == room.StoreId).FirstOrDefault();


                var result = new RoomDetailModel
                {
                    RoomName = room.RoomName,
                    StoreName = store.Name,
                    Capacity = room.Capacity,
                    PricePerHour = room.PricePerHour,
                    Area = room.Area,
                    HouseRule = room.HouseRule,
                    TypeOfRoom = type,
                    ListImages = listImageOfRoom,
                    Description = room.Description,
                    Address = store.Address,
                    Longtitude = store.Longitude,
                    Latitude = store.Latitude,
                    Aminities = listAminity,
                    RelatedRoom = relatedRoomsResult.Data as List<RoomModel>,
                    Status = room.Status
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

        public async Task<IBusinessResult> GetAllBookedRoomInUser(int userId)
        {
            try
            {
                var user = _unitOfWork.AccountRepository.GetById(userId);
                var allRooms = await _unitOfWork.RoomRepository.GetAllAsync();

                var listRoomID = _unitOfWork.BookingRepository.FindByCondition(b => b.UserId == userId).Select(b => b.RoomId).ToList();

                var listRoom = allRooms.Where(r => listRoomID.Contains(r.Id)).ToList();

                var result = new List<RoomModel>();


                foreach (var room in listRoom)
                {
                    var r = _unitOfWork.RoomRepository.GetById(room.Id);
                    var booking = _unitOfWork.BookingRepository.FindByCondition(b => b.RoomId == room.Id && b.UserId == userId).FirstOrDefault();

                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId);
                    var amity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == r.Id).Select(a => a.Type).FirstOrDefault();

                    var roomModel = new RoomModel
                    {
                        RoomName = r.RoomName,
                        StoreName = store.Name,
                        Capacity = r.Capacity,
                        PricePerHour = r.PricePerHour,
                        Description = r.Description,
                        Status = booking.Status ?? false,
                        Area = r.Area,
                        Type = amity
                    };
                    result.Add(roomModel);
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllBookedRoomInSup(int supID)
        {
            try
            {
                var rooms = _unitOfWork.RoomRepository.FindByCondition(r => r.StoreId == supID && r.Bookings.Any()).ToList();
                var result = new List<RoomModel>();

                foreach (var room in rooms)
                {
                    var booking = _unitOfWork.BookingRepository.FindByCondition(b => b.RoomId == room.Id).FirstOrDefault();
                    var store = _unitOfWork.StoreRepository.GetById(room.StoreId);
                    var amity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == room.Id).Select(a => a.Type).FirstOrDefault();

                    var roomModel = new RoomModel
                    {
                        RoomName = room.RoomName,
                        StoreName = store.Name,
                        Capacity = room.Capacity,
                        PricePerHour = room.PricePerHour,
                        Description = room.Description,
                        Status = booking.Status ?? false,
                        Area = room.Area,
                        Type = amity
                    };
                    result.Add(roomModel);
                }
                return new BusinessResult(Const.FAIL_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);

            }



        }



    }
}

