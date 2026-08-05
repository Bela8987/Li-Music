# LI Music atualizado

## Acessos iniciais

- Isabela: `isabela123`
- Lauany: `lauany123`

## Executar

1. Confira a conexão SQL Server em `appsettings.json`.
2. Abra a solução `LI Music.sln` no Visual Studio.
3. Restaure os pacotes NuGet.
4. Execute o projeto.

Na primeira inicialização, a API aplica as migrations, cria os dois usuários e importa os arquivos das pastas de áudio.

## Adicionar novas músicas

Coloque o MP3 e a capa com o mesmo nome-base:

```text
wwwroot/audios/isabela/Dreams_FleetwoodMac.mp3
wwwroot/capas/isabela/Dreams_FleetwoodMac.jpg
```

ou:

```text
wwwroot/audios/lauany/Numb_LinkinPark.mp3
wwwroot/capas/lauany/Numb_LinkinPark.jpg
```

Depois clique em **Sincronizar arquivos** na página Músicas.

O padrão obrigatório é `Titulo_Artista.extensão`. O primeiro `_` separa o título do artista.

## Fotos de artistas

Coloque a foto em `wwwroot/artistas` e, na página Artistas, clique no artista e salve o caminho, por exemplo:

```text
/artistas/fleetwood-mac.jpg
```

## Principais correções

- Contexto padronizado como `LiMusicContext`.
- `Program.cs` corrigido e sem `app.Run()` duplicado.
- Login validado pela API.
- Músicas separadas por usuário no banco.
- Capas ligadas diretamente às músicas.
- Álbuns removidos por completo.
- Controllers de artistas e playlists implementados.
- Importação automática de áudio e capa.
- Player com aleatório, repetir, progresso, volume e mudo.
- Busca com mensagem “Não encontrado”.
