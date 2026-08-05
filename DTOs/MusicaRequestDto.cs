using System.ComponentModel.DataAnnotations;

namespace LI_Music.DTOs;

public class MusicaRequestDto
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(150, MinimumLength = 2)]
    public string Titulo { get; set; } = string.Empty;

    [Range(1, 7200, ErrorMessage = "A duração deve estar entre 1 e 7200 segundos.")]
    public int DuracaoSegundos { get; set; }

    [Required(ErrorMessage = "O caminho do arquivo é obrigatório.")]
    [StringLength(500)]
    public string AudioUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string? CapaUrl { get; set; }

    [StringLength(60)]
    public string? Genero { get; set; }

    [Range(1, int.MaxValue)]
    public int ArtistaId { get; set; }

    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }
}
