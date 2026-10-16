using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Api;
using Xunit;

namespace RaceDay.Tests;

/// <summary>Jarred-focused unit tests for the RaceDay API's success and failure paths.</summary>
public class RaceDayApiTests
{
    private static RaceDayDbContext Database() => new(
        new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static TokenService Tokens() => new(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "JarredNeedsALongDevelopmentSigningKeyForTestsOnly2026",
            ["Jwt:Issuer"] = "RaceDay",
            ["Jwt:Audience"] = "RaceDayClients",
            ["Jwt:ExpiryMinutes"] = "60"
        }).Build());

    private static T As<T>(T controller, int userId, string role) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role)], "Test"))
            }
        };
        return controller;
    }

    [Fact]
    public void Jarreds_approved_endpoint_plan_has_all_twenty_actions()
    {
        var controllers = new[] { typeof(AuthController), typeof(UsersController), typeof(EventsController), typeof(CategoriesController), typeof(EnrollmentsController), typeof(EventEnrollmentController), typeof(ResultsController) };
        var actions = controllers.SelectMany(type => type.GetMethods())
            .SelectMany(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), true).Cast<HttpMethodAttribute>())
            .ToList();

        Assert.Equal(20, actions.Count);
        Assert.Contains(actions, action => action.HttpMethods.Contains("POST") && action.Template == "register");
        Assert.Contains(actions, action => action.HttpMethods.Contains("GET") && action.Template == "/api/events/{eventId:int}/results");
    }

    [Fact]
    public async Task Jarred_can_register_a_participant_account()
    {
        await using var db = Database();
        var controller = new AuthController(db, Tokens());

        var result = await controller.Register(new RegisterRequest(
            "jarred.runner", "jarred@example.test", "Jarred123!", "Participant",
            null, null, null, "Jarred", "Runner", new DateOnly(1998, 1, 12),
            "Male", "Sam Runner 0820000000"));

        Assert.IsType<CreatedResult>(result);
        var user = await db.Users.SingleAsync();
        Assert.Equal("jarred.runner", user.Username);
        Assert.NotEqual("Jarred123!", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Jarred123!", user.PasswordHash));
        Assert.Single(db.Participants);
    }

    [Fact]
    public async Task Jarred_cannot_register_with_an_existing_email()
    {
        await using var db = Database();
        db.Users.Add(new User { Username = "jarred", Email = "jarred@example.test", PasswordHash = "hash", Role = "Participant" });
        await db.SaveChangesAsync();
        var controller = new AuthController(db, Tokens());

        var result = await controller.Register(new RegisterRequest(
            "jarred.two", "jarred@example.test", "Jarred123!", "Participant",
            null, null, null, "Jarred", "Two", new DateOnly(1998, 1, 12), "Male", "Emergency"));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task Jarred_can_log_in_with_valid_credentials()
    {
        await using var db = Database();
        db.Users.Add(new User { Username = "jarred", Email = "jarred@example.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Jarred123!"), Role = "Participant" });
        await db.SaveChangesAsync();

        var result = await new AuthController(db, Tokens()).Login(new LoginRequest("jarred", "Jarred123!"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Jarred_is_rejected_when_login_password_is_wrong()
    {
        await using var db = Database();
        db.Users.Add(new User { Username = "jarred", Email = "jarred@example.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Jarred123!"), Role = "Participant" });
        await db.SaveChangesAsync();

        var result = await new AuthController(db, Tokens()).Login(new LoginRequest("jarred", "WrongPassword"));

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Jarred_cannot_create_an_event_with_an_invalid_type()
    {
        await using var db = Database();
        var controller = As(new EventsController(db), 1, "Organiser");

        var result = await controller.Create(new EventRequest("Jarred's Swim", "No swimming event type in plan", new DateOnly(2027, 1, 1), "Cape Town", 5, "swim", null, null));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(db.Events);
    }

    [Fact]
    public async Task Jarred_cannot_enrol_twice_in_the_same_category()
    {
        await using var db = Database();
        var participant = new Participant { User = new User { Username = "jarred", Email = "jarred@example.test", PasswordHash = "hash", Role = "Participant" }, FirstName = "Jarred", LastName = "Runner", DateOfBirth = new DateOnly(1998, 1, 12), Gender = "Male", EmergencyContact = "Emergency" };
        var organiser = new Organizer { User = new User { Username = "org", Email = "org@example.test", PasswordHash = "hash", Role = "Organiser" }, OrganisationName = "Jarred Events", ContactName = "Jarred", Phone = "0110000000" };
        var category = new Category { Event = new Event { Organizer = organiser, EventName = "Jarred Dash", Description = "Test", EventDate = new DateOnly(2027, 3, 1), Location = "Pretoria", Distance = 10, EventType = "run" }, CategoryName = "Open", Distance = 10, MinAge = 18, MaxAge = 99, EntryFee = 100 };
        db.Enrollments.Add(new Enrollment { Participant = participant, Category = category });
        await db.SaveChangesAsync();
        var controller = As(new EnrollmentsController(db), participant.UserId, "Participant");

        var result = await controller.Create(new EnrollmentRequest(category.CategoryId));

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(db.Enrollments);
    }

    [Fact]
    public async Task Jarred_can_read_the_public_event_list()
    {
        await using var db = Database();
        var organiser = new Organizer { User = new User { Username = "org", Email = "org@example.test", PasswordHash = "hash", Role = "Organiser" }, OrganisationName = "Jarred Events", ContactName = "Jarred", Phone = "0110000000" };
        db.Events.Add(new Event { Organizer = organiser, EventName = "Jarred Dash", Description = "Test", EventDate = new DateOnly(2027, 3, 1), Location = "Pretoria", Distance = 10, EventType = "run" });
        await db.SaveChangesAsync();

        var result = await new EventsController(db).List();

        Assert.IsType<OkObjectResult>(result);
    }
}
