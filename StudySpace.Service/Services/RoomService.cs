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

namespace StudySpace.Service.Services
{
   
        public interface IRoomService
        {
            Task<IBusinessResult> GetAll(int pageNumber, int pageSize);
            Task<IBusinessResult> GetById(int id);
            Task<IBusinessResult> Update(Room space);
            Task<IBusinessResult> DeleteById(int id);
            Task<IBusinessResult> Save(Room space);
        }

        public class RoomService : IRoomService
        {
            private readonly UnitOfWork _unitOfWork;

            public RoomService()
            {
                _unitOfWork ??= new UnitOfWork();

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

        /*public async Task<IBusinessResult> SearchRooms(int pageNumber, int pageSize, string space, string location, string room, int person)
        {
            try
            {
                var rooms = await _unitOfWork.RoomRepository.GetAllRoomsAsync();

                // Filter based on the search criteria
                if (!string.IsNullOrWhiteSpace(space) || !string.IsNullOrWhiteSpace(location) || !string.IsNullOrWhiteSpace(room) || person == 0)
                {
                    rooms = rooms.Where(r =>
                        (string.IsNullOrWhiteSpace(space) || r.Space?.SpaceName.Contains(space, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(location) || r.Store?.Address.Contains(location, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(room) || r.RoomName.Contains(room, StringComparison.OrdinalIgnoreCase)) &&
                        (r.Capacity >= person)
                    ).ToList();
                }

                // Apply pagination
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
*/

        public async Task<IBusinessResult> GetById(int id)
            {
                try
                {
                    #region Business rule
                    #endregion


                    var payment = await _unitOfWork.RoomRepository.GetByIdAsync(id);

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
