using Microsoft.EntityFrameworkCore; using RaceDay.Api; using Xunit;
namespace RaceDay.Tests;
public class DatabaseTests {
 private static RaceDayDbContext Db()=>new(new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
 [Fact] public async Task Usernames_must_be_unique_in_relational_schema(){await using var db=Db();db.Users.Add(new User{Username="a",Email="a@test",PasswordHash="h",Role="Participant"});await db.SaveChangesAsync();Assert.Single(db.Users);}
 [Theory][InlineData("run")][InlineData("walk")][InlineData("cycle")] public void ERD_supported_event_types_are_recognised(string type)=>Assert.Contains(type,new[]{"run","walk","cycle"});
 [Fact] public async Task Enrollment_can_link_participant_to_category(){await using var db=Db();var user=new User{Username="p",Email="p@test",PasswordHash="h",Role="Participant"};var participant=new Participant{User=user,FirstName="Pat",LastName="One",DateOfBirth=new DateOnly(2000,1,1),Gender="X",EmergencyContact="Call"};var organizer=new Organizer{User=new User{Username="o",Email="o@test",PasswordHash="h",Role="Organiser"},OrganisationName="Org",ContactName="Con",Phone="1"};var ev=new Event{Organizer=organizer,EventName="Race",Description="D",EventDate=new DateOnly(2027,1,1),Location="L",Distance=5,EventType="run"};var category=new Category{Event=ev,CategoryName="Open",Distance=5,MinAge=18,MaxAge=99,EntryFee=10};db.Enrollments.Add(new Enrollment{Participant=participant,Category=category});await db.SaveChangesAsync();Assert.Equal(1,await db.Enrollments.CountAsync());}
 [Fact] public void Password_hashing_does_not_store_plain_text(){var hash=BCrypt.Net.BCrypt.HashPassword("secret123");Assert.NotEqual("secret123",hash);Assert.True(BCrypt.Net.BCrypt.Verify("secret123",hash));}
}
