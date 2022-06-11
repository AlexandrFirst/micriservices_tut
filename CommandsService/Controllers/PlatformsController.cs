using System.Collections.Generic;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ICommandRepo repository;
        private readonly IMapper mapper;
        public PlatformsController(ICommandRepo repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;

        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            System.Console.WriteLine("--> Getting Platforms from CommandService");

            var platformItems = repository.GetAllPlatforms();

            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpPost]
        public ActionResult TestInboundConnection()
        {
            System.Console.WriteLine("--> Inbound POST # Command Service");
            return Ok("Inbound test ok from Platforms Controller");
        }
    }
}