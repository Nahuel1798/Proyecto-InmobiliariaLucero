using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Inquilino
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string? Dni { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Nombre completo")]
    public string? NombreCompleto { get; set; }

    [Phone]
    public string? Telefono { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }
    //Un inquilino puede tener muchos contratos
    public List<Contrato> Contratos { get; set; } = new();
}
