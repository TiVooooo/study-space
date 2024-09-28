using Microsoft.IdentityModel.Tokens;
using StudySpace.Common;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.DTOs.LoginDTO;
using StudySpace.DTOs.TokenDTO;
using StudySpace.Service.Base;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IStoreService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(Store store);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(Store store);
        Task<IBusinessResult> Login(string email, string password);
        Task<IBusinessResult> Logout(string token);
        DecodeTokenResponseDTO DecodeToken(string token);

       Task<IBusinessResult> GetAllAddress();

    }

    public class StoreService : IStoreService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly string _jwtSecret = "s3cr3tKeyF0rJWT@2024!MustBe32Char$";

        public StoreService()
        {
            _unitOfWork ??= new UnitOfWork();

        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var obj = await _unitOfWork.StoreRepository.GetByIdAsync(id);
                if (obj != null)
                {

                    var result = await _unitOfWork.StoreRepository.RemoveAsync(obj);
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

        public async Task<IBusinessResult> GetAllAddress()
        {
            try
            {
                var address = _unitOfWork.StoreRepository.GetAll().Select(x => x.Address).Distinct().ToList();
                address.Add("All");
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG,address);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);

            }

        }

        public async Task<IBusinessResult> GetAll()
        {
            try
            {
                #region Business rule
                #endregion

                var objs = await _unitOfWork.StoreRepository.GetAllAsync();

                if (objs == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, objs);
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


                var obj = await _unitOfWork.StoreRepository.GetByIdAsync(id);

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
                return new BusinessResult(-4, ex.Message);
            }
        }


        public async Task<IBusinessResult> Save(Store store)
        {
            try
            {
                int result = await _unitOfWork.StoreRepository.CreateAsync(store);
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

        public async Task<IBusinessResult> Update(Store store)
        {
            try
            {

                int result = await _unitOfWork.StoreRepository.UpdateAsync(store);
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

        public async Task<IBusinessResult> Login(string email, string password)
        {
            var store = await _unitOfWork.StoreRepository.GetByEmailAsync(email);

            if (store == null || store.Password != password)
            {
                return new BusinessResult(Const.FAIL_LOGIN, Const.FAIL_LOGIN_MSG);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var accountId = store.Id.ToString() ?? throw new ArgumentNullException("Store ID cannot be null");
            var accountRole = "Store";

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, accountId),
                    new Claim(ClaimTypes.GivenName, store.Name),
                    new Claim(ClaimTypes.Email, store.Email),
                    new Claim(ClaimTypes.HomePhone, store.Phone),
                    new Claim(ClaimTypes.StreetAddress, store.Address),
                    new Claim(ClaimTypes.Uri, store.ThumbnailUrl)
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return new BusinessResult(Const.SUCCESS_LOGIN, Const.SUCCESS_LOGIN_MSG, new LoginResponseDTO
            {
                Token = jwtToken,
                RoleName = accountRole
            });
        }

        public DecodeTokenResponseDTO DecodeToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token), "Token cannot be null or empty.");
            }

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                throw new ArgumentException("Invalid token");
            }

            var jwtToken = handler.ReadJwtToken(token);

            var storeID = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var phone = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone")?.Value;
            var address = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress")?.Value;
            var roleName = "Store";
            var avaUrl = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri")?.Value;

            //var expiration = jwtToken.ValidTo;

            return new DecodeTokenResponseDTO
            {
                UserID = int.Parse(storeID),
                Name = name,
                Email = email,
                Phone = phone,
                Address = address,
                RoleName = roleName,
                avaURL = avaUrl,
                //Expiration = expiration
            };
        }

        public async Task<IBusinessResult> Logout(string token)
        {
            return new BusinessResult(Const.SUCCESS_LOGOUT, Const.SUCCESS_LOGOUT_MSG);
        }
    }
}

