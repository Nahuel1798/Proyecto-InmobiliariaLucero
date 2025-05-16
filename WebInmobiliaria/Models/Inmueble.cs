using System.ComponentModel.DataAnnotations;

public class Inmueble
{
    public int Id { get; set; }

    [Required]
    public string? Direccion { get; set; }

    [Required]

    public string? Uso { get; set; }

    [Required]
    public string? Tipo { get; set; }

    [Range(1, 20)]
    public int Ambientes { get; set; }

    [Range(-90, 90)]
    public double Latitud { get; set; }

    [Range(-180, 180)]
    public double Longitud { get; set; }

    [Range(1, 99999999)]
    public decimal Precio { get; set; }

    public bool Estado { get; set; } = true;

    [Required(ErrorMessage = "Debes seleccionar un propietario.")]
    [Display(Name = "Propietario")]
    public int PropietarioId { get; set; }
    public Propietario? Propietario { get; set; }

    public List<Contrato> Contratos { get; set; } = new();
}
