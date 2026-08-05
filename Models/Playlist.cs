using System.ComponentModel.DataAnnotations;

namespace LI_Music.Models;

public class Playlist
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descricao { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public List<PlaylistMusica> PlaylistMusicas { get; set; } = [];
}
