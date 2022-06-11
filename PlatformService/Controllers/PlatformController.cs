using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataService.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepo platformRepo;
        private readonly IMapper mapper;
        private readonly ICommandDataClient commandDataClient;
        private readonly IMessageBusClient messageBusClient;

        public PlatformController(
            IPlatformRepo platformRepo, 
            IMapper mapper,
            ICommandDataClient commandDataClient,
            IMessageBusClient messageBusClient)
        {
            this.platformRepo = platformRepo;
            this.mapper = mapper;
            this.commandDataClient = commandDataClient;
            this.messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            System.Console.WriteLine("--> Getting platforms....");

            var platformItems = platformRepo.GetAllPlatforms();
            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platformItem = platformRepo.GetPlatformById(id);
            if (platformItem != null)
            {
                return Ok(mapper.Map<PlatformReadDto>(platformItem));
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            var platformModel = mapper.Map<Platform>(platformCreateDto);
            platformRepo.CreatePlatform(platformModel);
            platformRepo.SaveChanges();

            var platformReadDto = mapper.Map<PlatformReadDto>(platformModel);

            try
            {
                await commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch(Exception e)
            {
                System.Console.WriteLine($"--> Could not send synchronously: {e.Message}");
            }

            try
            {
                var platformPublishedDto = mapper.Map<PlatformPublishedDto>(platformReadDto);
                platformPublishedDto.Event = "Platform_Published";
                messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"--> Could not send asynchronously: {e.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
        }

    }
}