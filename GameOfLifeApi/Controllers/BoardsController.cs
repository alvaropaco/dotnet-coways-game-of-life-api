using GameOfLifeApi.DTOs;
using GameOfLifeApi.Models;
using GameOfLifeApi.Repositories;
using GameOfLifeApi.Services;
using GameOfLifeApi.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace GameOfLifeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IBoardRepository _repo;
    private readonly IGameOfLifeService _life;

    public BoardsController(IBoardRepository repo, IGameOfLifeService life)
    {
        _repo = repo;
        _life = life;
    }

    [HttpPost]
    public ActionResult<Guid> Upload([FromBody] UploadBoardRequest request)
    {
        var (ok, error) = GridUtils.ValidateRectangular(request.Grid);
        if (!ok) return BadRequest(new { message = error });

        var board = new Board
        {
            Grid = request.Grid!,
            Generation = 0
        };
        _repo.Insert(board);
        return Ok(board.Id);
    }

    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BoardStateResponse> UpdateGrid(Guid id, [FromBody] UploadBoardRequest request)
    {
        if (request is null) return BadRequest(new { message = "Request body must not be null." });
        var (ok, error) = GridUtils.ValidateRectangular(request.Grid);
        if (!ok) return BadRequest(new { message = error });
        var board = _repo.Get(id);
        if (board is null) return NotFound();
        board.Grid = request.Grid!;
        // não mexe em Generation aqui
        _repo.Update(board);
        return Ok(ToResponse(board, board.Grid));
    }

    [HttpGet("{id}")]
    public ActionResult<BoardStateResponse> GetCurrent(Guid id)
    {
        var board = _repo.Get(id);
        if (board is null) return NotFound();
        return Ok(ToResponse(board, board.Grid));
    }

    [HttpGet("{id}/next")]
    public ActionResult<BoardStateResponse> GetNext(Guid id)
    {
        var board = _repo.Get(id);
        if (board is null) return NotFound();
        var next = _life.Next(board.Grid);
        return Ok(ToResponse(board, next));
    }

    [HttpGet("{id}/steps/{n:int}")]
    public ActionResult<BoardStateResponse> GetSteps(Guid id, int n)
    {
        if (n < 0) return BadRequest(new { message = "n must be >= 0" });
        var board = _repo.Get(id);
        if (board is null) return NotFound();
        var nth = _life.Step(board.Grid, n);
        return Ok(ToResponse(board, nth, board.Generation + n));
    }

    [HttpPost("{id}/advance")]
    public ActionResult<BoardStateResponse> Advance(Guid id, [FromQuery] int steps = 1)
    {
        if (steps < 0) return BadRequest(new { message = "steps must be >= 0" });
        var board = _repo.Get(id);
        if (board is null) return NotFound();
        var next = _life.Step(board.Grid, steps);
        board.Grid = next;
        board.Generation += steps;
        _repo.Update(board);
        return Ok(ToResponse(board, next));
    }

    [HttpGet("{id}/final")]
    public ActionResult<FinalStateResponse> GetFinal(Guid id, [FromQuery] int maxAttempts = 10000)
    {
        var board = _repo.Get(id);
        if (board is null) return NotFound();
        try
        {
            var result = _life.ComputeConclusion(id, board.Grid, maxAttempts);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(422, new { message = ex.Message });
        }
    }

    private static BoardStateResponse ToResponse(Board board, bool[][] grid, int? generationOverride = null)
    {
        var h = grid.Length;
        var w = grid[0].Length;
        return new BoardStateResponse
        {
            Id = board.Id,
            Generation = generationOverride ?? board.Generation,
            Width = w,
            Height = h,
            AliveCount = GridUtils.AliveCount(grid),
            Grid = grid
        };
    }
}
