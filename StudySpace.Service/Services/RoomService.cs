﻿using Firebase.Auth;
using StudySpace.Common;
using StudySpace.Data.Helper;
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
    }

    public class RoomService : IRoomService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IFeedbackService _feedbackService;
        private readonly IFirebaseService _firebaseService;

        public RoomService(IFeedbackService feedbackService, IFirebaseService firebaseService)
        {
            _unitOfWork ??= new UnitOfWork();
            _feedbackService = feedbackService;
            _firebaseService = firebaseService;
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


                var list = new List<RoomModel>();

                foreach (var r in rooms)
                {
                    var store = _unitOfWork.StoreRepository.GetById(r.StoreId);
                    var amity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == r.Id).Select(a=>a.Type).FirstOrDefault();
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
                var rooms = _unitOfWork.RoomRepository.FindByCondition(x=>x.Status ==true).ToList();

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
                else if (!room.Status)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                var listImageOfRoom = _unitOfWork.ImageRoomRepository.FindByCondition(i => i.RoomId == id).Select(i => i.ImageUrl).ToList();
                var listAminity = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == id).Select(a => a.Name).ToList();
                var type = _unitOfWork.AmityRepository.FindByCondition(a => a.RoomId == id).Select(a => a.Type).FirstOrDefault();

                var feedbackResult = await _feedbackService.GetFeedback(id);

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
                    Feedbacks = feedbackResult.Data as List<FeedbackResponseModel>,
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



        public async Task<IBusinessResult> Save(CreateRoomRequestModel room)
        {
            try
            {
                var storeExisted = _unitOfWork.StoreRepository.FindByCondition(c => c.Id == room.StoreId).FirstOrDefault();
                var spaceExisted = _unitOfWork.SpaceRepository.FindByCondition(c => c.Id == room.SpaceId).FirstOrDefault();

                if(storeExisted == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, "Unknown Store.");
                }

                if(spaceExisted == null)
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
                    Description = room.Description,
                    Status = true,
                    Area = room.Area,
                    HouseRule = room.HouseRule
                };

                _unitOfWork.RoomRepository.PrepareCreate(newRoom);

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
                int result = await _unitOfWork.RoomRepository.SaveAsync();
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

                roomUnactive.Status = false;

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

    }
}
