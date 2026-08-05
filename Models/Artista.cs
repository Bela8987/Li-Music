using System.ComponentModel.DataAnnotations;

namespace LI_Music.Models;

public class Artista
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300)]
    public string? FotoUrl { get; set; }

    public List<Musica> Musicas { get; set; } = [];
}
