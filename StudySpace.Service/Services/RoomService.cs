
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
using Newtonsoft.Json;
using MimeKit;

namespace StudySpace.Service.Services
{

    public interface IRoomService
    {
        Task<IBusinessResult> GetAll(int pageNumber, int pageSize);
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(int roomId, UpdateRoomModel room);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetDetailBookedRoomInUser(int bookingId);
        Task<IBusinessResult> Save(CreateRoomRequestModel room);
        Task<IBusinessResult> SearchRooms(int pageNumber, int pageSize, string space, string location, string room, int person);
        Task<IBusinessResult> GetRoomWithCondition(string condition);
        Task<IBusinessResult> UnactiveRoom(int roomId);
        Task<IBusinessResult> GetAllBookedRoomInUser(int userId);
        Task<IBusinessResult> GetAllBookedRoomInSup(int supID);
        Task<IBusinessResult> FilterRoom(int pageNumber, int pageSize, string price, Double[]? priceRange, string[]? utilities, string space, string location, string room, int person);
        Task<IBusinessResult> GetAllRoomInSup(int supID);
        Task<IBusinessResult> Xoa(string url);
        Task<IBusinessResult> GetRoomDetailInSup(int supId, int roomId);
        Task<IBusinessResult> GetPremiumRoomStore();
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
                    var imageEntity = imageEntities.FirstOrDefault(ie => ie.RoomId == room.Id && !ie.ImageUrl.Contains("MENU"));

