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
    public class PagosController : Controller
    {
        private readonly InmobiliariaContext _context;

        public PagosController(InmobiliariaContext context)
        {
            _context = context;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            var inmobiliariaContext = _context.Pagos.Include(p => p.Contrato);
            return View(await inmobiliariaContext.ToListAsync());
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            ViewData["ContratoId"] = _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .ToList()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Inquilino?.NombreCompleto} - {c.Inmueble?.Direccion}"
                }).ToList();
            return View();
        }

        // POST: Pagos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FechaPago,Importe,ContratoId,NumeroPeriodo,Observaciones")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                // Calcular número de período automáticamente
                var numeroPago = await _context.Pagos
                    .Where(p => p.ContratoId == pago.ContratoId && !p.Anulado)
                    .CountAsync() + 1;

                pago.NumeroPeriodo = numeroPago;
                pago.FechaPago = DateTime.Now;
                pago.Anulado = false;

                _context.Add(pago);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["ContratoId"] = _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .ToList()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Inquilino?.NombreCompleto} - {c.Inmueble?.Direccion}"
                }).ToList();
            return View(pago);
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            ViewData["ContratoId"] = _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .ToList()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Inquilino?.NombreCompleto} - {c.Inmueble?.Direccion}"
                }).ToList();
            return View(pago);
        }

        // POST: Pagos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaPago,Importe,ContratoId,NumeroPeriodo,Observaciones")] Pago pago)
        {
            if (id != pago.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoExists(pago.Id))
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
            ViewData["ContratoId"] = _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .ToList()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Inquilino?.NombreCompleto} - {c.Inmueble?.Direccion}"
                }).ToList();
            return View(pago);
        }

        //Listar pagos realizados
        public async Task<IActionResult> PorContrato(int id)
        {
            var contrato = await _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contrato == null)
            {
                return NotFound();
            }

            var pagos = await _context.Pagos
                .Where(p => p.ContratoId == id)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            ViewBag.Contrato = contrato;
            ViewBag.ContratoId = id;

            return View(pagos);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> RegistrarPagoMulta(int contratoId, decimal importe)
        {
            var pago = new Pago
            {
                ContratoId = contratoId,
                Importe = importe,
                FechaPago = DateTime.Today,
                Concepto = "Pago por multa de finalización anticipada"
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Contrato");
        }


        [HttpPost]
        public async Task<IActionResult> CrearDesdeContrato(Pago pago)
        {
            // Calcular el número de período
            var numeroPago = await _context.Pagos
                .Where(p => p.ContratoId == pago.ContratoId && !p.Anulado)
                .CountAsync() + 1;

            pago.NumeroPeriodo = numeroPago;
            pago.FechaPago = DateTime.Now;
            pago.Concepto = $"Abono mes {numeroPago}";
            pago.Anulado = false;

            // Asignar usuario logueado
            var usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
            pago.UsuarioAltaId = usuarioId;

            // 🔥 RESETEAR Y VOLVER A VALIDAR
            ModelState.Clear();
            TryValidateModel(pago);

            if (ModelState.IsValid)
            {
                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();
                return RedirectToAction("PorContrato", new { id = pago.ContratoId });
            }

            // Si hay errores, volver a cargar la vista
            var contrato = await _context.Contratos
                .Include(c => c.Inquilino)
                .Include(c => c.Inmueble)
                .FirstOrDefaultAsync(c => c.Id == pago.ContratoId);

            var pagos = await _context.Pagos
                .Where(p => p.ContratoId == pago.ContratoId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            ViewBag.Contrato = contrato;
            ViewBag.ContratoId = pago.ContratoId;

            return View("PorContrato", pagos);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Anular(int id)
        {
            var pago = await _context.Pagos.FirstOrDefaultAsync(p => p.Id == id);

            if (pago == null || pago.Anulado)
                return NotFound();

            var usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
            pago.Anulado = true;
            pago.UsuarioAnulacionId = usuarioId;

            _context.Update(pago);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"El pago #{pago.Id} fue anulado correctamente.";
            return RedirectToAction("PorContrato",pago);
        }


        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Contrato)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PagoExists(int id)
        {
            return _context.Pagos.Any(e => e.Id == id);
        }
    }
}