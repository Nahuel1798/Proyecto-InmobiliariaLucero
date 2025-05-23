using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Pago
{
    [Key]
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de pago")]
    public DateTime FechaPago { get; set; }

    [Required]
    [Range(1, 9999999)]
    [Display(Name = "Importe")]
    public decimal Importe { get; set; }

    [Required]
    [Display(Name = "Contrato")]
    public int ContratoId { get; set; }
    
    [ForeignKey(nameof(ContratoId))]
    public Contrato? Contrato { get; set; }

    [Required]
    [Range(1, 240, ErrorMessage = "El número de pago debe ser entre 1 y 240.")]
    [Display(Name = "Número de pago")]
    public int NumeroPeriodo { get; set; } // Ej: "1" para el primer mes

    [StringLength(100)]
    [Display(Name = "Concepto")]
    public string Concepto { get; set; } = string.Empty; // Ej: "Abono mes mayo"

    [StringLength(250)]
    public string? Observaciones { get; set; }

    [Display(Name = "¿Anulado?")]
    public bool Anulado { get; set; } = false;

    // Auditoría
    [Display(Name = "Registrado por")]
    public int UsuarioAltaId { get; set; }
    public Usuario? UsuarioAlta { get; set; }

    [Display(Name = "Anulado por")]
    public int? UsuarioAnulacionId { get; set; }
    public Usuario? UsuarioAnulacion { get; set; }
}
