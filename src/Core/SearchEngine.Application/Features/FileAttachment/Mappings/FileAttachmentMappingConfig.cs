using SearchEngine.Application.Features.FileAttachments.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;

namespace SearchEngine.Application.Features.FileAttachments.Mappings;

public class FileAttachmentMappingConfig
    : IRegister
{
    public void Register(
        TypeAdapterConfig config)
    {
        config.NewConfig<
            FileAttachment,
            ReadFileAttachmentResponse>();
    }
}

