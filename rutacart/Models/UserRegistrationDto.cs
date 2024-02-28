using System.ComponentModel.DataAnnotations;

namespace rutacart.Models;
public class UserRegistrationDto
{
    [Required]
    [MaxLength(255)]
    public string Nombre { get; set; }

    [Required]
    [MaxLength(255)]
    public string Apellido { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

public class UserLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
