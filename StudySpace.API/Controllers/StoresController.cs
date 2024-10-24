﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudySpace.Data.Models;
using StudySpace.DTOs.LoginDTO;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/Stores")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService ??= storeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _storeService.GetAll());
        }

        [HttpPost("login-authen")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var result = await _storeService.Login(model.Email, model.Password);

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
                var decodedInfo = _storeService.DecodeToken(token);
                return Ok(decodedInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("address")]
        public async Task<IActionResult> GettAllAddress() 
        {
            var result = await _storeService.GetAllAddress();
            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDeatailUser(int id)
        {
            return Ok(await _storeService.GetById(id));
        }

        [HttpPost("email-sending-confirmation")]
        public async Task<IActionResult> SendConfirmEmail([FromBody] string email)
        {
            try
            {
                var token = await _storeService.SendRegistrationEmailAsync(email);
                return Ok(new { Message = "Confirm email sent.", Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UnactiveStore(int id)
        {
            var result = await _storeService.UnactiveStore(id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            return Ok(await _storeService.DeleteById(id));
        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] StoreRegistrationRequestModel model, [FromQuery] string token)
        {
            return Ok(await _storeService.Save(model, token));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStore(int id, [FromForm] UpdateStoreModel model)
        {
            return Ok(await _storeService.Update(id, model));
        }
    }
}
