using StudySpace.Common;
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
    public interface IPackageService
    {
        Task<IBusinessResult> GetAll(int pageNumber, int pageSize);
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(Package package);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(Package package);
    }

    public class PackageService : IPackageService
    {
        private readonly UnitOfWork _unitOfWork;

        public PackageService()
        {
            _unitOfWork ??= new UnitOfWork();
        }

        public async Task<IBusinessResult> GetAll(int pageNumber, int pageSize)
        {
            try
            {
                #region Business rule
                #endregion

                var packages = await _unitOfWork.PackageRepository.GetAllPackagesAsync();

                if (packages == null || !packages.Any())
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                var pagedPackages = packages.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var packageList = pagedPackages.Select(r => new PackageResponseDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Fee = r.Fee,
                    Duration = r.Duration,
                    Description = r.Description?.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(part => part.Trim())
                                                .ToArray() 
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, packageList);
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


                var obj = await _unitOfWork.PackageRepository.GetByIdAsync(id);

                if (obj == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    var descriptionParts = obj.Description?.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                                                           .Select(part => part.Trim())
                    .ToArray();

                    var packageResponse = new PackageResponseDTO
                    {
                        Id = obj.Id,
                        Name = obj.Name,
                        Fee = obj.Fee,
                        Duration = obj.Duration,
                        Description = descriptionParts
                    };

                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, packageResponse);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }


        public async Task<IBusinessResult> Save(Package package)
        {
            try
            {
                int result = await _unitOfWork.PackageRepository.CreateAsync(package);
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

        public async Task<IBusinessResult> Update(Package package)
        {
            try
            {

                int result = await _unitOfWork.PackageRepository.UpdateAsync(package);
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

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var obj = await _unitOfWork.PackageRepository.GetByIdAsync(id);
                if (obj != null)
                {

                    var result = await _unitOfWork.PackageRepository.RemoveAsync(obj);
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
    }
}
