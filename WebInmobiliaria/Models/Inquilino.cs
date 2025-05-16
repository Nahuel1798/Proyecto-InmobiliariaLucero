using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

public class Inquilino
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? Dni { get; set; }
    [Required]
    public string? NombreCompleto { get; set; }
    public string? Telefono { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }
    public List<Contrato> Contratos { get; set; } = new();
}
