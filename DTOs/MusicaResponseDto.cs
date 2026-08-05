namespace LI_Music.DTOs;

public record MusicaResponseDto(
    int Id,
    string Titulo,
    int DuracaoSegundos,
    string AudioUrl,
    string? CapaUrl,
    string? Genero,
    int ArtistaId,
    string Artista,
    int UsuarioId,
    string Usuario
);
