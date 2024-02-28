namespace rutacart.Models;
using System.ComponentModel.DataAnnotations;

public class Roles
{
    [Key]
    public int RolId { get; set; }
    [Required]
    [MaxLength(255)]
    public string NombreRol { get; set; }

    // Colección de usuarios para la relación uno a muchos
    public virtual ICollection<Usuarios> Usuarios { get; set; } = new HashSet<Usuarios>();
}