                    if (booking.Status != null && booking.Status == "")
                    {
                        return new RoomModel
                        {
                            RoomId = room.Id,
                            RoomName = room.RoomName,
                            StoreName = store?.Name ?? "N/A",
                            Capacity = room.Capacity ?? 0,
                            PricePerHour = room.PricePerHour ?? 0,
                            Description = room.Description,
                            Status = room.Status ?? false,

                            Area = room.Area ?? 0,
                            Type = room.Type,
                            Address = store?.Address ?? "N/A",
                            Image = imageEntity?.ImageUrl ?? "N/A"
                        };
                    }
                    return null;

                }).Where(roomModel => roomModel != null).ToList();

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
                var rooms = _unitOfWork.RoomRepository
                        .FindByCondition(r => r.Space.SpaceName.Equals(condition))
                        .ToList();

                var random = new Random();
                rooms = rooms.OrderBy(r => random.Next()).Take(3).ToList();

                var list = new List<RoomModel>();

                foreach (var r in rooms)
                {
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId ?? 0);
                    var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == r.Id && !ie.ImageUrl.Contains("MENU")).FirstOrDefault();

                    var roomModel = new RoomModel
                    {
                        RoomId = r.Id,
                        RoomName = r.RoomName,
                        StoreName = store?.Name ?? "Unknown", 
                        Capacity = r.Capacity ?? 0,
                        PricePerHour = r.PricePerHour ?? 0,
                        Description = r.Description,
                        Status = r.Status ?? false,
                        Area = r.Area ?? 0,
                        Type = r.Type,
                        Address = store?.Address ?? "Unknown", 
                        Image = imageEntity?.ImageUrl, 
                        isOvernight = store?.IsOverNight
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

                rooms = rooms.OrderByDescending(r => r.Id).ToList();

                var totalCount = rooms.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var pagedRooms = rooms.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var list = new List<RoomModel>();
                foreach (var r in pagedRooms)
                {
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId ?? 0);
                    var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == r.Id && !ie.ImageUrl.Contains("MENU")).FirstOrDefault();

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
                        Image = imageEntity.ImageUrl,
                        isOvernight = store.IsOverNight
                    };
                    list.Add(roomModel);
                }

                var result = new GetAllRoomModel
                {
                    TotalAvailable = list.Count,
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
                var rooms = await _unitOfWork.RoomRepository.GetAllRoomsAsync();

                if (!string.IsNullOrWhiteSpace(space) || !string.IsNullOrWhiteSpace(location) || !string.IsNullOrWhiteSpace(room) || person == 0)
                {
                    rooms = rooms.Where(r =>
                        (space.Equals("All") || (r.Space != null && r.Space.SpaceName.Contains(space, StringComparison.OrdinalIgnoreCase))) &&
                        (location.Equals("All") || (r.Store != null && r.Store.Address.Contains(location, StringComparison.OrdinalIgnoreCase))) &&
                        (r.Capacity >= person)
                    ).ToList();
                }

                if (priceRange[1] != 0)
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

                if (utilities != null && utilities.Any() && !utilities.Contains("All"))
                {
                    var selectedAmitiesIds = _unitOfWork.AmityRepository
                        .FindByCondition(x => utilities.Contains(x.Name))
                        .Select(x => x.Id)
                        .ToList();

                    var listR = new List<Room>();
                    var roomsWithSelectedAmenities = _unitOfWork.RoomAminitiesRepository
                        .FindByCondition(x => selectedAmitiesIds.Contains(x.AmitiesId))
                        .Select(x => x.RoomId)
                        .Distinct()
                        .ToList();

                    foreach (var roomId in roomsWithSelectedAmenities)
                    {
                        var amenitiesInRoom = _unitOfWork.RoomAminitiesRepository
                            .FindByCondition(x => x.RoomId == roomId)
                            .Select(x => x.AmitiesId)
                            .ToList();

                        bool hasAllAmenities = selectedAmitiesIds.All(id => amenitiesInRoom.Contains(id));

                        if (hasAllAmenities)
                        {
                            var roomNe = _unitOfWork.RoomRepository.GetById(roomId);
                            listR.Add(roomNe);
                        }
                    }

                    rooms = rooms.Where(r => listR.Any(lr => lr.Id == r.Id)).ToList();
                }

                // Tính tổng số lượng phòng sau khi lọc
                var totalRooms = rooms.Count;

                // Sử dụng Skip và Take để phân trang
                rooms = rooms.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

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
                        Image = imageEntity.ImageUrl ?? "default_image_url_here",
                        isOvernight = store.IsOverNight
                    };
                    list.Add(roomModel);
                }

                // Tính tổng số trang
                var totalPages = (int)Math.Ceiling((double)totalRooms / pageSize);

                var result = new GetAllRoomModel
                {
                    TotalAvailable = totalRooms,
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

                var bookedSlots = _unitOfWork.BookingRepository.FindByCondition(b => b.RoomId == id && b.Status == "Payed")
                                                               .GroupBy(b => b.StartTime.Value.Date)
                                                               .Select(date => new BookedSlots
                                                               {
                                                                   Date = date.Key.ToString("yyyy-MM-dd"),
                                                                   Slots = date.Select(b => new Slots
                                                                   {
                                                                       Start = b.StartTime.HasValue ? b.StartTime.Value.ToString("HH:mm") : null,
                                                                       End = b.EndTime.HasValue ? b.EndTime.Value.ToString("HH:mm") : null
                                                                   }).ToList()
                                                               }).ToList();

                var listImageOfRoom = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == id).Select(i => i.ImageUrl).ToList();

                var imageMenu = listImageOfRoom.FirstOrDefault(i => i.Contains("MENU"));
                var listOtherImage = listImageOfRoom.Where(i => !i.Contains("MENU")).ToList();

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

                var imageBuBuBu = new ListImages
                {
                    ImageMenu = imageMenu,
                    ImageList = listOtherImage
                };

                var result = new RoomDetailModel
                {
                    RoomName = room.RoomName,
                    StoreName = store.Name,
                    Capacity = room.Capacity ?? 0,
                    PricePerHour = room.PricePerHour ?? 0,
                    Area = room.Area ?? 0,
                    ListImages = imageBuBuBu,
                    HouseRule = room.HouseRule?.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(part => part.Trim())
                                                .ToArray(),
                    TypeOfRoom = room.Type,
                    Description = room.Description,
                    Address = store.Address,
                    Longtitude = store.Longitude,
                    Latitude = store.Latitude,
                    Aminities = listAminity,
                    RelatedRoom = relatedRoomsResult.Data as List<RoomModel>,
                    Status = room.Status ?? false,
                    BookedSlots = bookedSlots,
                    isOvernight = store.IsOverNight ?? false,
                    StartTime = store.OpenTime?.TimeOfDay,
                    EndTime = store.CloseTime?.TimeOfDay,
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
                // Step 1: Validate Store Existence
                var storeExisted = _unitOfWork.StoreRepository.FindByCondition(c => c.Id == room.StoreId).FirstOrDefault();
                if (storeExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Unknown Store.");
                }

                // Step 2: Validate Space Existence
                var spaceExisted = _unitOfWork.SpaceRepository.FindByCondition(c => c.Id == room.SpaceId).FirstOrDefault();
                if (spaceExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Unknown Space.");
                }

                
                if (room.Amities.Count != 0)
                {
                    foreach (var item in room.Amities)
                    {
                        var amity = _unitOfWork.AmityRepository.GetById(item.AmityId);
                        if (amity == null || amity.Quantity < item.Quantity)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, "There is not enough Amity in stock!");
                        }
                    }


                }
                
                // Step 3: Validate Amity Availability
                

                // Step 4: Format House Rules
                var hr = string.Join(".", room.HouseRule.Select(hr => hr.TrimEnd('.')));
                if (room.HouseRule.Any() && !room.HouseRule.Last().EndsWith("."))
                {
                    hr += ".";
                }

                // Step 5: Prepare new Room entity
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
                    HouseRule = hr
                };

                // Step 6: Save the Room
                _unitOfWork.RoomRepository.PrepareCreate(newRoom);
                int roomResult = await _unitOfWork.RoomRepository.SaveAsync();

                // Check if the room was saved successfully
                if (roomResult <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Failed to create room.");
                }

                // Step 7: Create RoomAmity entries
                if (room.Amities.Count != 0)
                {
                    foreach (var item in room.Amities)
                    {
                        var amity = _unitOfWork.AmityRepository.GetById(item.AmityId);
                        amity.Quantity -= item.Quantity; // Update the quantity
                        _unitOfWork.AmityRepository.PrepareUpdate(amity);

                        var amityRoom = new RoomAmity
                        {
                            RoomId = newRoom.Id, // Use the saved room's ID
                            AmitiesId = item.AmityId,
                            Quantity = item.Quantity
                        };
                        _unitOfWork.RoomAminitiesRepository.PrepareCreate(amityRoom);
                    }
                }
                

                // Handle menu image
                var newMenuImage = new ImageRoom();

                // Handle Room Images
                if (room.ImageRoom != null)
                {
                    foreach (var image in room.ImageRoom)
                    {
                        var newRoomImage = new ImageRoom
                        {
                            Room = newRoom, // Link to the saved room
                            Status = true,
                        };
                        var imagePath = FirebasePathName.RATING + Guid.NewGuid().ToString();
                        var imageUploadResult = await _firebaseService.UploadImageToFirebaseAsync(image, imagePath);
                        newRoomImage.ImageUrl = imageUploadResult;
                        _unitOfWork.ImageRoomRepository.PrepareCreate(newRoomImage);
                    }
                }

                if (room.ImageMenu != null)
                {
                    newMenuImage = new ImageRoom
                    {
                        Room = newRoom,
                        Status = true
                    };
                    var imgPath = FirebasePathName.MENU + Guid.NewGuid().ToString();
                    var imgUploadRes = await _firebaseService.UploadImageToFirebaseAsync(room.ImageMenu, imgPath);
                    newMenuImage.ImageUrl = imgUploadRes;
                    _unitOfWork.ImageRoomRepository.PrepareCreate(newMenuImage);
                }

                // Save all changes
                await _unitOfWork.RoomAminitiesRepository.SaveAsync();
                await _unitOfWork.ImageRoomRepository.SaveAsync();

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, room);
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetDetailBookedRoomInUser(int bookingId)
        {
            try
            {
                var booking = _unitOfWork.BookingRepository.GetById(bookingId);
                var room = _unitOfWork.RoomRepository.GetById(booking.RoomId ?? 0);
                var images = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == room.Id).Select(i => i.ImageUrl).ToList();
                var result = new BookedRoomDetailUserModel
                {
                    RoomId = room.Id,
                    RoomName = room.RoomName,
                    BookingDate = booking.BookingDate,
                    CheckIn = booking.Checkin,
                    Note = booking.Note,
                    Fee = booking.Fee ?? 0,
                    PaymentMethod = booking.PaymentMethod,
                    End = booking.EndTime?.TimeOfDay,
                    Start = booking.StartTime?.TimeOfDay,
                    ImageUrl = images
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> Xoa(string url)
        {
        //    var image = _unitOfWork.ImageRoomRepository.FindByCondition(i=>i.ImageUrl == url);

            var re = _firebaseService.DeleteFileFromFirebase(url);
            return new BusinessResult(Const.SUCCESS_DELETE,Const.SUCCESS_DELETE_MSG, re);
        }

        public async Task<IBusinessResult> Update(int roomId, UpdateRoomModel room)
        {
            try
            {


                var updatedRoom = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
                if (updatedRoom == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Room not found.");
                }

        
              
                if (room.PricePerHour != null)
                {
                    updatedRoom.PricePerHour = room.PricePerHour;

                }
                if (room.Description != null)
                {
                    updatedRoom.Description = room.Description;

                }
            
               

                if (room.HouseRule != null)
                {
                    var hr = string.Empty;

                    foreach (var hrs in room.HouseRule)
                    {
                        hr += hrs.ToString() + ".";
                    }

                    updatedRoom.HouseRule = hr;
                }

                _unitOfWork.RoomRepository.PrepareUpdate(updatedRoom);



                int result = await _unitOfWork.RoomRepository.SaveAsync();
                await _unitOfWork.ImageRoomRepository.SaveAsync();


                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UDATE, Const.SUCCESS_UDATE_MSG, room);
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

                if(user == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "User is not existed !");
                }

                var result = _unitOfWork.BookingRepository.GetBookingDetails()
                                                          .Where(b => b.UserId == userId)
                                                          .OrderByDescending(b => b.BookingDate)
                                                          .Select(booking => new GetBookedRoomInUserModel
                                                                    {
                                                                        BookingId = booking.Id,
                                                                        RoomId = booking.Room.Id,
                                                                        RoomName = booking.Room.RoomName,
                                                                        StoreName = booking.Room.Store.Name,
                                                                        Capacity = booking.Room.Capacity ?? 0,
                                                                        PricePerHour = booking.Room.PricePerHour ?? 0,
                                                                        Description = booking.Room.Description,
                                                                        Status = booking.Room.Status ?? false,
                                                                        Area = booking.Room.Area ?? 0,
                                                                        Type = booking.Room.Type,
                                                                        Address = booking.Room.Store.Address,
                                                                        Image = booking.Room.ImageRooms.FirstOrDefault().ImageUrl,
                                                                        BookedDate = booking.BookingDate.HasValue ? booking.BookingDate.Value.ToString("yyyy-MM-dd") : null,
                                                                        BookedTime = booking.BookingDate.HasValue ? booking.BookingDate.Value.ToString("HH:mm:ss") : null,
                                                                        BookingStatus = booking.Status,
                                                                        CheckIn = booking.Checkin ?? false,
                                                                        isOvernight = booking.Room.Store.IsOverNight,
                                                                        TypeSpace = booking.Room.Space.SpaceName,
                                                                        PaymentMethod = booking.PaymentMethod,
                                                                        Start = booking.StartTime.HasValue ? booking.StartTime.Value.ToString("HH:mm:ss") : null,
                                                                        End = booking.EndTime.HasValue ? booking.EndTime.Value.ToString("HH:mm:ss") : null,
                                                                        IsFeedback = booking.Feedbacks.Any(f => f.UserId == userId)
                                                                    })
                                                                    .ToList();

                
               
                if(result.Count > 0)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                } 
                else
                {
                    return new BusinessResult(Const.FAIL_READ, "No booked room",new List<GetBookedRoomInUserModel>());
                }
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
                    .Where(r => r.StoreId == supID)
                    .ToList();

                var result = new List<RoomSupModel>();

                foreach (var room in filteredRooms)
                {
                    var booking = _unitOfWork.BookingRepository.FindByCondition(b=>b.RoomId == room.Id).ToList();

                    foreach (var book in booking)
                    {
                        var store = room.Store;
                        var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == room.Id).FirstOrDefault();


                        var userName = _unitOfWork.AccountRepository.GetById(book.UserId ?? 0);


                      
                        var roomModel = new RoomSupModel
                        {
                            BookingId = book.Id,
                            UserName = userName.Name,
                            Avatar = userName.AvatarUrl,
                            Email = userName.Email,
                            Gender = userName.Gender,
                            UserAddress = userName.Address,
                            BookedDate = book.BookingDate,
                            Checkin = book.Checkin ?? false,
                            BookedTime = book.StartTime?.TimeOfDay,
                            RoomId = room.Id,
                            RoomName = room.RoomName,
                            StoreName = store.Name,
                            Capacity = room.Capacity ?? 0,
                            PricePerHour = room.PricePerHour ?? 0,
                            Description = room.Description,
                            Status = book?.Status ?? null,
                            Area = room.Area ?? 0,
                            SpaceType = room.Space.SpaceName,
                            RoomType = room.Type,
                            Address = store.Address,
                            Image = imageEntity?.ImageUrl,
                        };
                        result.Add(roomModel);
                    }
                    
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

        public async Task<IBusinessResult> GetAllRoomInSup(int supID)
        {
            try
            {
                var rooms = await _unitOfWork.RoomRepository.GetSupRoomAsync();
                var filteredRooms = rooms
                    .Where(r => r.StoreId == supID)
                    .ToList();

                var result = new List<RoomSupModelVer1>();

                foreach (var room in filteredRooms)
                {
                    var booking = room.Bookings.FirstOrDefault();
                    var store = room.Store;
                    var imageEntity = _unitOfWork.ImageRoomRepository.FindByCondition(ie => ie.RoomId == room.Id).FirstOrDefault();

                    var amitiesInRoom = new List<AmitiesInRoomVer1>();

                    foreach (var roomAmity in room.RoomAmities)
                    {
                        if (roomAmity.Amities != null)
                        {
                            var amityInRoom = new AmitiesInRoomVer1
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


                    var roomModel = new RoomSupModelVer1
                    {
                        RoomId = room.Id,
                        RoomName = room.RoomName,
                        StoreName = store.Name,
                        Capacity = room.Capacity ?? 0,
                        PricePerHour = room.PricePerHour ?? 0,
                        Description = room.Description,
                        Status = room.Status ?? false,
                        Area = room.Area ?? 0,
                        SpaceType = room.Space.SpaceName,
                        RoomType = room.Type,
                        Address = store.Address,
                        Image = imageEntity?.ImageUrl,
                        AmitiesInRoom = amitiesInRoom
                    };
                    result.Add(roomModel);
                }

                if (!result.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "No rooms found for this supplier.");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetRoomDetailInSup(int supId, int roomId)
        {
            try
            {
                var existedRoom = await _unitOfWork.RoomRepository.FindByConditionAsync(r => r.Id == roomId && r.StoreId == supId);
                if(existedRoom.Count() == 0)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);

                var listImageOfRoom = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == roomId).Select(i => i.ImageUrl).ToList();

                var imageMenu = listImageOfRoom.FirstOrDefault(i => i.Contains("MENU"));
                var listOtherImage = listImageOfRoom.Where(i => !i.Contains("MENU")).ToList();

                var listAminityRoom = _unitOfWork.RoomAminitiesRepository.FindByCondition(a => a.RoomId == roomId).Select(a => a.AmitiesId).ToList();

                var spaceName = _unitOfWork.SpaceRepository.GetById(room.SpaceId ?? 0);

                var store = _unitOfWork.StoreRepository.FindByCondition(s => s.Id == room.StoreId).FirstOrDefault();

                var space = _unitOfWork.SpaceRepository.FindByCondition(s => s.Id == room.Space.Id).FirstOrDefault();

                var imageBuBuBu = new ListImages
                {
                    ImageMenu = imageMenu,
                    ImageList = listOtherImage
                };

                var rooms = await _unitOfWork.RoomRepository.GetSupRoomAsync();
                var filteredRooms = rooms
                    .Where(r => r.StoreId == supId)
                    .ToList();

                var roomDo = filteredRooms.FirstOrDefault(c => c.Id == room.Id);

                var amitiesInRoom = new List<AmitiesInRoomVer2>();

                foreach (var roomAmity in roomDo.RoomAmities)
                {
                    if (roomAmity.Amities != null)
                    {
                        var amityInRoom = new AmitiesInRoomVer2
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

                var result = new RoomSupModelVer2
                {
                    RoomName = room.RoomName,
                    StoreName = store.Name,
                    Capacity = room.Capacity ?? 0,
                    PricePerHour = room.PricePerHour ?? 0,
                    Area = room.Area ?? 0,
                    ListImages = imageBuBuBu,
                    HouseRule = room.HouseRule?.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(part => part.Trim())
                                                .ToArray(),
                    TypeRoom = room.Type,
                    TypeSpace = space.SpaceName,
                    Description = room.Description,
                    Address = store.Address,
                    Longtitude = store.Longitude,
                    Latitude = store.Latitude,
                    Status = room.Status ?? false,
                    isOvernight = store.IsOverNight ?? false,
                    StartTime = store.OpenTime?.TimeOfDay,
                    EndTime = store.CloseTime?.TimeOfDay,
                    AmitiesInRoom = amitiesInRoom
                };

                if(result == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                } else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                }


            } catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAll()
        {
            try
            {
                var all = await _unitOfWork.RoomRepository.GetSupRoomAsync();

                var rooms = all.Select(r => new GetAllRoomsModel
                {
                    RoomId = r.Id,
                    RoomName = r.RoomName,
                    StoreName = r.Store.Name,
                    Capacity = r.Capacity,
                    PricePerHour = r.PricePerHour,
                    Description = r.Description,
                    Status = r.Status,
                    Area = r.Area,
                    RoomType = r.Type,
                    SpaceType = r.Space.SpaceName,
                    Image = r.ImageRooms.FirstOrDefault() != null ? r.ImageRooms.FirstOrDefault().ImageUrl : null,
                    Address = r.Store.Address,
                    AmitiesInRoom = r.RoomAmities.Select(ra => new AmityInRoom
                    {
                        Id = ra.Amities.Id,
                        Name = ra.Amities.Name,
                        Type = ra.Amities.Type,
                        Status = ra.Amities.Status,
                        Quantity = ra.Quantity,
                        Description = ra.Amities.Description
                    }).ToList()
                }).OrderByDescending(r => r.RoomId).ToList();

                if (rooms.Count > 0)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rooms);
                }
                else
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
            } catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetPremiumRoomStore()
        {
            try
            {
                var room = await _unitOfWork.RoomRepository.GetSupRoomAsync();

                var preRoom = room
                    .Where(c => c.Store.StorePackages.Any(sp => sp.PackageId == 3 && sp.Status == true) && c.Status == true)
                    .Select(pr => new PremiumStore
                        {
                            RoomId = pr.Id,
                            RoomImage = pr.ImageRooms.FirstOrDefault(img => !img.ImageUrl.Contains("MENU"))?.ImageUrl,
                            StoreName = pr.Store.Name
                        })
                    .ToList();

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, preRoom);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
    }
}

