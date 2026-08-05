using System.ComponentModel.DataAnnotations;

namespace LI_Music.DTOs;

public class PlaylistRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descricao { get; set; }

    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }
}

public record PlaylistResponseDto(
    int Id,
    string Nome,
    string? Descricao,
    int UsuarioId,
    string Usuario,
    List<MusicaResponseDto> Musicas
);
