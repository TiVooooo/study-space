using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudySpace.Data.Models;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accService;

        public AccountsController(IAccountService accService)
        {
            _accService ??= accService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Index()
        {
            var result = await _accService.GetAll();
            return Ok(result);
        }

    }
}
