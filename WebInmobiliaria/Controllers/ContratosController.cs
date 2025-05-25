using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria.Controllers
{
    public class ContratosController : Controller
    {
        private readonly InmobiliariaContext _context;

        public ContratosController(InmobiliariaContext context)
        {
            _context = context;
        }

        // GET: Contratos
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string direccionInmueble, int pagina = 1, int tamañoPagina = 5)
        {
            var contratos = _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble).ThenInclude(i => i.Propietario)
                .AsQueryable();

            if (desde.HasValue && hasta.HasValue)
            {
                contratos = contratos.Where(c => c.FechaInicio >= desde.Value && c.FechaFin <= hasta.Value);
            }
            else if (desde.HasValue)
            {
                contratos = contratos.Where(c => c.FechaInicio >= desde.Value);
            }
            else if (hasta.HasValue)
            {
                contratos = contratos.Where(c => c.FechaFin <= hasta.Value);
            }

            if (!string.IsNullOrEmpty(direccionInmueble))
            {
                contratos = contratos.Where(c => c.Inmueble.Direccion.Contains(direccionInmueble));
            }

            // Guardar filtros para que los inputs mantengan los valores
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Direccion = direccionInmueble;

            // Paginación
            var total = await contratos.CountAsync();

            var items = await contratos
                .OrderByDescending(c => c.FechaInicio)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            var modelo = new Paginador<Contrato>(items, total, pagina, tamañoPagina);
            return View(modelo);
        }


        // GET: Contratos/Details/5
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .Include(c => c.UsuarioAlta)
                .Include(c => c.UsuarioBaja)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }


        // GET: Contratos/Create
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Create(string nombreInquilino = "", string direccionInmueble = "")
        {
            var inquilinos = _context.Inquilinos.AsQueryable();
            var inmuebles = _context.Inmuebles.Where(i => i.Estado); // solo disponibles

            if (!string.IsNullOrWhiteSpace(nombreInquilino))
            {
                inquilinos = inquilinos.Where(i => i.NombreCompleto.Contains(nombreInquilino));
            }

            if (!string.IsNullOrWhiteSpace(direccionInmueble))
            {
                inmuebles = inmuebles.Where(i => i.Direccion.Contains(direccionInmueble));
            }

            ViewBag.NombreBuscado = nombreInquilino;
            ViewBag.DireccionBuscada = direccionInmueble;
            ViewBag.Inquilinos = inquilinos.ToList();
            ViewBag.Inmuebles = inmuebles.ToList();

            return View();
        }

        // POST: Contratos/Create
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Create([Bind("Id,FechaInicio,FechaFin,InquilinoId,InmuebleId,FechaTerminacionAnticipada")] Contrato contrato)
        {
            Console.WriteLine($"InquilinoId recibido: {contrato.InquilinoId}");
            Console.WriteLine($"InmuebleId recibido: {contrato.InmuebleId}");

            // Validación de superposición
            if (ExisteSuperposicionFechas(contrato.InmuebleId, contrato.FechaInicio, contrato.FechaFin))
            {
                ModelState.AddModelError("", "El inmueble ya está alquilado en ese período.");
            }

            // Validación de duración mínima y máxima
            var duracion = contrato.FechaFin - contrato.FechaInicio;
            if (duracion.TotalDays < 30)
            {
                ModelState.AddModelError("FechaFin", "La duración mínima del contrato debe ser de 1 mes.");
            }
            else if (duracion.TotalDays > 365 * 5)
            {
                ModelState.AddModelError("FechaFin", "La duración máxima del contrato es de 5 años.");
            }

            if (ModelState.IsValid)
            {
                var inmueble = await _context.Inmuebles.FindAsync(contrato.InmuebleId);
                if (inmueble == null)
                {
                    ModelState.AddModelError("", "El inmueble seleccionado no existe.");
                }
                else
                {
                    contrato.MontoMensual = inmueble.Precio;
                    inmueble.Estado = false;
                    _context.Update(inmueble);

                    contrato.UsuarioAltaId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
                    contrato.UsuarioBajaId = null;

                    _context.Add(contrato);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            // Si hay errores, recargar las listas sin filtros o con filtros opcionales (si quieres)
            ViewBag.Inquilinos = _context.Inquilinos.ToList();
            ViewBag.Inmuebles = _context.Inmuebles.Where(i => i.Estado).ToList();

            return View(contrato);
        }




        // GET: Contratos/Edit/5
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null)
            {
                return NotFound();
            }
            ViewData["InquilinoId"] = new SelectList(_context.Inquilinos, "Id", "NombreCompleto");
            ViewData["InmuebleId"] = new SelectList(_context.Inmuebles.Where(i => i.Estado), "Id", "Direccion");
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaInicio,FechaFin,MontoMensual,InquilinoId,InmuebleId,FechaTerminacionAnticipada")] Contrato contrato)
        {
            if (id != contrato.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contrato);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoExists(contrato.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["InquilinoId"] = new SelectList(_context.Inquilinos, "Id", "NombreCompleto");
            ViewData["InmuebleId"] = new SelectList(_context.Inmuebles.Where(i => i.Estado), "Id", "Direccion");
            return View(contrato);
        }


        //GET:  Contratos/Vigentes
        [Authorize]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Renovar(int id)
        {
            var original = _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .FirstOrDefault(c => c.Id == id);

            if (original == null)
                return NotFound();

            var nuevo = new Contrato
            {
                InmuebleId = original.InmuebleId,
                InquilinoId = original.InquilinoId,
                MontoMensual = original.MontoMensual,
                FechaInicio = original.FechaFin.AddDays(1),
                FechaFin = original.FechaFin.AddYears(1),
            };

            ViewBag.Inmueble = original.Inmueble?.Direccion;
            ViewBag.Inquilino = original.Inquilino?.NombreCompleto;

            return View(nuevo);
        }

        //POST:  Contratos/Vigentes
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Renovar(Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                contrato.Id = 0;
                var usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);

                // Buscar el contrato original activo
                var original = await _context.Contratos
                    .FirstOrDefaultAsync(c =>
                        c.InquilinoId == contrato.InquilinoId &&
                        c.InmuebleId == contrato.InmuebleId &&
                        c.FechaTerminacionAnticipada == null &&
                        c.FechaFin >= DateTime.Today);

                if (original != null)
                {
                    _context.Contratos.Remove(original); // ⬅️ Eliminar contrato anterior
                }

                contrato.UsuarioAltaId = usuarioId;
                _context.Add(contrato);

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Si algo falla, volvemos a cargar los datos visibles
            var originalContrato = await _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .FirstOrDefaultAsync(c =>
                    c.InquilinoId == contrato.InquilinoId &&
                    c.InmuebleId == contrato.InmuebleId);

            ViewBag.Inquilino = originalContrato?.Inquilino?.NombreCompleto;
            ViewBag.Inmueble = originalContrato?.Inmueble?.Direccion;

            return View(contrato);
        }


        //Finaliza contrato anticipadamente
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> FinalizarAnticipadamente(int id)
        {
            var contrato = await _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contrato == null || contrato.FechaTerminacionAnticipada != null)
            {
                return NotFound();
            }

            var hoy = DateTime.Today;
            var duracionTotal = (contrato.FechaFin - contrato.FechaInicio).TotalDays;
            var diasCumplidos = (hoy - contrato.FechaInicio).TotalDays;
            var mitad = duracionTotal / 2;

            // Calcular multa
            decimal multa = diasCumplidos < mitad ? contrato.MontoMensual * 2 : contrato.MontoMensual;
            contrato.MontoMulta = multa;

            return View("FinalizarAnticipadamente", contrato);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> FinalizarAnticipadamente(Contrato contrato)
        {
            var original = await _context.Contratos.FirstOrDefaultAsync(c => c.Id == contrato.Id);

            if (original == null || original.FechaTerminacionAnticipada != null)
                return NotFound();

            var hoy = DateTime.Today;
            var usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);

            var duracionTotal = (original.FechaFin - original.FechaInicio).TotalDays;
            var diasCumplidos = (hoy - original.FechaInicio).TotalDays;
            var mitad = duracionTotal / 2;

            // Calcular multa
            decimal multa = diasCumplidos < mitad ? original.MontoMensual * 2 : original.MontoMensual;

            original.FechaTerminacionAnticipada = hoy;
            original.MontoMulta = multa;
            original.UsuarioBajaId = usuarioId;

            _context.Update(original);
            // Registrar el pago de la multa
            var pagoMulta = new Pago
            {
                ContratoId = original.Id,
                FechaPago = hoy,
                Importe = multa,
                Concepto = "Multa por finalización anticipada",
                UsuarioAltaId = usuarioId
            };

            _context.Pagos.Add(pagoMulta);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Contrato finalizado anticipadamente. Multa: ${multa:N2}";
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Finalizar(int id)
        {
            var contrato = await _context.Contratos.FirstOrDefaultAsync(p => p.Id == id);

            if (contrato == null || contrato.Estado)
                return NotFound();

            var usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
            contrato.Estado = false;
            contrato.UsuarioBajaId = usuarioId;

            _context.Update(contrato);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"El Contrato #{contrato.Id} fue Finalizado correctamente.";
            return RedirectToAction("FinalizarAnticipadamente",contrato);
        }



        // GET: Contratos/Delete/5
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .Include(c => c.Inmueble)
                .Include(c => c.Inquilino)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato != null)
            {
                _context.Contratos.Remove(contrato);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContratoExists(int id)
        {
            return _context.Contratos.Any(e => e.Id == id);
        }

        private bool ExisteSuperposicionFechas(int inmuebleId, DateTime inicio, DateTime fin)
        {
            var startDate = inicio.Date;
            var endDate = fin.Date;

            return _context.Contratos.Any(c =>
                c.InmuebleId == inmuebleId &&
                c.FechaFin.Date > startDate &&   // termina después de que empieza el nuevo contrato
                c.FechaInicio.Date < endDate &&  // empieza antes de que termine el nuevo contrato
                c.FechaTerminacionAnticipada == null);
        }
    }
}