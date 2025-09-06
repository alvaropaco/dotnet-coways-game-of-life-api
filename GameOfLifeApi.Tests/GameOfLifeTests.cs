using System;
using System.Diagnostics;
using FluentAssertions;
using GameOfLifeApi.DTOs;
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

    [Fact]
    public void Step_With_Zero_Should_Return_Same_Grid()
    {
        bool[][] grid = new bool[][]
        {
            new []{ true, false },
            new []{ false, true },
        };
        var svc = new GameOfLifeService();
        var result = svc.Step(grid, 0);
        result.Should().BeEquivalentTo(grid);
    }

    [Fact]
    public void Step_With_Negative_Should_Throw()
    {
        bool[][] grid = new bool[][]
        {
            new []{ true, false }
        };
        var svc = new GameOfLifeService();
        Action act = () => svc.Step(grid, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ComputeConclusion_Should_Throw_When_MaxAttempts_Too_Low()
    {
        bool[][] grid = new bool[][]
        {
            new []{ true, false, true },
            new []{ false, true, false },
        };
        var svc = new GameOfLifeService();
        Action act = () => svc.ComputeConclusion(Guid.NewGuid(), grid, 1);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Next_On_1x1_Should_Become_Dead()
    {
        bool[][] grid = new bool[][] { new []{ true } };
        var svc = new GameOfLifeService();
        var next = svc.Next(grid);
        next[0][0].Should().BeFalse();
    }

    [Fact]
    public void GridUtils_ValidateRectangular_Should_Flag_Errors()
    {
        GridUtils.ValidateRectangular(null).ok.Should().BeFalse();
        GridUtils.ValidateRectangular(Array.Empty<bool[]>() ).ok.Should().BeFalse();
        GridUtils.ValidateRectangular(new bool[][] { null! }).ok.Should().BeFalse();
        GridUtils.ValidateRectangular(new bool[][] { Array.Empty<bool>(), new []{ true } }).ok.Should().BeFalse();
        GridUtils.ValidateRectangular(new bool[][] { Array.Empty<bool>() }).ok.Should().BeFalse();
    }

    [Fact]
    public void Performance_Next_On_Medium_Grid_Should_Be_Fast()
    {
        var rnd = new Random(42);
        int h = 100, w = 100;
        var grid = new bool[h][];
        for (int r = 0; r < h; r++)
        {
            grid[r] = new bool[w];
            for (int c = 0; c < w; c++)
                grid[r][c] = rnd.NextDouble() < 0.3;
        }
        var svc = new GameOfLifeService();
        var sw = Stopwatch.StartNew();
        var next = svc.Next(grid);
        sw.Stop();
        next.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(200); // generous threshold
    }
}
