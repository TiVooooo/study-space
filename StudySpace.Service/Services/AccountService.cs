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
using StudySpace.Service.Helper;
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
        Task<IBusinessResult> GetAllUser();
    }

    public class AccountService : IAccountService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly string _jwtSecret = "s3cr3tKeyF0rJWT@2024!MustBe32Char$";
        private readonly string _confirmUrl;
        private readonly string _frontendUrl;
        private readonly IFirebaseService _firebaseService;

        public AccountService(IMapper mapper, IEmailService emailService, IConfiguration config, IFirebaseService firebaseService)
        {
            _unitOfWork ??= new UnitOfWork();
            _mapper = mapper;
            _emailService = emailService;
            _confirmUrl = config["ConfirmUrl"];
            _frontendUrl = config["FrontEndUrl"];
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
            var existedAcc = await _unitOfWork.AccountRepository.GetByEmailAsync(model.Email);
            if (existedAcc != null)
            {
                throw new InvalidOperationException("Email is already existed!");
            }

            var hasUpperChar = model.Password.Any(char.IsUpper);
            var hasLowerChar = model.Password.Any(char.IsLower);
            var hasNumber = model.Password.Any(char.IsDigit);
            var hasSpecialChar = model.Password.Any(ch => !char.IsLetterOrDigit(ch));

            if (!(hasUpperChar && hasLowerChar && hasNumber && hasSpecialChar))
            {
                return new BusinessResult(Const.FAIL_CREATE, "Mật khẩu phải có ít nhất một chữ hoa, chữ thường, số và ký tự đặc biệt!");
            }

            if (model.Phone.Length != 10 || !model.Phone.All(char.IsDigit))
            {
                return new BusinessResult(Const.FAIL_CREATE, "Số điện thoại phải là 10 chữ số!");
            }

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
                    Password = PasswordHashHelper.HashPassword(model.Password),
                    Phone = model.Phone,
                    Address = model.Address,
                    Gender = model.Gender,
                    Dob = model.Dob,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Wallet = 0,
                    RoleId = userRole.Id,
                    AvatarUrl = "https://i.ibb.co/V23Jp9d/LOGO-SS-02.png"
                };

                _unitOfWork.AccountRepository.PrepareCreate(newAcc);
                int result = _unitOfWork.AccountRepository.Save();

                if (result > 0)
                {
                    var subject = "Chào mừng bạn đến với StudySpace!";
                    var body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Đăng ký thành công</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            padding: 20px;
        }}
        .container {{
            background-color: #fff;
            border-radius: 8px;
            padding: 20px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            color: #333;
        }}
        .btn {{
            display: inline-block;
            margin: 20px 0;
            padding: 10px 20px;
            background-color: #007BFF;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            font-size: 0.9em;
            color: #777;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1 class='header'>Chào mừng bạn đến với StudySpace, {model.Name}!</h1>
        <p>Chúc mừng bạn đã đăng ký tài khoản thành công. Bạn có thể đăng nhập vào hệ thống của chúng tôi bằng cách nhấp vào liên kết dưới đây:</p>
        <a href='{_frontendUrl}/login' class='btn'>Đăng nhập ngay</a>
        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
        <p>Trân trọng,<br>Đội ngũ StudySpace</p>
    </div>
    <div class='footer'>
        <p>Bạn nhận được email này vì đã đăng ký tài khoản trên StudySpace.</p>
    </div>
</body>
</html>
";

                    await _emailService.SendMailAsync(model.Email, subject, body);

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, model);
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

                existedUser.Name = string.IsNullOrEmpty(account.Name) ? existedUser.Name : account.Name;

                if (!string.IsNullOrEmpty(account.Password))
                {
                    var isPassword = PasswordHashHelper.VerifyPassword(account.Password, existedUser.Password);

                    if (!isPassword)
                    {
                        return new BusinessResult(Const.FAIL_UDATE, "Current password is incorrect!");
                    }

                    if(!string.IsNullOrEmpty(account.NewPassword) && account.NewPassword == account.ConfirmPassword)
                    {
                        existedUser.Password = PasswordHashHelper.HashPassword(account.NewPassword);
                    } else
                    {
                        return new BusinessResult(Const.FAIL_UDATE, "New password and Confirm password does not match !");
                    }
                }

                existedUser.Phone = string.IsNullOrEmpty(account.Phone) ? existedUser.Phone : account.Phone;
                existedUser.Address = string.IsNullOrEmpty (account.Address) ? existedUser.Address : account.Address;

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
                    return new BusinessResult(Const.SUCCESS_UDATE, Const.SUCCESS_UDATE_MSG, account);
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
            var stranger = await _unitOfWork.StoreRepository.GetByEmailAsync(email);
            var acc = await _unitOfWork.AccountRepository.GetByEmailAsync(email);

            if (stranger != null)
            {
                return new BusinessResult(Const.FAIL_LOGIN, "You don't have permission to login this service.");
            }


            if (acc == null || !PasswordHashHelper.VerifyPassword(password, acc.Password))
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
            //var avaUrl = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri")?.Value;

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
                //avaURL = avaUrl,
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


            var subject = "Chỉ còn một bước nữa để hoàn tất đăng ký của bạn!";
            var body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác nhận đăng ký</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            padding: 20px;
        }}
        .container {{
            background-color: #fff;
            border-radius: 8px;
            padding: 20px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            color: #333;
        }}
        .btn {{
            display: inline-block;
            margin: 20px 0;
            padding: 10px 20px;
            background-color: #007BFF;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            font-size: 0.9em;
            color: #777;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1 class='header'>Chào mừng bạn đến với StudySpace!</h1>
        <p>Cảm ơn bạn đã đăng ký tài khoản của chúng tôi. Để hoàn tất quá trình đăng ký, vui lòng nhấp vào liên kết dưới đây:</p>
        <a href='{confirmLink}' class='btn'>Xác nhận tài khoản</a>
        <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
        <p>Trân trọng,<br>Đội ngũ StudySpace</p>
    </div>
    <div class='footer'>
        <p>Bạn nhận được email này vì đã đăng ký tài khoản trên StudySpace.</p>
    </div>
</body>
</html>
";


            await _emailService.SendMailAsync(email, subject, body);

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

        public async Task<IBusinessResult> GetAllUser()
        {
            try
            {
                var users = _unitOfWork.AccountRepository.GetAllAccounts();

                var results = users.Select(u => new UserModel
                {
                    Id = u.Id,
                    RoleName = u.Role.RoleName,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    Gender = u.Gender,
                    DOB = u.Dob,
                    IsActive = u.IsActive,
                    Wallet = u.Wallet,
                    AvatarUrl = u.AvatarUrl,
                    Date = u.CreatedDate
                }).ToList();
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, results);
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
