using Core.ValueObjects;

namespace Core.Entities;
public class User
{
    public int Id { get; set; }
    public Email Email { get; set; }
    public Password Password { get; set; }
    public DateTime CreatedAt { get; set; }
}
