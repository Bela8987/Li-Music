using LI_Music.Data;
using LI_Music.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LI_Music.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistasController : ControllerBase
{
    private readonly LiMusicContext _context;

    public ArtistasController(LiMusicContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ArtistaResponseDto>>> GetTodos(
        [FromQuery] int? usuarioId,
        [FromQuery] string? nome)
    {
        var consulta = _context.Artistas.AsNoTracking().AsQueryable();

        if (usuarioId.HasValue)
            consulta = consulta.Where(artista => artista.Musicas.Any(musica => musica.UsuarioId == usuarioId));

        if (!string.IsNullOrWhiteSpace(nome))
            consulta = consulta.Where(artista => artista.Nome.Contains(nome));

        var artistas = await consulta
            .OrderBy(artista => artista.Nome)
            .Select(artista => new ArtistaResponseDto(
                artista.Id,
                artista.Nome,
                artista.FotoUrl,
                artista.Musicas.Count(musica => !usuarioId.HasValue || musica.UsuarioId == usuarioId)))
            .ToListAsync();

        return Ok(artistas);
    }

    [HttpGet("{id:int}/musicas")]
    public async Task<ActionResult<List<MusicaResponseDto>>> GetMusicas(
        int id,
        [FromQuery] int? usuarioId)
    {
        var artistaExiste = await _context.Artistas.AnyAsync(artista => artista.Id == id);
        if (!artistaExiste)
            return NotFound(new { mensagem = "Artista não encontrado." });

        var consulta = _context.Musicas
            .AsNoTracking()
            .Where(musica => musica.ArtistaId == id);

        if (usuarioId.HasValue)
            consulta = consulta.Where(musica => musica.UsuarioId == usuarioId);

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

    [HttpPut("{id:int}/foto")]
    public async Task<IActionResult> AtualizarFoto(
        int id,
        AtualizarFotoArtistaDto dto)
    {
        var artista = await _context.Artistas.FindAsync(id);
        if (artista is null)
            return NotFound(new { mensagem = "Artista não encontrado." });

        artista.FotoUrl = string.IsNullOrWhiteSpace(dto.FotoUrl)
            ? "/artistas/sem-foto.svg"
            : dto.FotoUrl.Trim();

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
