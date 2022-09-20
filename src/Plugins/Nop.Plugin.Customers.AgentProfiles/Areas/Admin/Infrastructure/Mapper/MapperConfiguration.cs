using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Models.Customers;
using Nop.Plugin.Customers.AgentProfiles.Domains;

namespace Nop.Plugin.Customers.AgentProfiles.Areas.Admin.Infrastructure.Mapper
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<AgentModel, Agent>()
                .ReverseMap();
        }

        public int Order => 1;
    }
}
