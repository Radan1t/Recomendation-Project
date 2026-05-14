namespace Shared.DTO.Auth;
public class RegisterRequestDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime DateOfBirth { get; set; }
}
