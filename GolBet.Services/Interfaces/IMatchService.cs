// GolBet.Services/Interfaces/IMatchService.cs 

using AutoMapper;
using GolBet.Entities.Enums;
using GolBet.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GolBet.Services.Interfaces;

public interface IMatchService
{
    /// <summary>Match board: all active matches ordered by date.</summary> 
    Task<IEnumerable<MatchDto>> GetBoardAsync(MatchStatus? status = null);

    Task<MatchDetailDto?> GetDetailAsync(int id);
}