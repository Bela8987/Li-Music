namespace LI_Music.DTOs;

public record UsuarioPerfilDto(
    int Id,
    string Nome,
    string Login,
    string? FotoUrl,
    List<MusicaResponseDto> Musicas,
    List<PlaylistResponseDto> Playlists
);
