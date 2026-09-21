using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrainingCatalog.Application;

namespace TrainingCatalog.Api.Tests;

public sealed class TrainingCreationTests
{
    [Fact]
    public async Task ReturnsCreatedWhenLessonsFitWithinTotalDuration()
    {
        using var factory = new TrainingCatalogApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateTrainingRequest(
            "Fundamentos de C#",
            "Introdução ao C#",
            "2026-09-15",
            8,
            2,
            4);

        var response = await client.PostAsJsonAsync("/api/trainings", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var training = await response.Content.ReadFromJsonAsync<Training>();
        Assert.NotNull(training);
        Assert.Equal(request.DurationHours, training.DurationHours);
        Assert.Equal(request.LessonCount, training.LessonCount);
        Assert.Equal(request.LessonDurationHours, training.LessonDurationHours);
    }

    [Fact]
    public async Task ReturnsBadRequestWhenLessonDurationExceedsFourHours()
    {
        using var factory = new TrainingCatalogApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateTrainingRequest(
            "Fundamentos de C#",
            "Introdução ao C#",
            "2026-09-15",
            8,
            1,
            5);

        var response = await client.PostAsJsonAsync("/api/trainings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(error.RootElement.GetProperty("errors").TryGetProperty("lessonDurationHours", out _));
    }

    [Fact]
    public async Task ReturnsBadRequestWhenLessonPlanExceedsTotalDuration()
    {
        using var factory = new TrainingCatalogApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateTrainingRequest(
            "Fundamentos de C#",
            "Introdução ao C#",
            "2026-09-15",
            8,
            3,
            4);

        var response = await client.PostAsJsonAsync("/api/trainings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "A soma da duração das aulas não pode exceder a carga horária total do treinamento.",
            error.RootElement.GetProperty("errors").GetProperty("lessonDurationHours")[0].GetString());
    }

    [Fact]
    public async Task ReturnsBadRequestWhenLessonCountOrDurationIsNotPositive()
    {
        using var factory = new TrainingCatalogApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateTrainingRequest(
            "Fundamentos de C#",
            "Introdução ao C#",
            "2026-09-15",
            8,
            0,
            0);

        var response = await client.PostAsJsonAsync("/api/trainings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(error.RootElement.GetProperty("errors").TryGetProperty("lessonCount", out _));
        Assert.True(error.RootElement.GetProperty("errors").TryGetProperty("lessonDurationHours", out _));
    }

    [Fact]
    public async Task ReturnsConflictWhenStartDateAlreadyExists()
    {
        using var factory = new TrainingCatalogApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateTrainingRequest(
            "Fundamentos de C#",
            "Introdução ao C#",
            "2026-09-15",
            8,
            2,
            4);

        var firstResponse = await client.PostAsJsonAsync("/api/trainings", request);
        var secondResponse = await client.PostAsJsonAsync(
            "/api/trainings",
            request with { Title = "C# Avançado" });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        using var error = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "Já existe um treinamento com esta data de início.",
            error.RootElement.GetProperty("errors").GetProperty("startDate")[0].GetString());
    }
}