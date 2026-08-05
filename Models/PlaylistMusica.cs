namespace LI_Music.Models;

public class PlaylistMusica
{
    public int PlaylistId { get; set; }

    public Playlist Playlist { get; set; } = null!;

    public int MusicaId { get; set; }

    public Musica Musica { get; set; } = null!;
}
