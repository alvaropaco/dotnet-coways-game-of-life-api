using System.IO;
using FluentAssertions;
using GameOfLifeApi.Models;
using GameOfLifeApi.Repositories;
using LiteDB;
using Xunit;

namespace GameOfLifeApi.Tests;

public class LiteDbRepositoryTests
{
    [Fact]
    public void Insert_And_Get_Should_Work()
    {
        using var memStream = new MemoryStream();
        using var db = new LiteDatabase(memStream);
        var repo = new LiteDbBoardRepository(db);
        var board = new Board
        {
            Grid = new bool[][]
            {
                new []{ true, false },
                new []{ false, true }
            },
            Generation = 0
        };
        repo.Insert(board);
        var loaded = repo.Get(board.Id);
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(board.Id);
        loaded.Grid[0][0].Should().BeTrue();
        loaded.Grid[1][1].Should().BeTrue();
    }

    [Fact]
    public void Update_Should_Persist_Changes()
    {
        using var memStream = new MemoryStream();
        using var db = new LiteDatabase(memStream);
        var repo = new LiteDbBoardRepository(db);
        var board = new Board
        {
            Grid = new bool[][]
            {
                new []{ true, false },
                new []{ false, true }
            },
            Generation = 0
        };
        repo.Insert(board);
        board.Generation = 5;
        repo.Update(board);
        var loaded = repo.Get(board.Id);
        loaded!.Generation.Should().Be(5);
    }
}
