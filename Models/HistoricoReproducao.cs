namespace LI_Music.Models
{
    public class HistoricoReproducao
    {
        public int Id { get; set; }

        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public DateTime TocadoEm { get; set; } = DateTime.Now;
    }
}