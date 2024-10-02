using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudySpace.Data.Models;
using StudySpace.DTOs.LoginDTO;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/Accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accService;

        public AccountsController(IAccountService accService)
        {
            _accService ??= accService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _accService.GetAllUser();
            return Ok(result);
        }


        [HttpPost("login-authen")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var result = await _accService.Login(model.Email, model.Password);

            if (result.Status == 1)
            {
                return Ok(new { token = result.Data });
            }

            return BadRequest(result.Message);
        }

        [HttpPost("token-decode")]
        public IActionResult Decode([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token cannot be null or empty." });
            }

            try
            {
                var decodedInfo = _accService.DecodeToken(token);
                return Ok(decodedInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDeatailUser(int id)
        {
                return Ok(await _accService.GetById(id));
        }

        [HttpPost("email-sending-confirmation")]
        public async Task<IActionResult> SendConfirmEmail([FromBody] string email)
        {
            try
            {
                var token = await _accService.SendRegistrationEmailAsync(email);
                return Ok(new { Message = "Confirm email sent.", Token = token });
            } catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] AccountRegistrationRequestModel model, [FromQuery] string token)
        {
            var result = await _accService.Save(model, token);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id,  [FromForm] UpdateAccountModel model)
        {
            var result = await _accService.Update(id, model);
            return Ok(result);
        }

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UnactiveAccount(int id)
        {
            var result = await _accService.UnactiveUser(id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _accService.DeleteById(id);
            return Ok(result);
        }

        [HttpGet("dashboard/admin/total-accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            return Ok(await _accService.CalculateTotalAccountsByRoleAndStatus());
        }
    }
}
