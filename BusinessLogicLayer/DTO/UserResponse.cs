namespace BusinessLogicLayer.DTO;

public record UserResponse(
    Guid UserID,
    string? Email,
    string? PersonName,
    string? Gender
)
{
    public UserResponse() : this(default, default, default, default)
    {
        
    }
}

