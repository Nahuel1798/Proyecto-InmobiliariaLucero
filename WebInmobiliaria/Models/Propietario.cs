using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Propietario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string? Dni { get; set; }

    [Required]
    [StringLength(50)]
    public string? Apellido { get; set; }

    [Required]
    [StringLength(50)]
    public string? Nombre { get; set; }

    [Display(Name = "Teléfono")]
    [Phone]
    public string? Telefono { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    // Relación 1 a muchos con Inmueble
    public List<Inmueble> Inmuebles { get; set; } = new();

    public override string ToString()
    {
        var res = $"{Nombre} {Apellido}";
        if (!string.IsNullOrEmpty(Dni))
        {
            res += $" ({Dni})";
        }
        return res;
    }
}

