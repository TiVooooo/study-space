using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/Packages")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackagesController(IPackageService packageService)
        {
            _packageService ??= packageService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _packageService.GetById(id));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize)
        {
            var result = await _packageService.GetAll(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            return Ok(await _packageService.DeleteById(id));
        }
    }
}
