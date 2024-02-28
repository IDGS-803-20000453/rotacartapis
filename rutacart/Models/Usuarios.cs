using rutacart.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Usuarios
{
    public Usuarios()
    {
        // Inicializa las colecciones para evitar referencias nulas
        Carritos = new HashSet<Carrito>();
        Pedidos = new HashSet<Pedido>();
    }

    [Key]
    public int UsuarioID { get; set; }
    [Required]
    [MaxLength(255)]
    public string Nombre { get; set; }
    [Required]
    [MaxLength(255)]
    public string Apellido { get; set; }
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }
    public string HashContrasena { get; set; }
    public string SaltContrasena { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime UltimoAcceso { get; set; }
    [ForeignKey("Roles")]
    public int RolId { get; set; }
    public virtual Roles Roles { get; set; } // Navegación hacia Rol
    public virtual ICollection<Carrito> Carritos { get; set; }
    public virtual ICollection<Pedido> Pedidos { get; set; }

    // atributos para el restablecimiento de contraseña
    public string? ResetPasswordToken { get; set; } // Token de restablecimiento de contraseña
    public DateTime? ResetPasswordTokenExpiration { get; set; } // Fecha de expiración del token (nullable)
    // campos para la verificación de correo electrónico
    public string? EmailVerificationToken { get; set; } // Token de verificación de correo
    public DateTime? EmailVerificationTokenExpiration { get; set; } // Fecha de expiración del token de verificación de correo
}
