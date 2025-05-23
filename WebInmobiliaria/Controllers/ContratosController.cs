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
        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string direccionInmueble)
        {
            var contratos = _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                    .ThenInclude(i => i.Propietario)
                .AsQueryable();

            // Buscar contratos cuya FechaInicio y FechaFin estén entre 'desde' y 'hasta'
            if (desde.HasValue && hasta.HasValue)
            {
                contratos = contratos.Where(c =>
                    c.FechaInicio >= desde.Value && c.FechaFin <= hasta.Value);
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

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            return View(await contratos.ToListAsync());
        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Contratos/Create
        public IActionResult Create(string nombreInquilino = "", string direccionInmueble = "")
        {
            var inquilinos = _context.Inquilinos.AsQueryable();
            var inmuebles = _context.Inmuebles.Where(i => i.Estado).AsQueryable(); // solo disponibles

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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Create([Bind("Id,FechaInicio,FechaFin,MontoMensual,InquilinoId,InmuebleId,FechaTerminacionAnticipada")] Contrato contrato, string NombreCompleto, string Direccion)
        {
            if (ExisteSuperposicionFechas(contrato.InmuebleId, contrato.FechaInicio, contrato.FechaFin))
            {
                ModelState.AddModelError("", "El inmueble ya está alquilado en ese período.");
            }

            if (contrato.FechaFin != contrato.FechaInicio.AddYears(1))
            {
                ModelState.AddModelError("FechaFin", "La fecha de finalización debe ser exactamente un año después de la fecha de inicio.");
            }

            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                contrato.UsuarioAltaId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
                contrato.UsuarioBajaId = null;

                // Marcar el inmueble como no disponible
                var inmueble = await _context.Inmuebles.FindAsync(contrato.InmuebleId);
                if (inmueble != null)
                {
                    inmueble.Estado = false;
                    _context.Update(inmueble);
                }

                _context.Add(contrato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si falla, volver a cargar las listas filtradas
            var inquilinos = _context.Inquilinos.AsQueryable();
            var inmuebles = _context.Inmuebles.Where(i => i.Estado).AsQueryable();

            if (!string.IsNullOrWhiteSpace(NombreCompleto))
            {
                inquilinos = inquilinos.Where(i => i.NombreCompleto.Contains(NombreCompleto));
            }

            if (!string.IsNullOrWhiteSpace(Direccion))
            {
                inmuebles = inmuebles.Where(i => i.Direccion.Contains(Direccion));
            }

            ViewBag.NombreBuscado = NombreCompleto;
            ViewBag.DireccionBuscada = Direccion;
            ViewBag.Inquilinos = inquilinos.ToList();
            ViewBag.Inmuebles = inmuebles.ToList();

            return View(contrato);
        }


        // GET: Contratos/Edit/5
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
        [Authorize]
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
            return _context.Contratos.Any(c =>
                c.InmuebleId == inmuebleId &&
                c.FechaFin >= inicio &&
                c.FechaInicio <= fin &&
                c.FechaTerminacionAnticipada == null);
        }
    }
}