using LI_Music.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace LI_Music.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QrCodesController : ControllerBase
{
    private readonly LiMusicContext _context;

    public QrCodesController(LiMusicContext context)
    {
        _context = context;
    }

    [HttpGet("perfil/{usuarioId:int}")]
    public async Task<IActionResult> GerarQrCode(
        int usuarioId,
        [FromQuery] string? baseUrl)
    {
        if (!await _context.Usuarios.AnyAsync(usuario => usuario.Id == usuarioId))
            return NotFound(new { mensagem = "Usuário não encontrado." });

        var enderecoBase = string.IsNullOrWhiteSpace(baseUrl)
            ? $"{Request.Scheme}://{Request.Host}"
            : baseUrl.TrimEnd('/');

        if (!Uri.TryCreate(enderecoBase, UriKind.Absolute, out _))
            return BadRequest(new { mensagem = "Informe uma URL válida." });

        var urlPerfil = $"{enderecoBase}/?perfil={usuarioId}";
        var imagem = PngByteQRCodeHelper.GetQRCode(
            urlPerfil,
            QRCodeGenerator.ECCLevel.Q,
            20);

        return File(imagem, "image/png");
    }
}
