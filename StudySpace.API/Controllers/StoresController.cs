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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var result = await _storeService.Login(model.Email, model.Password);

            if (result.Status == 1)
            {
                return Ok(new { token = result.Data });
            }

            return BadRequest(result.Message);
        }

        [HttpPost("decode")]
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

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _storeService.Logout(token);

            if (result.Status == 1)
            {
                return Ok(result);
            }

            return BadRequest(result.Message);
        }
    }
}