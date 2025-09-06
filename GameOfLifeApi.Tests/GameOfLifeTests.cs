using System;
using FluentAssertions;
using GameOfLifeApi.Services;
using GameOfLifeApi.Utils;
using Xunit;

namespace GameOfLifeApi.Tests;

public class GameOfLifeTests
{
    [Fact]
    public void Blinker_Should_Oscillate_Period_2()
    {
        bool[][] grid = new bool[][]
        {
            new []{ false, true,  false },
            new []{ false, true,  false },
            new []{ false, true,  false },
        };
        var svc = new GameOfLifeService();
        var next = svc.Next(grid);
        var next2 = svc.Next(next);
        next.Should().BeEquivalentTo(new bool[][]
        {
            new []{ false, false, false },
            new []{ true,  true,  true  },
            new []{ false, false, false },
        });
        next2.Should().BeEquivalentTo(grid);
    }

    [Fact]
    public void Block_Should_Be_Stable()
    {
        bool[][] grid = new bool[][]
        {
            new []{ true, true },
            new []{ true, true },
        };
        var svc = new GameOfLifeService();
        var next = svc.Next(grid);
        next.Should().BeEquivalentTo(grid);
        var final = svc.ComputeConclusion(System.Guid.Empty, grid, 10);
        final.IsLoop.Should().BeTrue();
        final.Period.Should().Be(1);
        final.Conclusion.Should().Contain("Stable");
    }

    [Fact]
    public void Serialize_Should_Be_Deterministic()
    {
        bool[][] grid = new bool[][]
        {
            new []{ true, false, true },
            new []{ false, true, false },
        };
        var s1 = GridUtils.Serialize(grid);
        var s2 = GridUtils.Serialize(GridUtils.Clone(grid));
        s1.Should().Be(s2);
    }
}
