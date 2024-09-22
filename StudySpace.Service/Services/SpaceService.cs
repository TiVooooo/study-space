﻿using StudySpace.Data.UnitOfWork;
using StudySpace.Data.Models;
using StudySpace.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudySpace.Common;

namespace StudySpace.Service.Services
{
    public interface ISpaceService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(Space space);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(Space space);
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

    }
}
