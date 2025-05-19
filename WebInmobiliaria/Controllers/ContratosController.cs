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
        public async Task<IActionResult> Index()
        {
            var inmobiliariaContext = _context.Contratos.Include(c => c.Inmueble).Include(c => c.Inquilino);
            return View(await inmobiliariaContext.ToListAsync());
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
        public IActionResult Create()
        {
            ViewData["InquilinoId"] = new SelectList(_context.Inquilinos, "Id", "NombreCompleto");
            ViewData["InmuebleId"] = new SelectList(_context.Inmuebles.Where(i => i.Estado), "Id", "Direccion");
            return View();
        }

        // POST: Contratos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FechaInicio,FechaFin,MontoMensual,InquilinoId,InmuebleId,FechaTerminacionAnticipada")] Contrato contrato)
        {
            if (ExisteSuperposicionFechas(contrato.InmuebleId, contrato.FechaInicio, contrato.FechaFin))
            {
                ModelState.AddModelError("", "El inmueble ya está alquilado en ese período.");
            }

            foreach(var modelState in ModelState.Values)
            {
                foreach(var error in modelState.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                contrato.UsuarioAltaId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
                contrato.UsuarioBajaId = null;
                _context.Add(contrato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["InquilinoId"] = new SelectList(_context.Inquilinos, "Id", "NombreCompleto");
            ViewData["InmuebleId"] = new SelectList(_context.Inmuebles.Where(i => i.Estado), "Id", "Direccion");
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

        //Trae los contratos vigentes
        [Authorize]
        public async Task<IActionResult> Vigentes()
        {
            var hoy = DateTime.Today;
            var contratos = await _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .Where(c => c.FechaInicio <= hoy && c.FechaFin >= hoy && c.FechaTerminacionAnticipada == null)
                .ToListAsync();

            return View(contratos);
        }

        //Trae los contratos por inmueble
        [Authorize]
        public async Task<IActionResult> PorInmueble(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();

            var contratos = await _context.Contratos
                .Include(c => c.Inquilino)
                .Where(c => c.InmuebleId == id)
                .ToListAsync();

            ViewBag.Inmueble = inmueble;
            return View(contratos);
        }

        //Renovar contrato
        [Authorize]
        public IActionResult Renovar(int id)
        {
            var original = _context.Contratos.Find(id);

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Renovar(Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contrato);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(contrato);
        }


        //Finaliza contrato anticipadamente
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> Finalizar(int id)
        {
            var contrato = await _context.Contratos
                .Include(c => c.Inmueble)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contrato == null || contrato.FechaTerminacionAnticipada != null)
                return NotFound();

            var hoy = DateTime.Today;
            var duracionTotal = (contrato.FechaFin - contrato.FechaInicio).TotalDays;
            var duracionTranscurrida = (hoy - contrato.FechaInicio).TotalDays;

            bool mitadOmenos = duracionTranscurrida < (duracionTotal / 2);
            var multa = contrato.MontoMensual * (mitadOmenos ? 2 : 1);

            ViewBag.Multa = multa;
            ViewBag.MesesAdeudados = await _context.Pagos.CountAsync(p => p.ContratoId == contrato.Id) < 
                                    ((hoy.Month - contrato.FechaInicio.Month) + (12 * (hoy.Year - contrato.FechaInicio.Year)));

            return View(contrato);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarFinalizacion(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null) return NotFound();

            contrato.FechaTerminacionAnticipada = DateTime.Today;

            // Cálculo de multa
            var total = (contrato.FechaFin - contrato.FechaInicio).TotalDays;
            var actual = (DateTime.Today - contrato.FechaInicio).TotalDays;
            bool mitad = actual < (total / 2);
            contrato.MontoMulta = contrato.MontoMensual * (mitad ? 2 : 1);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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