using System.ComponentModel.DataAnnotations;

namespace LI_Music.DTOs;

public class LoginRequestDto
{
    [Required, StringLength(40)]
    public string Login { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Senha { get; set; } = string.Empty;
}
