using System.ComponentModel.DataAnnotations;

public class Contrato
{
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; }

    [Required]
    [Range(1, 9999999)]
    public decimal MontoMensual { get; set; }

    public decimal? MontoMulta { get; set; }

    [Required]
    public int InquilinoId { get; set; }

    public Inquilino Inquilino { get; set; }

    [Required]
    public int InmuebleId { get; set; }

    public Inmueble Inmueble { get; set; }

    public DateTime? FechaTerminacionAnticipada { get; set; }
    

    // Auditoría
    public int UsuarioAltaId { get; set; }
    public Usuario UsuarioAlta { get; set; }

    public int? UsuarioBajaId { get; set; }
    public Usuario UsuarioBaja { get; set; }
}

