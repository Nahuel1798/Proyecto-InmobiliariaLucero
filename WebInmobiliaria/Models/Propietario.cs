using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
public class Propietario
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? Dni { get; set; }
    [Required]
    public string? Apellido { get; set; }
    [Required]
    public string? Nombre { get; set; }
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }
    
    public List<Inmueble> Inmuebles { get; set; }

    public override string ToString()
  {
    //return $"{Apellido}, {Nombre}";
    //return $"{Nombre} {Apellido}";
    var res = $"{Nombre} {Apellido}";
    if (!String.IsNullOrEmpty(Dni))
    {
      res += $" ({Dni})";
    }
    return res;
  }
}
