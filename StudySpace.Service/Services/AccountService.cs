﻿using AutoMapper;
using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudySpace.Common;
using StudySpace.Data.Helper;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.DTOs.LoginDTO;
using StudySpace.DTOs.TokenDTO;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudySpace.Service.Services
{
    public interface IAccountService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(int id, UpdateAccountModel account);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(AccountRegistrationRequestModel model, string token);
        Task<IBusinessResult> Login(string email, string password);
        DecodeTokenResponseDTO DecodeToken(string token);
        Task<string> SendRegistrationEmailAsync(string email);
        Task<IBusinessResult> UnactiveUser(int userId);
        Task<IBusinessResult> CalculateTotalAccountsByRoleAndStatus();

        Task<IBusinessResult> GetAllUser();

    }

    public class AccountService : IAccountService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly string _jwtSecret = "s3cr3tKeyF0rJWT@2024!MustBe32Char$";
        private readonly string _confirmUrl;
        private readonly IFirebaseService _firebaseService;

        public AccountService(IMapper mapper, IEmailService emailService, IConfiguration config, IFirebaseService firebaseService)
        {
            _unitOfWork ??= new UnitOfWork();
            _mapper = mapper;
            _emailService = emailService;
            _confirmUrl = config["ConfirmUrl"];
            _firebaseService = firebaseService;
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {

                var obj = await _unitOfWork.AccountRepository.GetByIdAsync(id);
                if (obj != null)
                {

                    var result = await _unitOfWork.AccountRepository.RemoveAsync(obj);
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

                var objs = await _unitOfWork.AccountRepository.GetAllAsync();

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
                var obj = await _unitOfWork.AccountRepository.GetByIdAsync(id);

                if (obj == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }
                else
                {
                    

                    var userModel = _mapper.Map<GetDetailUserModel>(obj);
                    var roleName = _unitOfWork.UserRoleRepository.FindByCondition(r=>r.Id == obj.RoleId).Select(r=>r.RoleName).FirstOrDefault();
                    userModel.RoleName = roleName;
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, userModel);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> Save(AccountRegistrationRequestModel model, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                if(email == null)
                {
                    return new BusinessResult(Const.WARNING_INVALID_TOKEN, Const.WARNING_INVALID_TOKEN_MSG);
                }

                var userRole = await _unitOfWork.AccountRepository.GetRole(model.RoleName);

                if (userRole == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Role not found.");
                }

               
                var newAcc = new Account
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone,
                    Address = model.Address,
                    Gender = model.Gender,
                    Dob = model.Dob,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Wallet = 0,
                    RoleId = userRole.Id,
                    AvatarUrl = "https://scontent.fsgn3-1.fna.fbcdn.net/v/t39.30808-6/460988998_122109837560414676_6957837609958495355_n.jpg?_nc_cat=104&ccb=1-7&_nc_sid=6ee11a&_nc_eui2=AeGxasUF1_cqgAkICzwV6pNWkrJeYIPbAnySsl5gg9sCfLwS-qjG0dNHVAW1YSj7J_zUqzbG21mFXwWepYn1e5mA&_nc_ohc=FUj7X_J07FIQ7kNvgFT9YnZ&_nc_zt=23&_nc_ht=scontent.fsgn3-1.fna&oh=00_AYDEwjSOMughY8zhvOx1jvo0-dGrxz80SlsqrENQ4edGNQ&oe=67009BD9"
                };

                _unitOfWork.AccountRepository.PrepareCreate(newAcc);
                int result = _unitOfWork.AccountRepository.Save();

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                } else
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }

            }
            catch (SecurityTokenExpiredException ex)
            {
                return new BusinessResult(-4, "Token has expired. Please request a new registration email.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(-4, ex.Message);
            }
        }

        public async Task<IBusinessResult> Update(int id, UpdateAccountModel account)
        {
            try
            {
                var existedUser = await _unitOfWork.AccountRepository.GetByIdAsync(id);

                if(existedUser == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                existedUser.Name = account.Name;
                existedUser.Email = account.Email;
                existedUser.Password = account.Password;
                existedUser.Phone = account.Phone;
                existedUser.Address = account.Address;
                existedUser.Gender = account.Gender;
                existedUser.Dob = account.Dob;

                var imageUrl = account.AvatarUrl;
                if (imageUrl != null)
                {
                    var imagePath = FirebasePathName.AVATAR + Guid.NewGuid().ToString();
                    var imgUploadResult = await _firebaseService.UploadImageToFirebaseAsync(imageUrl, imagePath);
                    existedUser.AvatarUrl = imgUploadResult;
                    _unitOfWork.AccountRepository.PrepareCreate(existedUser);
                }

                int result = await _unitOfWork.AccountRepository.UpdateAsync(existedUser);

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
            var acc = await _unitOfWork.AccountRepository.GetByEmailAsync(email);

            if (acc == null || acc.Password != password)
            {
                return new BusinessResult(Const.FAIL_LOGIN, Const.FAIL_LOGIN_MSG);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var accountId = acc.Id.ToString() ?? throw new ArgumentNullException("Account ID cannot be null");
            var accountRole = acc.Role?.RoleName ?? "User";

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, acc.Id.ToString()),
                    new Claim(ClaimTypes.GivenName, acc.Name),
                    new Claim(ClaimTypes.Email, acc.Email),
                    new Claim(ClaimTypes.HomePhone, acc.Phone),
                    new Claim(ClaimTypes.StreetAddress, acc.Address),
                    new Claim(ClaimTypes.Gender, acc.Gender),
                    new Claim(ClaimTypes.Role, accountRole),
                    new Claim(ClaimTypes.Uri, acc.AvatarUrl)
                }),
                //Expires = DateTime.UtcNow.AddHours(2),
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

            var userID = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var phone = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone")?.Value;
            var address = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress")?.Value;
            var gender = jwtToken.Claims.FirstOrDefault(c => c.Type == "gender")?.Value;
            var roleName = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var avaUrl = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri")?.Value;

            //var expiration = jwtToken.ValidTo;

            return new DecodeTokenResponseDTO
            {
                UserID = int.Parse(userID),
                Name = name,
                Email = email,
                Phone = phone,
                Address = address,
                Gender = gender,
                RoleName = roleName,
                avaURL = avaUrl,
                //Expiration = expiration
            };
        }

        public async Task<string> SendRegistrationEmailAsync(string email)
        {
            var existedAcc = await _unitOfWork.AccountRepository.GetByEmailAsync(email);
            if(existedAcc != null)
            {
                throw new InvalidOperationException("Email is already existed!");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            var confirmLink = $"{_confirmUrl}?token={jwtToken}&email={email}";
            await _emailService.SendMailAsync(email, confirmLink, $"30ph đéo bấm thì mất acc con ạ: {confirmLink}");

            return jwtToken;
        }

        public async Task<IBusinessResult> UnactiveUser(int userId)
        {
            try
            {
                var userUnactive = await _unitOfWork.AccountRepository.GetByIdAsync(userId);

                if (userUnactive == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                userUnactive.IsActive = !userUnactive.IsActive;

                int result = await _unitOfWork.AccountRepository.UpdateAsync(userUnactive);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UNACTIVATE, Const.SUCCESS_UNACTIVATE_MSG);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UNACTIVATE, Const.FAIL_UNACTIVATE_MSG);
                }
            } catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> CalculateTotalAccountsByRoleAndStatus()
        {
            try
            {
                var admins = _unitOfWork.AccountRepository
                    .FindByCondition(a => a.Role.RoleName == "Admin")
                    .ToList();

                var activeUsers = _unitOfWork.AccountRepository
                    .FindByCondition(a => a.Role.RoleName == "User" && a.IsActive == true)
                    .ToList();

                var inactiveUsers = _unitOfWork.AccountRepository
                    .FindByCondition(a => a.Role.RoleName == "User" && a.IsActive == false)
                    .ToList();

                int totalAdmins = admins.Count;
                int totalActiveUsers = activeUsers.Count;
                int totalInactiveUsers = inactiveUsers.Count;

                int totalUserAccounts = totalActiveUsers + totalInactiveUsers;

                int totalAccounts = totalAdmins + totalUserAccounts;

                var response = new
                {
                    TotalAdmins = totalAdmins,
                    TotalActiveUsers = totalActiveUsers,
                    TotalInactiveUsers = totalInactiveUsers,
                    TotalUserAccounts = totalUserAccounts,
                    TotalAccounts = totalAccounts
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllUser()
        {
            try
            {
                var users = _unitOfWork.AccountRepository.GetAll();
                var result = new List<UserModel>();
                foreach (var user in users)
                {
                    var role = _unitOfWork.UserRoleRepository.GetById(user.RoleId ??0);
                    var userModel = new UserModel
                    {
                        AvatarUrl = user.AvatarUrl,
                        Email = user.Email,
                        Name = user.Name,
                        Phone = user.Phone,
                        RoleName = role.RoleName
                    };
                    result.Add(userModel);
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);

            }
        }

        public async Task<IBusinessResult> GetAllUserv2()
        {
            try
            {
                var users = _unitOfWork.AccountRepository.GetAll();
                var result = _mapper.Map<List<UserModel>>(users);
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);

            }
        }


    }
}
