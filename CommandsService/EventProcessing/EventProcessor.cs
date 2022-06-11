using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IMapper mapper;
        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            this.mapper = mapper;
            this.scopeFactory = scopeFactory;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch (eventType)
            {
                case EventType.PlatformPublished:

                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            System.Console.WriteLine($"--> Determining event");
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (eventType.Event)
            {
                case "Platform_Published":
                    System.Console.WriteLine("--> Platform published event detected");
                    return EventType.PlatformPublished;
                default:
                    System.Console.WriteLine("--> Could not determine event type");
                    return EventType.Undetermined;
            }
        }

        private void addPlatform(string platformPublishedMessage)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
                try
                {
                    var plat = mapper.Map<Platform>(platformPublishedDto);

                    if (!repo.ExternalPlatformExists(plat.ExternalId))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                    }
                    else
                    {
                        System.Console.WriteLine("--> PLatform already exists");
                    }

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"--> Could not add platform to db: {ex.Message}");
                }
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}