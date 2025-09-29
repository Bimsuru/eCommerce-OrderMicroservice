namespace BusinessLogicLayer.DTO;

public record UserResponse(
    Guid UserID,
    string? Email,
    string? PersonName,
    string? Gender
);

