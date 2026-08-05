using LI_Music.Data;
using LI_Music.DTOs;
using LI_Music.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LI_Music.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly LiMusicContext _context;

    public UsuariosController(LiMusicContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponseDto>>> GetTodos()
    {
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Id)
            .Select(usuario => new UsuarioResponseDto(
                usuario.Id,
                usuario.Nome,
                usuario.Login,
                usuario.FotoUrl))
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UsuarioResponseDto>> Login(LoginRequestDto dto)
    {
        var login = dto.Login.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Login == login);

        if (usuario is null || !SenhaService.Conferir(dto.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new
            {
                mensagem = "Usuário ou senha incorretos."
            });
        }

        return Ok(new UsuarioResponseDto(
            usuario.Id,
            usuario.Nome,
            usuario.Login,
            usuario.FotoUrl));
    }

    [HttpGet("{id:int}/perfil")]
    public async Task<ActionResult<UsuarioPerfilDto>> GetPerfil(int id)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new UsuarioPerfilDto(
                item.Id,
                item.Nome,
                item.Login,
                item.FotoUrl,
                item.Musicas
                    .OrderBy(musica => musica.Titulo)
                    .Select(musica => new MusicaResponseDto(
                        musica.Id,
                        musica.Titulo,
                        musica.DuracaoSegundos,
                        musica.CaminhoArquivo,
                        musica.CapaUrl,
                        musica.Genero,
                        musica.ArtistaId,
                        musica.Artista.Nome,
                        musica.UsuarioId,
                        musica.Usuario.Nome))
                    .ToList(),
                item.Playlists
                    .OrderBy(playlist => playlist.Nome)
                    .Select(playlist => new PlaylistResponseDto(
                        playlist.Id,
                        playlist.Nome,
                        playlist.Descricao,
                        playlist.UsuarioId,
                        playlist.Usuario.Nome,
                        playlist.PlaylistMusicas
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
                    .ToList()))
            .FirstOrDefaultAsync();

        if (usuario is null)
            return NotFound(new { mensagem = "Usuário não encontrado." });

        return Ok(usuario);
    }
}
