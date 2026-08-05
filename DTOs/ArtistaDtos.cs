using System.ComponentModel.DataAnnotations;

namespace LI_Music.DTOs;

public record ArtistaResponseDto(
    int Id,
    string Nome,
    string? FotoUrl,
    int QuantidadeMusicas
);

public class AtualizarFotoArtistaDto
{
    [StringLength(300)]
    public string? FotoUrl { get; set; }
}
