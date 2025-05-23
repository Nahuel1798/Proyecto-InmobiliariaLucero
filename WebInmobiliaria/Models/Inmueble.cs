using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Inmueble
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? Direccion { get; set; }

    [Required]
    [Display(Name = "Uso del Inmueble")]
    [RegularExpression("^(Residencial|Comercial)$", ErrorMessage = "El uso debe ser Residencial o Comercial.")]
    public string? Uso { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un tipo de inmueble.")]
    [Display(Name = "Tipo de Inmueble")]
    public int TipoInmuebleId { get; set; }

    [ForeignKey(nameof(TipoInmuebleId))]
    public TipoInmueble? TipoInmueble { get; set; }

    [Range(1, 20, ErrorMessage = "Debe tener al menos 1 ambiente.")]
    public int Ambientes { get; set; }

    [Range(-90, 90)]
    public double Latitud { get; set; }

    [Range(-180, 180)]
    public double Longitud { get; set; }

    [Range(1, 99999999)]
    public decimal Precio { get; set; }

    [Display(Name = "¿Disponible?")]
    public bool Estado { get; set; } = true;

    [Required(ErrorMessage = "Debes seleccionar un propietario.")]
    [Display(Name = "Propietario")]
    public int PropietarioId { get; set; }

    [ForeignKey(nameof(PropietarioId))]
    public Propietario? Propietario { get; set; }
    //Un inmueble puede tener muchos contratos
    public List<Contrato> Contratos { get; set; } = new();
}
