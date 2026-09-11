using SearchEngine.Application.Features.Users.DTOs;
using SearchEngine.Infrastructure.Identity.Entities;
using Mapster;

namespace SearchEngine.Infrastructure.Identity.Mappings;

public class UserMappingConfig
    : IRegister
{
    public void Register(
        TypeAdapterConfig config)
    {
        config.NewConfig<
            ApplicationUser,
            ReadUserResponse>()

            .Map(
                dest => dest.Username,
                src => src.UserName)

            .Map(
                dest => dest.Role,
                src => string.Empty);
    }
}

