using LI_Music.Models;
using Microsoft.EntityFrameworkCore;

namespace LI_Music.Data;

public class LiMusicContext : DbContext
{
    public LiMusicContext(DbContextOptions<LiMusicContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Artista> Artistas => Set<Artista>();
    public DbSet<Musica> Musicas => Set<Musica>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistMusica> PlaylistMusicas => Set<PlaylistMusica>();
    

    public object HistoricoReproducoes { get; internal set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(usuario => usuario.Login).IsUnique();
            entity.Property(usuario => usuario.Nome).HasMaxLength(80);
            entity.Property(usuario => usuario.Login).HasMaxLength(40);
            entity.Property(usuario => usuario.SenhaHash).HasMaxLength(64);
            entity.Property(usuario => usuario.FotoUrl).HasMaxLength(300);
        });

        modelBuilder.Entity<Artista>(entity =>
        {
            entity.Property(artista => artista.Nome).HasMaxLength(120);
            entity.Property(artista => artista.FotoUrl).HasMaxLength(300);
        });

        modelBuilder.Entity<Musica>(entity =>
        {
            entity.HasIndex(musica => musica.CaminhoArquivo).IsUnique();
            entity.Property(musica => musica.Titulo).HasMaxLength(150);
            entity.Property(musica => musica.CaminhoArquivo).HasMaxLength(500);
            entity.Property(musica => musica.CapaUrl).HasMaxLength(500);
            entity.Property(musica => musica.Genero).HasMaxLength(60);

            entity.HasOne(musica => musica.Artista)
                .WithMany(artista => artista.Musicas)
                .HasForeignKey(musica => musica.ArtistaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(musica => musica.Usuario)
                .WithMany(usuario => usuario.Musicas)
                .HasForeignKey(musica => musica.UsuarioId)
                // Evita múltiplos caminhos de exclusão em cascata no SQL Server:
                // Usuario -> Musicas -> PlaylistMusicas e
                // Usuario -> Playlists -> PlaylistMusicas.
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.Property(playlist => playlist.Nome).HasMaxLength(100);
            entity.Property(playlist => playlist.Descricao).HasMaxLength(300);

            entity.HasOne(playlist => playlist.Usuario)
                .WithMany(usuario => usuario.Playlists)
                .HasForeignKey(playlist => playlist.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaylistMusica>(entity =>
        {
            entity.HasKey(item => new { item.PlaylistId, item.MusicaId });

            entity.HasOne(item => item.Playlist)
                .WithMany(playlist => playlist.PlaylistMusicas)
                .HasForeignKey(item => item.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Musica)
                .WithMany(musica => musica.PlaylistMusicas)
                .HasForeignKey(item => item.MusicaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}