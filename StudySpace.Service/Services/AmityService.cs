using AutoMapper;
using StudySpace.Common;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IAmityService
    {
        Task<BusinessResult> GetAllAmities();

    }
    public class AmityService : IAmityService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AmityService(IMapper mapper)
        {
            _unitOfWork ??= new UnitOfWork();
            
            _mapper = mapper;
        }

        public async Task<BusinessResult> GetAllAmities()
        {
            try
            {
                var amities = _unitOfWork.AmityRepository.GetAll();

                var name = amities.Select(a => a.Name).ToList();
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, name);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }
    }
}
