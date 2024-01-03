using AutoMapper;
using Oip.Security.Bl.Dtos.Log;
using Oip.Security.Dal.Common;
using Oip.Security.Dal.Entities;
using Skoruba.AuditLogging.EntityFramework.Entities;

namespace Oip.Security.Bl.Mappers;

public class LogMapperProfile : Profile
{
    public LogMapperProfile()
    {
        CreateMap<Log, LogDto>(MemberList.Destination)
            .ReverseMap();

        CreateMap<PagedList<Log>, LogsDto>(MemberList.Destination)
            .ForMember(x => x.Logs, opt => opt.MapFrom(src => src.Data));

        CreateMap<AuditLog, AuditLogDto>(MemberList.Destination)
            .ReverseMap();

        CreateMap<PagedList<AuditLog>, AuditLogsDto>(MemberList.Destination)
            .ForMember(x => x.Logs, opt => opt.MapFrom(src => src.Data));
    }
}