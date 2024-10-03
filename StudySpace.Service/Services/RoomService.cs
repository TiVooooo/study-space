
using StudySpace.Data.Helper;

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
        Task<IBusinessResult> Update(int roomId, CreateRoomRequestModel room);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(CreateRoomRequestModel room);
        Task<IBusinessResult> SearchRooms(int pageNumber, int pageSize, string space, string location, string room, int person);
        Task<IBusinessResult> GetRoomWithCondition(string condition);
        Task<IBusinessResult> UnactiveRoom(int roomId);
        Task<IBusinessResult> GetAllBookedRoomInUser(int userId);
        Task<IBusinessResult> GetAllBookedRoomInSup(int supID);

        Task<IBusinessResult> FilterRoom(int pageNumber, int pageSize, string price, Double[]? priceRange, string[]? utilities, string space, string location, string room, int person);


    }

    public class RoomService : IRoomService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IFeedbackService _feedbackService;
        private readonly IMapper _mapper;
        private readonly IFirebaseService _firebaseService;

        public RoomService(IFeedbackService feedbackService, IFirebaseService firebaseService, IMapper mapper)
        {
            _unitOfWork ??= new UnitOfWork();
            _feedbackService = feedbackService;
            _firebaseService = firebaseService;
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

                if (!pagedRooms.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                var roomIds = pagedRooms.Select(r => r.Id).ToList();
                var bookings = _unitOfWork.BookingRepository
                    .FindByCondition(b => roomIds.Contains(b.RoomId.Value))
                    .ToList();

                var stores = _unitOfWork.StoreRepository
                    .FindByCondition(s => roomIds.Contains(s.Id))
                    .ToList();

                var imageEntities = _unitOfWork.ImageRoomRepository
                    .FindByCondition(ie => roomIds.Contains(ie.RoomId.Value))
                    .ToList();

                var list = pagedRooms.Select(room =>
                {
                    var booking = bookings.FirstOrDefault(b => b.RoomId == room.Id);
                    var store = stores.FirstOrDefault(s => s.Id == room.StoreId);
                    var imageEntity = imageEntities.FirstOrDefault(ie => ie.RoomId == room.Id);

                    return new RoomModel
                    {
                        RoomId = room.Id,
                        RoomName = room.RoomName,
                        StoreName = store?.Name ?? "N/A",
                        Capacity = room.Capacity ?? 0,
                        PricePerHour = room.PricePerHour ?? 0,
                        Description = room.Description,
                        Status = booking?.Status ?? false,
                        Area = room.Area ?? 0,
                        Type = room.Type,
                        Address = store?.Address ?? "N/A",
                        Image = imageEntity?.ImageUrl ?? "N/A"
                    };
                }).ToList();

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
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId ?? 0);
                    var roomModel = new RoomModel
                    {
                        RoomName = r.RoomName,
                        StoreName = store.Name,
                        Capacity = r.Capacity ?? 0,
                        PricePerHour = r.PricePerHour ?? 0,
                        Description = r.Description,
                        Status = r.Status ?? false,
                        Area = r.Area ?? 0,
                        Type = r.Type
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
                var allRooms = await _unitOfWork.RoomRepository.GetAllRoomsAsync();
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
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId ?? 0);
                    var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == r.Id).FirstOrDefault();

                    var roomModel = new RoomModel
                    {
                        RoomId = r.Id,
                        RoomName = r.RoomName,
                        StoreName = store.Name,
                        Capacity = r.Capacity ?? 0,
                        PricePerHour = r.PricePerHour ?? 0,
                        Description = r.Description,
                        Status = r.Status ?? false,
                        Area = r.Area ?? 0,
                        Type = r.Type,
                        Address = store.Address,
                        Image = imageEntity.ImageUrl
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

        public async Task<IBusinessResult> FilterRoom(int pageNumber, int pageSize, string price, Double[]? priceRange, string[]? utilities, string space, string location, string room, int person)
        {
            try
            {
                var pagedRooms = await _unitOfWork.RoomRepository.GetAllRoomsAsync();


                if (!string.IsNullOrWhiteSpace(space) || !string.IsNullOrWhiteSpace(location) || !string.IsNullOrWhiteSpace(room) || person == 0)
                {
                    pagedRooms = pagedRooms.Where(r =>
                        (space.Equals("All") || (r.Space != null && r.Space.SpaceName.Contains(space, StringComparison.OrdinalIgnoreCase))) &&
                        (location.Equals("All") || (r.Store != null && r.Store.Address.Contains(location, StringComparison.OrdinalIgnoreCase))) &&
                        (r.Capacity >= person)
                    ).ToList();
                }

                var totalCount = pagedRooms.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var rooms = pagedRooms.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                if (priceRange[0] != 0 && priceRange[1] != 0)
                {
                    rooms = rooms.Where(r => r.PricePerHour >= priceRange[0] && r.PricePerHour <= priceRange[1]).ToList();

                }


                if (price.Equals("highest"))
                {
                    rooms = rooms.OrderByDescending(x => x.PricePerHour).ToList();
                }
                else if (price.Equals("lowest"))
                {
                    rooms = rooms.OrderBy(x => x.PricePerHour).ToList();
                }
                else if (utilities != null && utilities.Any() && !utilities.Contains("All"))
                {
                    var selectedAmitiesIds = _unitOfWork.AmityRepository
                .FindByCondition(x => utilities.Contains(x.Name))
                .Select(x => x.Id)
                .ToList();
                    var listR = new List<Room>();

                    // First, get all rooms that have any of the selected amenities
                    var roomsWithSelectedAmenities = _unitOfWork.RoomAminitiesRepository
                        .FindByCondition(x => selectedAmitiesIds.Contains(x.AmitiesId))
                        .Select(x => x.RoomId)
                        .Distinct()
                        .ToList();

                    // Now, filter rooms to only include those that have all the selected amenities
                    foreach (var roomId in roomsWithSelectedAmenities)
                    {
                        var amenitiesInRoom = _unitOfWork.RoomAminitiesRepository
                            .FindByCondition(x => x.RoomId == roomId)
                            .Select(x => x.AmitiesId)
                            .ToList();

                        // Check if all selected amenities are present in this room
                        if (selectedAmitiesIds.All(id => amenitiesInRoom.Contains(id)))
                        {
                            var roomNe = _unitOfWork.RoomRepository.GetById(roomId);
                            listR.Add(roomNe);
                        }
                    }

                    rooms = rooms.Where(r => listR.Any(lr => lr.Id == r.Id)).ToList();

                }


                var list = new List<RoomModel>();
                foreach (var r in rooms)
                {
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId ?? 0);
                    var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == r.Id).FirstOrDefault();

                    var roomModel = new RoomModel
                    {
                        RoomId = r.Id,
                        RoomName = r.RoomName,
                        StoreName = store.Name,
                        Capacity = r.Capacity ?? 0,
                        PricePerHour = r.PricePerHour ?? 0,
                        Description = r.Description,
                        Status = r.Status ?? false,
                        Area = r.Area ?? 0,
                        Type = r.Type,
                        Address = store.Address,
                        Image = imageEntity.ImageUrl
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


                var listImageOfRoom = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == id).Select(i => i.ImageUrl).ToList();

                var listAminityRoom = _unitOfWork.RoomAminitiesRepository.FindByCondition(a => a.RoomId == id).Select(a => a.AmitiesId).ToList();

                var listAminity = new List<string>();

                foreach (var amity in listAminityRoom)
                {
                    var name = _unitOfWork.AmityRepository.GetById(amity);
                    listAminity.Add(name.Name);
                }

                var spaceName = _unitOfWork.SpaceRepository.GetById(room.SpaceId ?? 0);


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
                    Capacity = room.Capacity ?? 0,
                    PricePerHour = room.PricePerHour ?? 0,
                    Area = room.Area ?? 0,
                    HouseRule = room.HouseRule,
                    TypeOfRoom = room.Type,
                    ListImages = listImageOfRoom,
                    Description = room.Description,
                    Address = store.Address,
                    Longtitude = store.Longitude,
                    Latitude = store.Latitude,
                    Aminities = listAminity,
                    RelatedRoom = relatedRoomsResult.Data as List<RoomModel>,
                    Status = room.Status ?? false
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> Save(CreateRoomRequestModel room)
        {
            try
            {
                var storeExisted = _unitOfWork.StoreRepository.FindByCondition(c => c.Id == room.StoreId).FirstOrDefault();
                var spaceExisted = _unitOfWork.SpaceRepository.FindByCondition(c => c.Id == room.SpaceId).FirstOrDefault();

                if (storeExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Unknown Store.");
                }

                if (spaceExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Unknown Space.");
                }

                var newRoom = new Room
                {
                    SpaceId = room.SpaceId,
                    RoomName = room.RoomName,
                    StoreId = room.StoreId,
                    Capacity = room.Capacity,
                    PricePerHour = room.PricePerHour,
                    Type = room.Type,
                    Description = room.Description,
                    Status = true,
                    Area = room.Area,
                    HouseRule = room.HouseRule
                };

                _unitOfWork.RoomRepository.PrepareCreate(newRoom);
                int result = await _unitOfWork.RoomRepository.SaveAsync();
                foreach (var item in room.Amities)
                {

                    var amity = _unitOfWork.AmityRepository.GetById(item.AmityId);
                    if (amity.Quantity >= item.Quantity)
                    {
                        amity.Quantity -= item.Quantity;
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_CREATE, "There is not enough Amity in stock!");
                    }

                    _unitOfWork.AmityRepository.PrepareUpdate(amity);
                    var amityRoom = new RoomAmity
                    {
                        RoomId = newRoom.Id,
                        AmitiesId = item.AmityId,
                        Quantity = item.Quantity
                    };
                    _unitOfWork.RoomAminitiesRepository.PrepareCreate(amityRoom);
                    await _unitOfWork.RoomAminitiesRepository.SaveAsync();


                }

                await _unitOfWork.AmityRepository.SaveAsync();

                var imageUrls = room.ImageRoom;
                if (imageUrls != null)
                {
                    foreach (var image in imageUrls)
                    {
                        var newRoomImage = new ImageRoom
                        {
                            Room = newRoom
                        };
                        var imagePath = FirebasePathName.RATING + Guid.NewGuid().ToString();
                        var imageUploadResult = await _firebaseService.UploadImageToFirebaseAsync(image, imagePath);
                        newRoomImage.ImageUrl = imageUploadResult;
                        _unitOfWork.ImageRoomRepository.PrepareCreate(newRoomImage);
                    }

                }
                await _unitOfWork.ImageRoomRepository.SaveAsync();


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

        public async Task<IBusinessResult> Update(int roomId, CreateRoomRequestModel room)
        {
            try
            {
                var storeExisted = _unitOfWork.StoreRepository.FindByCondition(c => c.Id == room.StoreId).FirstOrDefault();
                var spaceExisted = _unitOfWork.SpaceRepository.FindByCondition(c => c.Id == room.SpaceId).FirstOrDefault();

                if (storeExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Unknown Store.");
                }

                if (spaceExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Unknown Space.");
                }

                var updatedRoom = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
                if (updatedRoom == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Room not found.");
                }

                updatedRoom.SpaceId = room.SpaceId;
                updatedRoom.RoomName = room.RoomName;
                updatedRoom.StoreId = room.StoreId;
                updatedRoom.Capacity = room.Capacity;
                updatedRoom.PricePerHour = room.PricePerHour;
                updatedRoom.Description = room.Description;
                updatedRoom.Status = true;
                updatedRoom.Area = room.Area;
                updatedRoom.HouseRule = room.HouseRule;

                _unitOfWork.RoomRepository.PrepareUpdate(updatedRoom);

                /*
                                foreach (var item in room.Amities)
                                {

                                    var amity = _unitOfWork.AmityRepository.GetById(item.AmityId);
                                    if (amity.Quantity >= item.Quantity)
                                    {
                                        amity.Quantity -= item.Quantity;
                                    }
                                    else
                                    {
                                        return new BusinessResult(Const.FAIL_CREATE, "There is not enough Amity in stock!");
                                    }

                                    _unitOfWork.AmityRepository.PrepareUpdate(amity);
                                    var amityRoom = new RoomAmity
                                    {
                                        RoomId = newRoom.Id,
                                        AmitiesId = item.AmityId,
                                        Quantity = item.Quantity
                                    };
                                    _unitOfWork.RoomAminitiesRepository.PrepareCreate(amityRoom);
                                    await _unitOfWork.RoomAminitiesRepository.SaveAsync();


                                }*/

                await _unitOfWork.AmityRepository.SaveAsync();


                var imageUrls = room.ImageRoom;
                if (imageUrls != null)
                {
                    var existingImages = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == roomId).ToList();
                    foreach (var imageUrl in existingImages)
                    {
                        _unitOfWork.ImageRoomRepository.PrepareRemove(imageUrl);
                    }

                    foreach (var image in imageUrls)
                    {
                        var newRoomImage = new ImageRoom
                        {
                            Room = updatedRoom
                        };
                        var imagePath = FirebasePathName.RATING + Guid.NewGuid().ToString();
                        var imageUploadResult = await _firebaseService.UploadImageToFirebaseAsync(image, imagePath);
                        newRoomImage.ImageUrl = imageUploadResult;
                        _unitOfWork.ImageRoomRepository.PrepareCreate(newRoomImage);
                    }

                }
                int result = await _unitOfWork.RoomRepository.SaveAsync();
                await _unitOfWork.ImageRoomRepository.SaveAsync();


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
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> UnactiveRoom(int roomId)
        {
            try
            {
                var roomUnactive = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);

                if (roomUnactive == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                roomUnactive.Status = !roomUnactive.Status;

                int result = await _unitOfWork.RoomRepository.UpdateAsync(roomUnactive);

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

                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId ?? 0);
                    var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == r.Id).FirstOrDefault();

                    var roomModel = new RoomModel
                    {
                        RoomId = r.Id,
                        RoomName = r.RoomName,
                        StoreName = store.Name,
                        Capacity = r.Capacity ?? 0,
                        PricePerHour = r.PricePerHour ?? 0,
                        Description = r.Description,
                        Status = booking.Status ?? false,
                        Area = r.Area ?? 0,
                        Type = r.Type,
                        Address = store.Address,
                        Image = imageEntity.ImageUrl
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
                var rooms = await _unitOfWork.RoomRepository.GetSupRoomAsync();

                var filteredRooms = rooms
                    .Where(r => r.StoreId == supID && r.Bookings.Any())
                    .ToList();

                var result = new List<RoomSupModel>();

                foreach (var room in filteredRooms)
                {
                    var booking = room.Bookings.FirstOrDefault();
                    var store = room.Store;
                    var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == room.Id).FirstOrDefault();

                    var amitiesInRoom = new List<AmitiesInRoom>();

                    foreach (var roomAmity in room.RoomAmities)
                    {
                        if (roomAmity.Amities != null)
                        {
                            var amityInRoom = new AmitiesInRoom
                            {
                                Id = roomAmity.Amities.Id,
                                Name = roomAmity.Amities.Name,
                                Type = roomAmity.Amities.Type,
                                Status = roomAmity.Amities.Status ?? false,
                                Quantity = roomAmity.Quantity ?? 0,
                                Description = roomAmity.Amities.Description
                            };
                            amitiesInRoom.Add(amityInRoom);
                        }
                    }

                    var roomModel = new RoomSupModel
                    {
                        RoomId = room.Id,
                        RoomName = room.RoomName,
                        StoreName = store.Name,
                        Capacity = room.Capacity ?? 0,
                        PricePerHour = room.PricePerHour ?? 0,
                        Description = room.Description,
                        Status = booking?.Status ?? false,
                        Area = room.Area ?? 0,
                        Type = room.Type,
                        Address = store.Address,
                        Image = imageEntity?.ImageUrl,
                        AmitiesInRoom = amitiesInRoom
                    };
                    result.Add(roomModel);
                }

                if (!result.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "No booked rooms found for this supplier.");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }


    }
}

