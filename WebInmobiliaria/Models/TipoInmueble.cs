using System.ComponentModel.DataAnnotations;

public class TipoInmueble
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Descripcion { get; set; } = string.Empty;

    public List<Inmueble> Inmuebles { get; set; } = new();
}
