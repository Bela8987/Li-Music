using System.Security;
using LI_Music.Models;
using LI_Music.Services;
using Microsoft.EntityFrameworkCore;

namespace LI_Music.Data;

public static class DbInitializer
{
    private static readonly string[] ExtensoesAudio = [".mp3", ".wav", ".ogg", ".m4a"];
    private static readonly string[] ExtensoesImagem = [".jpg", ".jpeg", ".png", ".webp", ".svg"];

    private static readonly Dictionary<string, string> NomesArtistas =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["FleetwoodMac"] = "Fleetwood Mac",
            ["OliviaRodrigo"] = "Olivia Rodrigo",
            ["TheCranberries"] = "The Cranberries",
            ["Bleachers"] = "Bleachers",
            ["PretaGil"] = "Preta Gil",
            ["TaylorSwift"] = "Taylor Swift",
            ["EmpireOfTheSun"] = "Empire of the Sun",
            ["GalCosta"] = "Gal Costa",
            ["Player"] = "Player",
            ["Journey"] = "Journey",
            ["DepecheMode"] = "Depeche Mode",
            ["TheGooGooDolls"] = "The Goo Goo Dolls"
        };

    private static readonly Dictionary<string, string> Titulos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Landslide"] = "Landslide",
            ["less"] = "less",
            ["Linger"] = "Linger",
            ["MerryChristmasPleaseDontCall"] = "Merry Christmas, Please Don't Call",
            ["SinaisDeFogo"] = "Sinais de Fogo",
            ["TheLastTime"] = "The Last Time",
            ["WeAreThePeople"] = "We Are the People",
            ["Azul"] = "Azul",
            ["BabyComeBack"] = "Baby Come Back",
            ["DontStopBelievin"] = "Don't Stop Believin'",
            ["EnjoyTheSilence"] = "Enjoy the Silence",
            ["FatherFigure"] = "Father Figure",
            ["GoldDustWoman"] = "Gold Dust Woman",
            ["Iris"] = "Iris"
        };

    private static readonly Dictionary<string, int> Duracoes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Landslide_FleetwoodMac"] = 197,
            ["less_OliviaRodrigo"] = 194,
            ["Linger_TheCranberries"] = 277,
            ["MerryChristmasPleaseDontCall_Bleachers"] = 200,
            ["SinaisDeFogo_PretaGil"] = 216,
            ["TheLastTime_TaylorSwift"] = 299,
            ["WeAreThePeople_EmpireOfTheSun"] = 267,
            ["Azul_GalCosta"] = 224,
            ["BabyComeBack_Player"] = 257,
            ["DontStopBelievin_Journey"] = 250,
            ["EnjoyTheSilence_DepecheMode"] = 254,
            ["FatherFigure_TaylorSwift"] = 216,
            ["GoldDustWoman_FleetwoodMac"] = 296,
            ["Iris_TheGooGooDolls"] = 290
        };

    private static readonly Dictionary<string, string> Generos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Fleetwood Mac"] = "Rock",
            ["Olivia Rodrigo"] = "Pop",
            ["The Cranberries"] = "Rock alternativo",
            ["Bleachers"] = "Pop alternativo",
            ["Preta Gil"] = "MPB",
            ["Taylor Swift"] = "Pop",
            ["Empire of the Sun"] = "Eletrônica",
            ["Gal Costa"] = "MPB",
            ["Player"] = "Soft rock",
            ["Journey"] = "Rock",
            ["Depeche Mode"] = "Synth-pop",
            ["The Goo Goo Dolls"] = "Rock alternativo"
        };

    public static async Task InicializarAsync(
        LiMusicContext context,
        IWebHostEnvironment environment)
    {
        await context.Database.MigrateAsync();
        await RemoverDadosDeExemploAsync(context);
        await SincronizarBibliotecasAsync(context, environment);
    }

    public static async Task<int> SincronizarBibliotecasAsync(
        LiMusicContext context,
        IWebHostEnvironment environment)
    {
        var isabela = await ObterOuCriarUsuarioAsync(
            context,
            nome: "Isabela",
            login: "isabela",
            senha: "isabela123");

        var lauany = await ObterOuCriarUsuarioAsync(
            context,
            nome: "Lauany",
            login: "lauany",
            senha: "lauany123");

        var total = 0;
        total += await ImportarPastaAsync(context, environment.WebRootPath, "isabela", isabela);
        total += await ImportarPastaAsync(context, environment.WebRootPath, "lauany", lauany);
        return total;
    }

    private static async Task RemoverDadosDeExemploAsync(LiMusicContext context)
    {
        var musicasExemplo = await context.Musicas
            .Where(musica => musica.CaminhoArquivo.StartsWith("/musicas/n"))
            .ToListAsync();

        if (musicasExemplo.Count > 0)
            context.Musicas.RemoveRange(musicasExemplo);

        var playlistsExemplo = await context.Playlists
            .Where(playlist =>
                playlist.Nome == "Músicas da Isabela" ||
                playlist.Nome == "Músicas da Lauany")
            .ToListAsync();

        if (playlistsExemplo.Count > 0)
            context.Playlists.RemoveRange(playlistsExemplo);

        var artistasExemplo = await context.Artistas
            .Where(artista => artista.Nome == "Artista 1" ||
                              artista.Nome == "Artista 2" ||
                              artista.Nome == "Artista 3")
            .ToListAsync();

        if (artistasExemplo.Count > 0)
            context.Artistas.RemoveRange(artistasExemplo);

        if (musicasExemplo.Count > 0 || playlistsExemplo.Count > 0 || artistasExemplo.Count > 0)
            await context.SaveChangesAsync();
    }

    private static async Task<Usuario> ObterOuCriarUsuarioAsync(
        LiMusicContext context,
        string nome,
        string login,
        string senha)
    {
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(item => item.Login == login || item.Nome == nome);

        if (usuario is null)
        {
            usuario = new Usuario();
            context.Usuarios.Add(usuario);
        }

        usuario.Nome = nome;
        usuario.Login = login;
        usuario.SenhaHash = SenhaService.CriarHash(senha);

        await context.SaveChangesAsync();
        return usuario;
    }

    private static async Task<int> ImportarPastaAsync(
        LiMusicContext context,
        string webRootPath,
        string pastaUsuario,
        Usuario usuario)
    {
        var pastaAudios = Path.Combine(webRootPath, "audios", pastaUsuario);
        var pastaCapas = Path.Combine(webRootPath, "capas", pastaUsuario);

        Directory.CreateDirectory(pastaAudios);
        Directory.CreateDirectory(pastaCapas);
        Directory.CreateDirectory(Path.Combine(webRootPath, "artistas"));

        var arquivos = Directory
            .EnumerateFiles(pastaAudios)
            .Where(arquivo => ExtensoesAudio.Contains(
                Path.GetExtension(arquivo),
                StringComparer.OrdinalIgnoreCase))
            .Select(arquivo => CriarDadosArquivo(arquivo, pastaCapas, pastaUsuario))
            .Where(item => item is not null)
            .Cast<DadosArquivo>()
            .OrderBy(item => item.Titulo)
            .ToList();

        var artistasBanco = await context.Artistas.ToListAsync();
        var artistas = artistasBanco.ToDictionary(
            item => Normalizar(item.Nome),
            item => item,
            StringComparer.OrdinalIgnoreCase);

        foreach (var nomeArtista in arquivos.Select(item => item.Artista).Distinct())
        {
            var chave = Normalizar(nomeArtista);
            if (artistas.ContainsKey(chave))
                continue;

            var artista = new Artista
            {
                Nome = nomeArtista,
                FotoUrl = ObterOuCriarFotoArtista(webRootPath, nomeArtista)
            };

            context.Artistas.Add(artista);
            artistas[chave] = artista;
        }

        await context.SaveChangesAsync();

        var musicasBanco = await context.Musicas.ToListAsync();
        var musicasPorUrl = musicasBanco.ToDictionary(
            item => item.CaminhoArquivo,
            item => item,
            StringComparer.OrdinalIgnoreCase);

        var alteracoes = 0;
        foreach (var item in arquivos)
        {
            var artista = artistas[Normalizar(item.Artista)];

            if (!musicasPorUrl.TryGetValue(item.AudioUrl, out var musica))
            {
                musica = new Musica { CaminhoArquivo = item.AudioUrl };
                context.Musicas.Add(musica);
                musicasPorUrl[item.AudioUrl] = musica;
                alteracoes++;
            }

            musica.Titulo = item.Titulo;
            musica.DuracaoSegundos = item.DuracaoSegundos;
            musica.CapaUrl = item.CapaUrl;
            musica.Genero = item.Genero;
            musica.Artista = artista;
            musica.Usuario = usuario;
        }

        await context.SaveChangesAsync();
        await CriarPlaylistBibliotecaAsync(context, usuario);
        return alteracoes;
    }

    private static async Task CriarPlaylistBibliotecaAsync(
        LiMusicContext context,
        Usuario usuario)
    {
        var nome = $"Biblioteca de {usuario.Nome}";
        var playlist = await context.Playlists
            .Include(item => item.PlaylistMusicas)
            .FirstOrDefaultAsync(item => item.UsuarioId == usuario.Id && item.Nome == nome);

        if (playlist is null)
        {
            playlist = new Playlist
            {
                Nome = nome,
                Descricao = $"Todas as músicas de {usuario.Nome}.",
                UsuarioId = usuario.Id
            };
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();
        }

        var idsNaPlaylist = playlist.PlaylistMusicas
            .Select(item => item.MusicaId)
            .ToHashSet();

        var idsMusicas = await context.Musicas
            .Where(musica => musica.UsuarioId == usuario.Id)
            .Select(musica => musica.Id)
            .ToListAsync();

        foreach (var musicaId in idsMusicas.Where(id => !idsNaPlaylist.Contains(id)))
        {
            context.PlaylistMusicas.Add(new PlaylistMusica
            {
                PlaylistId = playlist.Id,
                MusicaId = musicaId
            });
        }

        await context.SaveChangesAsync();
    }

    private static DadosArquivo? CriarDadosArquivo(
        string arquivoAudio,
        string pastaCapas,
        string pastaUsuario)
    {
        var nomeBase = Path.GetFileNameWithoutExtension(arquivoAudio);
        var separador = nomeBase.IndexOf('_');

        if (separador <= 0 || separador >= nomeBase.Length - 1)
            return null;

        var parteTitulo = nomeBase[..separador];
        var parteArtista = nomeBase[(separador + 1)..];

        var titulo = Titulos.TryGetValue(parteTitulo, out var tituloConhecido)
            ? tituloConhecido
            : SepararPalavras(parteTitulo);

        var artista = NomesArtistas.TryGetValue(parteArtista, out var artistaConhecido)
            ? artistaConhecido
            : SepararPalavras(parteArtista);

        var arquivoCapa = EncontrarArquivoComMesmoNome(pastaCapas, nomeBase);
        var capaUrl = arquivoCapa is null
            ? "/capas/sem-capa.svg"
            : $"/capas/{pastaUsuario}/{Uri.EscapeDataString(Path.GetFileName(arquivoCapa))}";

        var duracao = Duracoes.TryGetValue(nomeBase, out var segundos) ? segundos : 180;
        var genero = Generos.TryGetValue(artista, out var generoConhecido)
            ? generoConhecido
            : "Não informado";

        return new DadosArquivo(
            titulo,
            artista,
            $"/audios/{pastaUsuario}/{Uri.EscapeDataString(Path.GetFileName(arquivoAudio))}",
            capaUrl,
            duracao,
            genero);
    }

    private static string? EncontrarArquivoComMesmoNome(string pasta, string nomeBase)
    {
        foreach (var extensao in ExtensoesImagem)
        {
            var caminho = Path.Combine(pasta, nomeBase + extensao);
            if (File.Exists(caminho))
                return caminho;
        }

        return Directory
            .EnumerateFiles(pasta)
            .FirstOrDefault(arquivo =>
                string.Equals(
                    Path.GetFileNameWithoutExtension(arquivo),
                    nomeBase,
                    StringComparison.OrdinalIgnoreCase) &&
                ExtensoesImagem.Contains(
                    Path.GetExtension(arquivo),
                    StringComparer.OrdinalIgnoreCase));
    }

    private static string ObterOuCriarFotoArtista(
        string webRootPath,
        string nomeArtista)
    {
        var pasta = Path.Combine(webRootPath, "artistas");
        Directory.CreateDirectory(pasta);

        var slug = CriarSlug(nomeArtista);
        foreach (var extensao in ExtensoesImagem)
        {
            var existente = Path.Combine(pasta, slug + extensao);
            if (File.Exists(existente))
                return $"/artistas/{Uri.EscapeDataString(Path.GetFileName(existente))}";
        }

        var caminhoSvg = Path.Combine(pasta, slug + ".svg");
        var iniciais = string.Concat(nomeArtista
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(parte => char.ToUpperInvariant(parte[0])));

        var nomeSeguro = SecurityElement.Escape(nomeArtista) ?? nomeArtista;
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 300 300" role="img" aria-label="{{nomeSeguro}}">
              <defs>
                <linearGradient id="fundo" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0" stop-color="#f7a3c2"/>
                  <stop offset="1" stop-color="#74203e"/>
                </linearGradient>
              </defs>
              <rect width="300" height="300" rx="60" fill="#260816"/>
              <circle cx="150" cy="150" r="112" fill="url(#fundo)" opacity=".92"/>
              <text x="150" y="177" text-anchor="middle" font-family="Arial, sans-serif" font-size="82" font-weight="700" fill="#260816">{{iniciais}}</text>
            </svg>
            """;

        File.WriteAllText(caminhoSvg, svg);
        return $"/artistas/{slug}.svg";
    }

    private static string CriarSlug(string valor)
    {
        var normalizado = valor.Normalize(System.Text.NormalizationForm.FormD);
        var semAcentos = new string(normalizado
            .Where(caractere => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(caractere) !=
                                System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());

        return System.Text.RegularExpressions.Regex
            .Replace(semAcentos.ToLowerInvariant(), "[^a-z0-9]+", "-")
            .Trim('-');
    }

    private static string Normalizar(string valor)
        => string.Join(
            " ",
            valor.Trim().ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static string SepararPalavras(string valor)
    {
        var texto = valor.Replace('_', ' ').Replace('-', ' ');
        return System.Text.RegularExpressions.Regex
            .Replace(texto, "(?<=[a-záéíóúç])(?=[A-Z])", " ")
            .Trim();
    }

    private sealed record DadosArquivo(
        string Titulo,
        string Artista,
        string AudioUrl,
        string CapaUrl,
        int DuracaoSegundos,
        string Genero);
}
