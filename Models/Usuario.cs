using System.ComponentModel.DataAnnotations;

namespace LI_Music.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Login { get; set; } = string.Empty;

    [Required, StringLength(64)]
    public string SenhaHash { get; set; } = string.Empty;

    [StringLength(300)]
    public string? FotoUrl { get; set; }

    public List<Musica> Musicas { get; set; } = [];

    public List<Playlist> Playlists { get; set; } = [];
    public ICollection<HistoricoReproducao>
    HistoricoReproducoes
    { get; set; }
    = new List<HistoricoReproducao>();
}
