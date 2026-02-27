namespace Application.Contracts;
public interface ICurrentUserService
{
    int UserId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
