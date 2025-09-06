using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GameOfLifeApi.DTOs;
using LiteDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GameOfLifeApi.Tests;

[Trait("Category", "Integration")]
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ILiteDatabase));
                if (dbDescriptor != null)
                {
                    services.Remove(dbDescriptor);
                }
                services.AddSingleton<ILiteDatabase>(_ => new LiteDatabase(new MemoryStream()));
            });
        });
    }

    [Fact]
    public async Task Upload_And_GetCurrent_Should_Work()
    {
        var client = CreateHttpsClient();
        var grid = new bool[][] { new[] { true, false }, new[] { false, true } };
        var id = await UploadAsync(client, grid);

        var current = await client.GetFromJsonAsync<BoardStateResponse>($"/api/boards/{id}");
        current.Should().NotBeNull();
        current!.Id.Should().Be(id);
        current.Width.Should().Be(2);
        current.Height.Should().Be(2);
        current.AliveCount.Should().Be(2);
    }

    [Fact]
    public async Task GetNext_Should_Return_Next_State()
    {
        var client = CreateHttpsClient();
        var grid = new bool[][]
        {
            new []{ false, true,  false },
            new []{ false, true,  false },
            new []{ false, true,  false },
        };
        var id = await UploadAsync(client, grid);
        var next = await client.GetFromJsonAsync<BoardStateResponse>($"/api/boards/{id}/next");
        next!.Grid.Should().BeEquivalentTo(new bool[][]
        {
            new []{ false, false, false },
            new []{ true,  true,  true  },
            new []{ false, false, false },
        });
    }

    [Fact]
    public async Task Steps_And_Advance_Should_Work_And_Persist()
    {
        var client = CreateHttpsClient();
        var grid = new bool[][]
        {
            new []{ false, true,  false },
            new []{ false, true,  false },
            new []{ false, true,  false },
        };
        var id = await UploadAsync(client, grid);

        var steps = await client.GetFromJsonAsync<BoardStateResponse>($"/api/boards/{id}/steps/2");
        steps!.Generation.Should().Be(2);

        var resp = await client.PostAsync($"/api/boards/{id}/advance?steps=2", content: null);
        resp.EnsureSuccessStatusCode();
        var advanced = await resp.Content.ReadFromJsonAsync<BoardStateResponse>();
        advanced!.Generation.Should().Be(2);

        var current = await client.GetFromJsonAsync<BoardStateResponse>($"/api/boards/{id}");
        current!.Generation.Should().Be(2);
        current.Grid.Should().BeEquivalentTo(advanced.Grid);
    }

    [Fact]
    public async Task Final_Should_Return_Loop_Info()
    {
        var client = CreateHttpsClient();
        var grid = new bool[][]
        {
            new []{ true, true },
            new []{ true, true },
        };
        var id = await UploadAsync(client, grid);
        var final = await client.GetFromJsonAsync<FinalStateResponse>($"/api/boards/{id}/final?maxAttempts=10");
        final!.IsLoop.Should().BeTrue();
        final.Period.Should().Be(1);
    }

    [Fact]
    public async Task Upload_Invalid_Grid_Should_Return_BadRequest()
    {
        var client = CreateHttpsClient();
        var req = new UploadBoardRequest { Grid = new bool[][] { new bool[0], new bool[1] } };
        var resp = await client.PostAsJsonAsync("/api/boards", req);
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        // null grid
        req = new UploadBoardRequest { Grid = null };
        resp = await client.PostAsJsonAsync("/api/boards", req);
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task NotFound_Cases_Should_Return_404()
    {
        var client = CreateHttpsClient();
        var id = Guid.NewGuid();
        (await client.GetAsync($"/api/boards/{id}")).StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        (await client.GetAsync($"/api/boards/{id}/next")).StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        (await client.GetAsync($"/api/boards/{id}/steps/1")).StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        (await client.PostAsync($"/api/boards/{id}/advance?steps=1", null)).StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        (await client.GetAsync($"/api/boards/{id}/final")).StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Steps_And_Advance_Invalid_Params_Should_Return_400()
    {
        var client = CreateHttpsClient();
        var grid = new bool[][] { new[] { true, false } };
        var id = await UploadAsync(client, grid);
        (await client.GetAsync($"/api/boards/{id}/steps/-1")).StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        (await client.PostAsync($"/api/boards/{id}/advance?steps=-2", null)).StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Final_Unreached_Should_Return_422()
    {
        var client = CreateHttpsClient();
        var grid = new bool[][]
        {
            new []{ true, false, true },
            new []{ false, true, false },
        };
        var id = await UploadAsync(client, grid);
        var resp = await client.GetAsync($"/api/boards/{id}/final?maxAttempts=1");
        resp.StatusCode.Should().Be((System.Net.HttpStatusCode)422);
    }

    private HttpClient CreateHttpsClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static async Task<Guid> UploadAsync(HttpClient client, bool[][] grid)
    {
        var req = new UploadBoardRequest { Grid = grid };
        var response = await client.PostAsJsonAsync("/api/boards", req);
        response.EnsureSuccessStatusCode();
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return id;
    }
}