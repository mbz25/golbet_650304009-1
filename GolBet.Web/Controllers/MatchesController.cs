// GolBet.Web/Controllers/MatchesController.cs 
using Microsoft.AspNetCore.Authorization;
using GolBet.Repositories.Data;
using GolBet.Entities.Enums;
using GolBet.Services.DTOs;
using GolBet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolBet.Web.Controllers;

public class MatchesController : Controller
{
    private readonly IMatchService _matchService;
    private readonly ITeamService _teamService;

    public MatchesController(IMatchService matchService, ITeamService teamService)
    {
        _matchService = matchService;
        _teamService = teamService;
    }

    public async Task<IActionResult> Index(MatchStatus? status)
    {
        ViewBag.CurrentStatus = status;
        var board = await _matchService.GetBoardAsync(status);
        return View(board);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var match = await _matchService.GetDetailAsync(id);
        if (match is null) return NotFound();

        return View(match);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(MatchFormDto model)
    {
        if (ModelState.IsValid)
        {
            await _matchService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name", model.HomeTeamId);
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var match = await _matchService.GetForEditAsync(id);
        if (match == null) return NotFound();

        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name", match.HomeTeamId);
        return View(match);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, MatchFormDto model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await _matchService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name", model.HomeTeamId);
        return View(model);
    }
}