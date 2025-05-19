using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class UsuariosController : Controller
{
    private readonly InmobiliariaContext context;
    private readonly IWebHostEnvironment environment;

    public UsuariosController(InmobiliariaContext ctx, IWebHostEnvironment env)
    {
        context = ctx;
        environment = env;
    }

    // ==============================
    // LOGIN
    // ==============================

    [AllowAnonymous]
        public IActionResult Login()
        {
            return View(); // Esto debe buscar Views/Usuario/Login.cshtml
        }



    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string email, string clave)
    {
        var usuario = context.Usuarios.FirstOrDefault(u => u.Email == email);

        if (usuario != null && usuario.Clave == clave) // ⚠️ usar hash
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Id", usuario.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

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
    // ABM (solo ADMINISTRADORES)
    // ==============================

    [Authorize(Roles = "Administrador")]
    public IActionResult Index()
    {
        var usuarios = context.Usuarios.ToList();
        return View(usuarios);
    }

    [AllowAnonymous]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Usuario u)
    {
        if (ModelState.IsValid)
        {
            context.Add(u);
            await context.SaveChangesAsync();
            TempData["Mensaje"] = "Usuario creado";
            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Edit(Usuario u)
    {
        if (ModelState.IsValid)
        {
            context.Update(u);
            await context.SaveChangesAsync();
            TempData["Mensaje"] = "Usuario actualizado";
            return RedirectToAction(nameof(Index));
        }

        return View(u);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await context.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        context.Usuarios.Remove(u);
        await context.SaveChangesAsync();
        TempData["Mensaje"] = "Usuario eliminado";
        return RedirectToAction(nameof(Index));
    }

    // ==============================
    // PERFIL DE USUARIO (Empleado o Admin)
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
            usuario.Clave = nuevaClave; // ⚠️ aplicar hashing en producción
        }

        if (avatarNuevo != null && avatarNuevo.Length > 0)
        {
            string ruta = Path.Combine(environment.WebRootPath, "img", "avatars");
            if (!Directory.Exists(ruta)) Directory.CreateDirectory(ruta);

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
    // Acceso denegado
    // ==============================

    [AllowAnonymous]
    public IActionResult Denegado()
    {
        return View();
    }
}