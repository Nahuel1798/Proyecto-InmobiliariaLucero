using System.ComponentModel.DataAnnotations;
public class Usuario
{
    public int Id { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Clave { get; set; }
    public string Rol { get; set; } // "Administrador" o "Empleado"
    public string? Avatar { get; set; }
}

