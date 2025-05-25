using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class UsuariosController : Controller
{
    private readonly InmobiliariaContext context;
    private readonly IWebHostEnvironment environment;
    private readonly PasswordHasher<string> hasher = new();

    public UsuariosController(InmobiliariaContext ctx, IWebHostEnvironment env)
    {
        context = ctx;
        environment = env;
    }

    // ==============================
    // LOGIN
    // ==============================

    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string email, string clave)
    {
        var usuario = context.Usuarios.FirstOrDefault(u => u.Email == email);

        if (usuario != null && hasher.VerifyHashedPassword(null, usuario.Clave, clave) == PasswordVerificationResult.Success)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Id", usuario.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        TempData["Error"] = "Credenciales incorrectas";
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }

    // ==============================
    // ABM USUARIOS (ADMINISTRADORES)
    // ==============================

    [Authorize(Roles = "Administrador")]
    public IActionResult Index(string EmailUsuario = "", int pagina = 1, int tamañoPagina = 5)
    {
        var query = context.Usuarios.AsQueryable();

        if (!string.IsNullOrEmpty(EmailUsuario))
        {
            query = query.Where(u => u.Email.Contains(EmailUsuario));
        }

        var total = query.Count();

        var items = query
            .OrderBy(u => u.Email)
            .Skip((pagina - 1) * tamañoPagina)
            .Take(tamañoPagina)
            .ToList();

        ViewBag.EmailBuscado = EmailUsuario;

        var modelo = new Paginador<Usuario>(items, total, pagina, tamañoPagina);
        return View(modelo);
    }

    [AllowAnonymous]
    public IActionResult Create() => View();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Usuario u)
    {
        if (ModelState.IsValid)
        {
            u.Clave = hasher.HashPassword(null, u.Clave);
            context.Add(u);
            await context.SaveChangesAsync();
            TempData["Mensaje"] = "Usuario creado";
            return RedirectToAction(nameof(Login));
        }

        return View(u);
    }

    [Authorize(Roles = "Administrador")]
    public IActionResult Edit(int id)
    {
        var u = context.Usuarios.Find(id);
        if (u == null) return NotFound();
        return View(u);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(Usuario actualizado, IFormFile? avatarNuevo)
    {
        var usuario = await context.Usuarios.FindAsync(actualizado.Id);
        if (usuario == null) return NotFound();

        if (ModelState.IsValid)
        {
            usuario.Email = actualizado.Email;
            usuario.Rol = actualizado.Rol;

            if (avatarNuevo != null && avatarNuevo.Length > 0)
            {
                string ruta = Path.Combine(environment.WebRootPath, "img", "avatars");
                if (!Directory.Exists(ruta)) Directory.CreateDirectory(ruta);

                // Borrar avatar anterior
                if (!string.IsNullOrEmpty(usuario.Avatar))
                {
                    var rutaAnterior = Path.Combine(environment.WebRootPath, usuario.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(rutaAnterior))
                        System.IO.File.Delete(rutaAnterior);
                }

                string nombreArchivo = $"avatar_{usuario.Id}{Path.GetExtension(avatarNuevo.FileName)}";
                string rutaCompleta = Path.Combine(ruta, nombreArchivo);

                using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await avatarNuevo.CopyToAsync(fileStream);
                }

                usuario.Avatar = "/img/avatars/" + nombreArchivo;
            }

            context.Update(usuario);
            await context.SaveChangesAsync();

            TempData["Mensaje"] = "Usuario actualizado";
            return RedirectToAction(nameof(Index));
        }

        return View(actualizado);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await context.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        // Borrar avatar si existe
        if (!string.IsNullOrEmpty(u.Avatar))
        {
            var ruta = Path.Combine(environment.WebRootPath, u.Avatar.TrimStart('/'));
            if (System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);
        }

        context.Usuarios.Remove(u);
        await context.SaveChangesAsync();
        TempData["Mensaje"] = "Usuario eliminado";
        return RedirectToAction(nameof(Index));
    }

    // ==============================
    // PERFIL USUARIO (LOGUEADO)
    // ==============================

    [Authorize]
    public IActionResult Perfil()
    {
        int id = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
        var u = context.Usuarios.Find(id);
        if (u == null) return NotFound();
        return View(u);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Perfil(Usuario actualizado, IFormFile? avatarNuevo, string? nuevaClave)
    {
        int id = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
        var usuario = context.Usuarios.Find(id);
        if (usuario == null) return NotFound();

        usuario.Email = actualizado.Email;

        if (!string.IsNullOrEmpty(nuevaClave))
        {
            usuario.Clave = hasher.HashPassword(null, nuevaClave);
        }

        if (avatarNuevo != null && avatarNuevo.Length > 0)
        {
            string ruta = Path.Combine(environment.WebRootPath, "img", "avatars");
            if (!Directory.Exists(ruta)) Directory.CreateDirectory(ruta);

            if (!string.IsNullOrEmpty(usuario.Avatar))
            {
                var rutaAnterior = Path.Combine(environment.WebRootPath, usuario.Avatar.TrimStart('/'));
                if (System.IO.File.Exists(rutaAnterior))
                    System.IO.File.Delete(rutaAnterior);
            }

            string nombreArchivo = $"avatar_{usuario.Id}{Path.GetExtension(avatarNuevo.FileName)}";
            string rutaCompleta = Path.Combine(ruta, nombreArchivo);

            using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await avatarNuevo.CopyToAsync(fileStream);
            }

            usuario.Avatar = "/img/avatars/" + nombreArchivo;
        }

        context.Update(usuario);
        await context.SaveChangesAsync();

        TempData["Mensaje"] = "Perfil actualizado";
        return RedirectToAction(nameof(Perfil));
    }

    [Authorize]
    public async Task<IActionResult> QuitarAvatar()
    {
        int id = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
        var u = await context.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        if (!string.IsNullOrEmpty(u.Avatar))
        {
            string ruta = Path.Combine(environment.WebRootPath, u.Avatar.TrimStart('/'));
            if (System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);

            u.Avatar = null;
            context.Update(u);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Perfil));
    }

    // ==============================
    // ACCESO DENEGADO
    // ==============================

    [AllowAnonymous]
    public IActionResult Denegado() => View();
}
