// GolBet.Services/Mapping/MappingProfile.cs
using AutoMapper;
using GolBet.Entities;
using GolBet.Services.DTOs;

namespace GolBet.Services.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeos existentes del Módulo 5
        CreateMap<Match, MatchDto>();

        CreateMap<Match, MatchDetailDto>()
            .ForMember(dto => dto.TotalBets,
                       options => options.MapFrom(match => match.Bets.Count));

        // Nuevos mapeos del Módulo 6
        CreateMap<Team, TeamDto>();
        CreateMap<TeamFormDto, Team>().ReverseMap();
        CreateMap<MatchFormDto, Match>().ReverseMap();
    }
}
