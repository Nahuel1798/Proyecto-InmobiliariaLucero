using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Contrato
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Inquilino")]
    public int InquilinoId { get; set; }

    [ForeignKey(nameof(InquilinoId))]
    public Inquilino? Inquilino { get; set; }

    [Required]
    [Display(Name = "Inmueble")]
    public int InmuebleId { get; set; }

    [ForeignKey(nameof(InmuebleId))]
    public Inmueble? Inmueble { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime FechaInicio { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de finalización")]
    public DateTime FechaFin { get; set; }

    [Required]
    [Range(1, 9999999)]
    [Display(Name = "Monto mensual")]
    public decimal MontoMensual { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de terminación anticipada")]
    public DateTime? FechaTerminacionAnticipada { get; set; }

    [Display(Name = "Monto de multa")]
    public decimal? MontoMulta { get; set; }

    public List<Pago> Pagos { get; set; } = new();

    // Auditoría
    [Display(Name = "Usuario que creó el contrato")]
    public int UsuarioAltaId { get; set; }

    public Usuario? UsuarioAlta { get; set; }

    [Display(Name = "Usuario que finalizó el contrato")]
    public int? UsuarioBajaId { get; set; }

    public Usuario? UsuarioBaja { get; set; }
}
