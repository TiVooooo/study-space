using AutoMapper;
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

namespace StudySpace.Service.Services
{
    public interface IStoreService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> Update(int storeId, UpdateStoreModel model);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> Save(StoreRegistrationRequestModel model, string token);
        Task<IBusinessResult> Login(string email, string password);
        DecodeTokenResponseDTO DecodeToken(string token);
        Task<string> SendRegistrationEmailAsync(string email);
        Task<IBusinessResult> GetAllAddress();
        Task<IBusinessResult> UnactiveStore(int storeID);
    }

    public class StoreService : IStoreService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly string _confirmUrl;
        private readonly IMapper _mapper;
        private readonly string _jwtSecret = "s3cr3tKeyF0rJWT@2024!MustBe32Char$";
        private readonly IFirebaseService _firebaseService;
        private readonly string _registerSuccessURL = "https//front-end";

        public StoreService(IMapper mapper, IEmailService emailService, IConfiguration config, IFirebaseService firebaseService)
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
                var addresses = _unitOfWork.StoreRepository.GetAll()
                    .Select(x => x.Address)
                    .Distinct()
                    .ToList();

                var districts = addresses.Select(address =>
                {
                    var parts = address.Split(',');
                    return parts.Length > 1 ? parts[1].Trim() : string.Empty; 
                })
                .Distinct() 
                .ToList();

                districts.Insert(0, "All");

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, districts);
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


        public async Task<IBusinessResult> Save(StoreRegistrationRequestModel model, string token)
        {
            var existedAcc = await _unitOfWork.StoreRepository.GetByEmailAsync(model.Email);
            if (existedAcc != null)
            {
                throw new InvalidOperationException("Email is already existed!");
            }

            if (model.Phone.Length != 10 || !model.Phone.All(char.IsDigit))
            {
                return new BusinessResult(Const.FAIL_CREATE, "Invalid PhoneNumber Format!");
            }

            var hasUpperChar = model.Password.Any(char.IsUpper);
            var hasLowerChar = model.Password.Any(char.IsLower);
            var hasNumber = model.Password.Any(char.IsDigit);
            var hasSpecialChar = model.Password.Any(ch => !char.IsLetterOrDigit(ch));

            if (!(hasUpperChar && hasLowerChar && hasNumber && hasSpecialChar))
            {
                return new BusinessResult(Const.FAIL_CREATE, "Invalid Password Format!");
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

                if (email == null)
                {
                    return new BusinessResult(Const.WARNING_INVALID_TOKEN, Const.WARNING_INVALID_TOKEN_MSG);
                }

                var newStore = new Store
                {
                    ThumbnailUrl = "https://scontent.fsgn3-1.fna.fbcdn.net/v/t39.30808-6/460988998_122109837560414676_6957837609958495355_n.jpg?_nc_cat=104&ccb=1-7&_nc_sid=6ee11a&_nc_eui2=AeGxasUF1_cqgAkICzwV6pNWkrJeYIPbAnySsl5gg9sCfLwS-qjG0dNHVAW1YSj7J_zUqzbG21mFXwWepYn1e5mA&_nc_ohc=FUj7X_J07FIQ7kNvgFT9YnZ&_nc_zt=23&_nc_ht=scontent.fsgn3-1.fna&oh=00_AYDEwjSOMughY8zhvOx1jvo0-dGrxz80SlsqrENQ4edGNQ&oe=67009BD9",
                    Longitude = 0,
                    Latitude = 0,
                    Description = model.Description,
                    Status = true,
                    IsApproved = false,
                    Name = model.Name,
                    Email = model.Email,
                    Password = PasswordHashHelper.HashPassword(model.Password),
                    Address = model.Address,
                    Phone = model.Phone,
                    CreateDate = DateTime.Now,
                    OpenTime = model.OpenTime,
                    CloseTime = model.CloseTime,
                    IsOverNight = model.IsOverNight,
                    IsActive = true,
                    TaxNumber = "-1",
                    PostalNumber = "-1",
                };

                _unitOfWork.StoreRepository.PrepareCreate(newStore);
                int result = _unitOfWork.AccountRepository.Save();

                if (result > 0)
                {
                    var subject = "Đăng ký thành công!";
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
                        background-color: #28a745;
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
                    <h1 class='header'>Chúc mừng bạn đã đăng ký thành công!</h1>
                    <p>Store của bạn đã được đăng ký thành công. Bạn có thể truy cập vào dashboard của store thông qua liên kết dưới đây:</p>
                    <a href='{_registerSuccessURL}' class='btn'>Truy cập Store Dashboard</a>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                    <p>Trân trọng,<br>Đội ngũ StudySpace</p>
                </div>
                <div class='footer'>
                    <p>Bạn nhận được email này vì đã đăng ký tài khoản trên StudySpace.</p>
                </div>
            </body>
            </html>
            ";
                    await _emailService.SendMailAsync(newStore.Email, subject, body);
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newStore);
                }
                else
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

        public async Task<IBusinessResult> Update(int storeId, UpdateStoreModel model)
        {
            try
            {
                var existedStore = await _unitOfWork.StoreRepository.GetByIdAsync(storeId);

                if (existedStore == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                existedStore.Description = string.IsNullOrEmpty(model.Description) ? existedStore.Description : model.Description;
                existedStore.Name = string.IsNullOrEmpty(model.Name) ? existedStore.Name : model.Name;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    var isPassword = PasswordHashHelper.VerifyPassword(model.Password, existedStore.Password);

                    if (!isPassword)
                    {
                        return new BusinessResult(Const.FAIL_UDATE, "Current password is incorrect!");
                    }

                    if (!string.IsNullOrEmpty(model.NewPassword) && model.NewPassword == model.ConfirmPassword)
                    {
                        existedStore.Password = PasswordHashHelper.HashPassword(model.NewPassword);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UDATE, "New password and Confirm password does not match !");
                    }
                }

                existedStore.Address = string.IsNullOrEmpty(model.Address) ? existedStore.Address : model.Address;
                existedStore.Phone = string.IsNullOrEmpty(model.Phone) ? existedStore.Phone : model.Phone;
                existedStore.OpenTime = model.OpenTime ?? existedStore.OpenTime;
                existedStore.CloseTime = model.CloseTime ?? existedStore.CloseTime;
                existedStore.IsOverNight = model.IsOverNight ?? existedStore.IsOverNight;

                var imageUrl = model.ThumbnailUrl;
                if (imageUrl != null)
                {
                    var imagePath = FirebasePathName.AVATAR + Guid.NewGuid().ToString();
                    var imgUploadResult = await _firebaseService.UploadImageToFirebaseAsync(imageUrl, imagePath);
                    existedStore.ThumbnailUrl = imgUploadResult;
                    _unitOfWork.StoreRepository.PrepareCreate(existedStore);
                }

                int result = await _unitOfWork.StoreRepository.UpdateAsync(existedStore);

                if (result > 0)
                {
                    return new BusinessResult(Const.FAIL_UDATE, Const.SUCCESS_UDATE_MSG, existedStore);
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

            if (store == null || !PasswordHashHelper.VerifyPassword(password, store.Password))
            {
                return new BusinessResult(Const.FAIL_LOGIN, Const.FAIL_LOGIN_MSG);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var accountId = store.Id.ToString() ?? throw new ArgumentNullException("Store ID cannot be null");
            var accountRole = "Store";

            var packagedList = _unitOfWork.StorePackageRepository
                    .FindByCondition(c => c.StoreId == store.Id && c.Status == true).ToList();

            bool isPackaged = packagedList.Count > 0;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, accountId),
                    new Claim(ClaimTypes.GivenName, store.Name),
                    new Claim(ClaimTypes.Email, store.Email),
                    new Claim(ClaimTypes.HomePhone, store.Phone),
                    new Claim(ClaimTypes.StreetAddress, store.Address),
                    new Claim(ClaimTypes.Uri, store.ThumbnailUrl),
                    new Claim("RoleName", accountRole),
                    new Claim("isPackaged", isPackaged.ToString())
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return new BusinessResult(Const.SUCCESS_LOGIN, Const.SUCCESS_LOGIN_MSG, new
            {
                LoginInformation = new LoginResponseDTO
                {
                    Token = jwtToken,
                    RoleName = accountRole
                }
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
            var roleName = jwtToken.Claims.FirstOrDefault(c => c.Type == "RoleName")?.Value;
            var avaUrl = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri")?.Value;
            var isPacaged = jwtToken.Claims.FirstOrDefault(c => c.Type == "isPackaged")?.Value;

            //var expiration = jwtToken.ValidTo;

            return new DecodeTokenResponseDTO
            {
                UserID = int.Parse(storeID),
                Name = name,
                Email = email,
                Phone = phone,
                Address = address,
                RoleName = roleName,
                AvaURL = avaUrl,
                IsPackaged = isPacaged
                //Expiration = expiration
            };
        }

        public async Task<string> SendRegistrationEmailAsync(string email)
        {
            var existedAcc = await _unitOfWork.StoreRepository.GetByEmailAsync(email);
            if (existedAcc != null)
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

        public async Task<IBusinessResult> UnactiveStore(int storeID)
        {
            try
            {
                var storeUnactive = await _unitOfWork.StoreRepository.GetByIdAsync(storeID);

                if (storeUnactive == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
                }

                storeUnactive.IsActive = !storeUnactive.IsActive;

                int result = await _unitOfWork.StoreRepository.UpdateAsync(storeUnactive);

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

