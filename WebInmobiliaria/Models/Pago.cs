using System.ComponentModel.DataAnnotations;

public class Pago
{
    public int Id { get; set; }

    [Required]
    public DateTime FechaPago { get; set; }

    [Required]
    [Range(1, 9999999)]
    public decimal Importe { get; set; }

    [Required]
    public int ContratoId { get; set; }

    [Required]
    [Range(1, 240)] // por ejemplo: hasta 20 años
    public int NumeroPeriodo { get; set; }

    public string? Observaciones { get; set; }

    public Contrato? Contrato { get; set; }

    [Required]
    public string Concepto { get; set; }

    public bool Anulado { get; set; }

    public int UsuarioAltaId { get; set; }
    public Usuario UsuarioAlta { get; set; }

    public int? UsuarioAnulacionId { get; set; }
    public Usuario UsuarioAnulacion { get; set; }
}
