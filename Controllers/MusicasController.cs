using LI_Music.Data;
using LI_Music.DTOs;
using LI_Music.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LI_Music.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicasController : ControllerBase
{
    private readonly LiMusicContext _context;
    private readonly IWebHostEnvironment _environment;

    public MusicasController(
        LiMusicContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<List<MusicaResponseDto>>> GetTodos(
        [FromQuery] int? usuarioId,
        [FromQuery] string? nome,
        [FromQuery] string? artista)
    {
        var consulta = _context.Musicas.AsNoTracking().AsQueryable();

        if (usuarioId.HasValue)
            consulta = consulta.Where(musica => musica.UsuarioId == usuarioId);

        if (!string.IsNullOrWhiteSpace(nome))
            consulta = consulta.Where(musica => musica.Titulo.Contains(nome));

        if (!string.IsNullOrWhiteSpace(artista))
            consulta = consulta.Where(musica => musica.Artista.Nome.Contains(artista));

        var musicas = await consulta
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
            .ToListAsync();

        return Ok(musicas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MusicaResponseDto>> GetPorId(int id)
    {
        var musica = await _context.Musicas
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new MusicaResponseDto(
                item.Id,
                item.Titulo,
                item.DuracaoSegundos,
                item.CaminhoArquivo,
                item.CapaUrl,
                item.Genero,
                item.ArtistaId,
                item.Artista.Nome,
                item.UsuarioId,
                item.Usuario.Nome))
            .FirstOrDefaultAsync();

        if (musica is null)
            return NotFound(new { mensagem = "Música não encontrada." });

        return Ok(musica);
    }

    [HttpPost("sincronizar")]
    public async Task<IActionResult> SincronizarArquivos()
    {
        var adicionadas = await DbInitializer.SincronizarBibliotecasAsync(_context, _environment);
        return Ok(new
        {
            mensagem = "Bibliotecas sincronizadas com as pastas de áudio e capas.",
            musicasAdicionadas = adicionadas
        });
    }

    [HttpPost]
    public async Task<ActionResult<MusicaResponseDto>> Criar(MusicaRequestDto dto)
    {
        if (!await _context.Artistas.AnyAsync(item => item.Id == dto.ArtistaId))
            return BadRequest(new { mensagem = "O artista informado não existe." });

        if (!await _context.Usuarios.AnyAsync(item => item.Id == dto.UsuarioId))
            return BadRequest(new { mensagem = "O usuário informado não existe." });

        if (await _context.Musicas.AnyAsync(item => item.CaminhoArquivo == dto.AudioUrl))
            return Conflict(new { mensagem = "Já existe uma música com esse arquivo de áudio." });

        var musica = new Musica
        {
            Titulo = dto.Titulo.Trim(),
            DuracaoSegundos = dto.DuracaoSegundos,
            CaminhoArquivo = dto.AudioUrl.Trim(),
            CapaUrl = dto.CapaUrl?.Trim(),
            Genero = dto.Genero?.Trim(),
            ArtistaId = dto.ArtistaId,
            UsuarioId = dto.UsuarioId
        };

        _context.Musicas.Add(musica);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = musica.Id }, await BuscarRespostaAsync(musica.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, MusicaRequestDto dto)
    {
        var musica = await _context.Musicas.FindAsync(id);
        if (musica is null)
            return NotFound(new { mensagem = "Música não encontrada." });

        if (!await _context.Artistas.AnyAsync(item => item.Id == dto.ArtistaId))
            return BadRequest(new { mensagem = "O artista informado não existe." });

        if (!await _context.Usuarios.AnyAsync(item => item.Id == dto.UsuarioId))
            return BadRequest(new { mensagem = "O usuário informado não existe." });

        musica.Titulo = dto.Titulo.Trim();
        musica.DuracaoSegundos = dto.DuracaoSegundos;
        musica.CaminhoArquivo = dto.AudioUrl.Trim();
        musica.CapaUrl = dto.CapaUrl?.Trim();
        musica.Genero = dto.Genero?.Trim();
        musica.ArtistaId = dto.ArtistaId;
        musica.UsuarioId = dto.UsuarioId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var musica = await _context.Musicas.FindAsync(id);
        if (musica is null)
            return NotFound(new { mensagem = "Música não encontrada." });

        _context.Musicas.Remove(musica);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Task<MusicaResponseDto> BuscarRespostaAsync(int id)
        => _context.Musicas
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new MusicaResponseDto(
                item.Id,
                item.Titulo,
                item.DuracaoSegundos,
                item.CaminhoArquivo,
                item.CapaUrl,
                item.Genero,
                item.ArtistaId,
                item.Artista.Nome,
                item.UsuarioId,
                item.Usuario.Nome))
            .FirstAsync();
}
