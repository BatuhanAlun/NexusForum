namespace NexusForum.Api.Domain.Enums;

// Stored as string in DB so SQL queries are human-readable without enum lookups.
public enum UserRole
{
    Member = 0,
    Moderator = 1,
    Admin = 2
}
