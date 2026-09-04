namespace RaceDay.Api;
public record RegisterRequest(string Username,string Email,string Password,string Role,string? OrganisationName,string? ContactName,string? Phone,string? FirstName,string? LastName,DateOnly? DateOfBirth,string? Gender,string? EmergencyContact);
public record LoginRequest(string UsernameOrEmail,string Password);
public record EventRequest(string EventName,string Description,DateOnly EventDate,string Location,decimal Distance,string EventType,string? BannerImageUrl,string? Status);
public record CategoryRequest(string CategoryName,decimal Distance,int MinAge,int MaxAge,decimal EntryFee);
public record EnrollmentRequest(int CategoryId); public record ResultRequest(int EnrollmentId,TimeOnly FinishTime,int Position,string? Remarks); public record UpdateResultRequest(TimeOnly FinishTime,int Position,string? Remarks);
public record ProfileRequest(string? OrganisationName,string? ContactName,string? Phone,string? FirstName,string? LastName,DateOnly? DateOfBirth,string? Gender,string? EmergencyContact,string? ProfilePictureUrl);
