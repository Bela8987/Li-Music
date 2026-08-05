using System.ComponentModel.DataAnnotations;

namespace LI_Music.DTOs
{
    public class HistoricoReproducaoRequestDto
    {
        [Required]
        public int MusicaId { get; set; }

        [Required]
        public int UsuarioId { get; set; }
    }
}