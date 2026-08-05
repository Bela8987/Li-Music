using LI_Music.Data;
using LI_Music.DTOs;
using LI_Music.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LI_Music.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly LiMusicContext _context;

    public PlaylistsController(LiMusicContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlaylistResponseDto>>> GetTodos(
        [FromQuery] int? usuarioId)
    {
        var consulta = _context.Playlists.AsNoTracking().AsQueryable();

        if (usuarioId.HasValue)
            consulta = consulta.Where(playlist => playlist.UsuarioId == usuarioId);

        var playlists = await consulta
            .OrderBy(playlist => playlist.Nome)
            .Select(playlist => new PlaylistResponseDto(
                playlist.Id,
                playlist.Nome,
                playlist.Descricao,
                playlist.UsuarioId,
                playlist.Usuario.Nome,
                playlist.PlaylistMusicas
                    .OrderBy(item => item.Musica.Titulo)
                    .Select(item => new MusicaResponseDto(
                        item.Musica.Id,
                        item.Musica.Titulo,
                        item.Musica.DuracaoSegundos,
                        item.Musica.CaminhoArquivo,
                        item.Musica.CapaUrl,
                        item.Musica.Genero,
                        item.Musica.ArtistaId,
                        item.Musica.Artista.Nome,
                        item.Musica.UsuarioId,
                        item.Musica.Usuario.Nome))
                    .ToList()))
            .ToListAsync();

        return Ok(playlists);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlaylistResponseDto>> GetPorId(int id)
    {
        var playlist = await _context.Playlists
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new PlaylistResponseDto(
                item.Id,
                item.Nome,
                item.Descricao,
                item.UsuarioId,
                item.Usuario.Nome,
                item.PlaylistMusicas
                    .OrderBy(relacao => relacao.Musica.Titulo)
                    .Select(relacao => new MusicaResponseDto(
                        relacao.Musica.Id,
                        relacao.Musica.Titulo,
                        relacao.Musica.DuracaoSegundos,
                        relacao.Musica.CaminhoArquivo,
                        relacao.Musica.CapaUrl,
                        relacao.Musica.Genero,
                        relacao.Musica.ArtistaId,
                        relacao.Musica.Artista.Nome,
                        relacao.Musica.UsuarioId,
                        relacao.Musica.Usuario.Nome))
                    .ToList()))
            .FirstOrDefaultAsync();

        if (playlist is null)
            return NotFound(new { mensagem = "Playlist não encontrada." });

        return Ok(playlist);
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistResponseDto>> Criar(PlaylistRequestDto dto)
    {
        if (!await _context.Usuarios.AnyAsync(usuario => usuario.Id == dto.UsuarioId))
            return BadRequest(new { mensagem = "O usuário informado não existe." });

        var playlist = new Playlist
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            UsuarioId = dto.UsuarioId
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPorId),
            new { id = playlist.Id },
            await BuscarRespostaAsync(playlist.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, PlaylistRequestDto dto)
    {
        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist is null)
            return NotFound(new { mensagem = "Playlist não encontrada." });

        if (playlist.UsuarioId != dto.UsuarioId)
            return BadRequest(new { mensagem = "Não é possível transferir a playlist para outro usuário." });

        playlist.Nome = dto.Nome.Trim();
        playlist.Descricao = dto.Descricao?.Trim();
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{playlistId:int}/musicas/{musicaId:int}")]
    public async Task<IActionResult> AdicionarMusica(int playlistId, int musicaId)
    {
        var playlist = await _context.Playlists.FindAsync(playlistId);
        var musica = await _context.Musicas.FindAsync(musicaId);

        if (playlist is null)
            return NotFound(new { mensagem = "Playlist não encontrada." });

        if (musica is null)
            return NotFound(new { mensagem = "Música não encontrada." });

        if (playlist.UsuarioId != musica.UsuarioId)
            return BadRequest(new { mensagem = "A música pertence a outro perfil." });

        var existe = await _context.PlaylistMusicas
            .AnyAsync(item => item.PlaylistId == playlistId && item.MusicaId == musicaId);

        if (existe)
            return Conflict(new { mensagem = "A música já está nesta playlist." });

        _context.PlaylistMusicas.Add(new PlaylistMusica
        {
            PlaylistId = playlistId,
            MusicaId = musicaId
        });

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{playlistId:int}/musicas/{musicaId:int}")]
    public async Task<IActionResult> RemoverMusica(int playlistId, int musicaId)
    {
        var relacao = await _context.PlaylistMusicas
            .FindAsync(playlistId, musicaId);

        if (relacao is null)
            return NotFound(new { mensagem = "A música não está nesta playlist." });

        _context.PlaylistMusicas.Remove(relacao);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist is null)
            return NotFound(new { mensagem = "Playlist não encontrada." });

        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Task<PlaylistResponseDto> BuscarRespostaAsync(int id)
        => _context.Playlists
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new PlaylistResponseDto(
                item.Id,
                item.Nome,
                item.Descricao,
                item.UsuarioId,
                item.Usuario.Nome,
                item.PlaylistMusicas
                    .OrderBy(relacao => relacao.Musica.Titulo)
                    .Select(relacao => new MusicaResponseDto(
                        relacao.Musica.Id,
                        relacao.Musica.Titulo,
                        relacao.Musica.DuracaoSegundos,
                        relacao.Musica.CaminhoArquivo,
                        relacao.Musica.CapaUrl,
                        relacao.Musica.Genero,
                        relacao.Musica.ArtistaId,
                        relacao.Musica.Artista.Nome,
                        relacao.Musica.UsuarioId,
                        relacao.Musica.Usuario.Nome))
                    .ToList()))
            .FirstAsync();
}
