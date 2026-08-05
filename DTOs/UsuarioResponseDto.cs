namespace LI_Music.DTOs;

public record UsuarioResponseDto(
    int Id,
    string Nome,
    string Login,
    string? FotoUrl
);
