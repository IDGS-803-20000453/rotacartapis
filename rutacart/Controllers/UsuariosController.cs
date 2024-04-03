using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rutacart.Data;
using rutacart.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Web;

namespace rutacart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(ApplicationDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }



        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation("Inicio del método ResetPassword para el email: {Email}", request.Email);

            if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("La solicitud no puede ser procesada debido a parámetros inválidos.");
            }

            var user = await _context.Usuarios
                                     .Where(u => u.Email == request.Email && u.ResetPasswordToken == request.Token)
                                     .FirstOrDefaultAsync();

            if (user == null || user.ResetPasswordTokenExpiration == null || user.ResetPasswordTokenExpiration < DateTime.UtcNow)
            {
                return BadRequest("El token de restablecimiento de contraseña es inválido o ha expirado.");
            }

            // Aquí, actualizamos la contraseña del usuario
            CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.HashContrasena = Convert.ToBase64String(passwordHash);
            user.SaltContrasena = Convert.ToBase64String(passwordSalt);

            // Limpiar el token de restablecimiento de contraseña y su fecha de expiración
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiration = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "La contraseña se ha restablecido correctamente." });
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            _logger.LogInformation("Inicio del método ForgotPassword para el email: {Email}", model.Email);

            try
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    // Considera retornar siempre un Ok para evitar enumeración de cuentas
                    return Ok("Si tu correo electrónico existe en nuestra base de datos, recibirás un enlace para restablecer tu contraseña.");
                }

                // Genera un token de restablecimiento de contraseña
                var resetToken = Guid.NewGuid().ToString();

                // Asigna el token y su fecha de expiración al usuario
                user.ResetPasswordToken = resetToken;
                user.ResetPasswordTokenExpiration = DateTime.UtcNow.AddMinutes(15); // El token expira en 15 minutos

                // Guarda los cambios en la base de datos
                _context.Usuarios.Update(user);
                await _context.SaveChangesAsync();

                // Envía el correo electrónico
                var emailEncoded = HttpUtility.UrlEncode(user.Email);
                var resetLink = $"http://localhost:4200/reset-password?token={resetToken}&email={emailEncoded}";
                var emailBody = $"<p>Por favor, usa este <a href='{resetLink}'>enlace</a> para restablecer tu contraseña. Este enlace será válido por 15 minutos.</p>";
                await SendEmailAsync(user.Email, "Restablecimiento de Contraseña", emailBody);

                _logger.LogInformation("Correo de restablecimiento enviado a: {Email}", model.Email);

                return Ok(new { message = "Si tu correo electrónico existe en nuestra base de datos, recibirás un enlace para restablecer tu contraseña." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "Error enviando correo de restablecimiento a: {Email}", model.Email);

                return StatusCode(500, "Ocurrió un error al procesar tu solicitud. Por favor, inténtalo de nuevo más tarde.");
            }
        }




        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.Credentials = new NetworkCredential("leofcleon@gmail.com", "qnio meob dwqr qpgu");
                smtpClient.EnableSsl = true; // Depende de tu servidor SMTP

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress("leofcleon@gmail.com", "Soporte Técnico");
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    await smtpClient.SendMailAsync(message);
                }
            }
        }

        // POST: api/Usuarios/SignUp
        [HttpPost("SignUp")]
        public async Task<ActionResult> SignUp([FromBody] UserRegistrationDto registrationDto)
        {
            _logger.LogInformation("Inicio del método SignUp para el email: {Email}", registrationDto.Email);

            if (await UserExists(registrationDto.Email))
            {
                return BadRequest("El correo electrónico ya está en uso.");
            }

            CreatePasswordHash(registrationDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var usuario = new Usuarios
            {
                Nombre = registrationDto.Nombre,
                Apellido = registrationDto.Apellido,
                Email = registrationDto.Email,
                HashContrasena = Convert.ToBase64String(passwordHash),
                SaltContrasena = Convert.ToBase64String(passwordSalt),
                FechaCreacion = DateTime.UtcNow,
                UltimoAcceso = DateTime.UtcNow,
                RolId = 2 // Rol de usuario
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return StatusCode(201); // Created
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            _logger.LogInformation("Inicio del método Login");

            var usuario = await _context.Usuarios
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado: {Email}", loginDto.Email);
                return Unauthorized(new { message = "Las credenciales son incorrectas." });
            }

            if (!VerifyPasswordHash(loginDto.Password, usuario.HashContrasena, usuario.SaltContrasena))
            {
                _logger.LogWarning("Falló la verificación de contraseña para el usuario: {Email}", loginDto.Email);
                return Unauthorized(new { message = "Las credenciales son incorrectas." });
            }

            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(usuario); // Genera el token
            _logger.LogInformation("Login exitoso para el usuario: {Email}", usuario.Email);

            return Ok(new
            {
                message = "Inicio de sesión exitoso.",
                usuarioId = usuario.UsuarioID,
                email = usuario.Email,
                nombre = usuario.Nombre,
                rolId = usuario.RolId,
                imagenURL = usuario.ImagenURL,
                nombreRol = usuario.Roles.NombreRol,
                token // Envía el token como parte de la respuesta
            });
        }


        private string GenerateJwtToken(Usuarios usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Intenta obtener la clave secreta de las variables de entorno o usa un valor predeterminado para el desarrollo
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "una_clave_secreta_muy_larga_para_uso_en_desarrollo";
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            // Más claims según sea necesario
            new Claim(ClaimTypes.Role, usuario.Roles.NombreRol)
        }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }








        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var saltBytes = Convert.FromBase64String(storedSalt);

            using (var hmac = new HMACSHA512(saltBytes))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(hashBytes);
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private async Task<bool> UserExists(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email == email);
        }


        [HttpPost("StartVerification")]
        public async Task<IActionResult> StartVerification([FromBody] UserRegistrationDto registrationDto)
        {
            _logger.LogInformation("Iniciando verificación de usuario con email: {Email}", registrationDto?.Email);

            try
            {
                Usuarios userToVerify = null;

                var user = await _context.Usuarios
                                         .FirstOrDefaultAsync(u => u.Email == registrationDto.Email);

                if (user != null && (user.EmailVerificationToken != null || (user.EmailVerificationTokenExpiration.HasValue && user.EmailVerificationTokenExpiration.Value.AddDays(1) < DateTime.UtcNow)))
                {
                    user.EmailVerificationToken = Guid.NewGuid().ToString();
                    user.EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24);
                    userToVerify = user;
                    _context.Usuarios.Update(user); // Asegúrate de marcar el usuario como actualizado si modificas cualquier propiedad.
                }
                else if (user == null)
                {
                    CreatePasswordHash(registrationDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                    var newUser = new Usuarios
                    {
                        Nombre = registrationDto.Nombre,
                        Apellido = registrationDto.Apellido,
                        Email = registrationDto.Email,
                        HashContrasena = Convert.ToBase64String(passwordHash),
                        SaltContrasena = Convert.ToBase64String(passwordSalt),
                        EmailVerificationToken = Guid.NewGuid().ToString(),
                        EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24),
                        FechaCreacion = DateTime.UtcNow,
                        UltimoAcceso = DateTime.UtcNow,
                        RolId = 2 // Asegúrate de que este valor corresponde a un Rol válido en tu base de datos.
                    };
                    _logger.LogInformation("Creando nuevo usuario para la verificación.");

                    _context.Usuarios.Add(newUser);
                    userToVerify = newUser;
                }
                else
                {
                    _logger.LogWarning("El correo electrónico ya está en uso y verificado.");
                    return BadRequest("El correo electrónico ya está en uso y verificado.");
                }

                // Aquí es donde realmente guardas los cambios en la base de datos.
                await _context.SaveChangesAsync();

                if (userToVerify == null)
                {
                    _logger.LogError("Error inesperado: el objeto userToVerify es nulo");
                    return BadRequest("Error inesperado al procesar la verificación.");
                }

                var verificationToken = userToVerify.EmailVerificationToken;
                var emailTo = userToVerify.Email;
                var verificationLink = $"http://localhost:4200/complete-verification?token={verificationToken}&email={HttpUtility.UrlEncode(emailTo)}";
                var emailBody = $"<p>Por favor, confirma tu correo electrónico haciendo clic en este <a href='{verificationLink}'>enlace</a>. Este enlace será válido por 24 horas.</p>";

                await SendEmailAsync(emailTo, "Verificación de Correo Electrónico", emailBody);

                return Ok(new { message = "Se ha enviado un correo electrónico de verificación. Por favor, revisa tu bandeja de entrada." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al intentar iniciar la verificación de usuario: {Message}", ex.Message);
                return StatusCode(500, "Ocurrió un error al procesar tu solicitud. Por favor, inténtalo de nuevo más tarde.");
            }
        }


        public class RegistrationCompletionRequest
        {
            public string Token { get; set; }
            public string Email { get; set; }
        }

        [HttpPost("CompleteRegistration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] RegistrationCompletionRequest request)
        {
            _logger.LogInformation("Inicio del método CompleteRegistration para el email: {Email}", request.Email);

            if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("La solicitud no es válida.");
            }

            var user = await _context.Usuarios
                                     .FirstOrDefaultAsync(u => u.Email == request.Email && u.EmailVerificationToken == request.Token);

            if (user == null || !user.EmailVerificationTokenExpiration.HasValue || user.EmailVerificationTokenExpiration.Value < DateTime.UtcNow)
            {
                return BadRequest("El token de verificación es inválido o ha expirado.");
            }

            // Verificar el usuario
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiration = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "La cuenta ha sido verificada con éxito. Bienvenido/a a la plataforma." });
        }



        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userUpdateDto)
        {
            if (userUpdateDto == null)
            {
                return BadRequest("Datos de actualización inválidos.");
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound($"Usuario con ID {id} no encontrado.");
            }


            usuario.Nombre = userUpdateDto.Nombre ?? usuario.Nombre;
            usuario.Apellido = userUpdateDto.Apellido ?? usuario.Apellido;
            usuario.ImagenURL = userUpdateDto.ImagenURL ?? usuario.ImagenURL;
           

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // O podrías retornar un Ok si prefieres enviar una respuesta al cliente
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Manejar la excepción si ocurre un problema al guardar los cambios
                _logger.LogError(ex, "Ocurrió un error al actualizar el usuario con ID {UserID}.", id);
                return StatusCode(500, "No se pudo actualizar la información del usuario debido a un error interno.");
            }
        }

        public class UserUpdateDto
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string ImagenURL { get; set; }
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<IActionResult> GetAllUsuarios()
        {
            var usuarios = await _context.Usuarios
                                         .AsNoTracking()
                                         .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/Usuarios/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUsuarioById([FromRoute] int id)
        {
            var usuario = await _context.Usuarios
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(u => u.UsuarioID == id);

            if (usuario == null)
            {
                return NotFound($"Usuario con ID {id} no encontrado.");
            }

            return Ok(usuario);
        }

    }
}
