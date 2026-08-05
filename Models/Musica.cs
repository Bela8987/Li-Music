using System.ComponentModel.DataAnnotations;

namespace LI_Music.Models;

public class Musica
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Range(1, 7200)]
    public int DuracaoSegundos { get; set; }

    [Required, StringLength(500)]
    public string CaminhoArquivo { get; set; } = string.Empty;

    [StringLength(500)]
    public string? CapaUrl { get; set; }

    [StringLength(60)]
    public string? Genero { get; set; }

    public int ArtistaId { get; set; }

    public Artista Artista { get; set; } = null!;

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public List<PlaylistMusica> PlaylistMusicas { get; set; } = [];

    public ICollection<HistoricoReproducao>
    HistoricoReproducoes
    { get; set; }
    = new List<HistoricoReproducao>();
}
