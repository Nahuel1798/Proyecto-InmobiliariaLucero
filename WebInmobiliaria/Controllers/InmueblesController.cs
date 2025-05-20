using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebInmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly InmobiliariaContext _context;

        public InmueblesController(InmobiliariaContext context)
        {
            _context = context;
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index()
        {
            var inmobiliariaContext = _context.Inmuebles.Include(i => i.Propietario);
            return View(await inmobiliariaContext.ToListAsync());
        }

        // GET: Inmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles
                .Include(i => i.Propietario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public IActionResult Create()
        {
            var inmueble = new Inmueble();
            ViewData["PropietarioId"] = new SelectList(
                _context.Propietarios
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Nombre + " " + p.Apellido
                    }).ToList(),
                "Value", "Text", inmueble?.PropietarioId
            );

            ViewData["TipoInmuebleId"] = new SelectList(
                _context.TipoInmueble
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Descripcion
                    }).ToList(),
                "Value", "Text"
            );

            return View();
        }

        // POST: Inmuebles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Direccion,Uso,TipoInmuebleId,Ambientes,Latitud,Longitud,Precio,Estado,PropietarioId")] Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inmueble);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropietarioId"] = new SelectList(
                _context.Propietarios
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Nombre + " " + p.Apellido
                    }).ToList(),
                "Value", "Text", inmueble?.PropietarioId
            );
            ViewData["TipoInmuebleId"] = new SelectList(
                _context.TipoInmueble
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Descripcion
                    }).ToList(),
                "Value", "Text", inmueble?.TipoInmuebleId
            );
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            ViewData["PropietarioId"] = new SelectList(
                _context.Propietarios.Select(p => new
                {
                    p.Id,
                    NombreCompleto = p.Nombre + " " + p.Apellido
                }),
                "Id",
                "NombreCompleto",
                inmueble.PropietarioId
            );

            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Direccion,Uso,Tipo,Ambientes,Latitud,Longitud,Precio,Estado,PropietarioId")] Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InmuebleExists(inmueble.Id))
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
            ViewData["PropietarioId"] = new SelectList(
                _context.Propietarios.Select(p => new
                {
                    p.Id,
                    NombreCompleto = p.Nombre + " " + p.Apellido
                }),
                "Id",
                "NombreCompleto",
                inmueble.PropietarioId
            );

            return View(inmueble);
        }

        public async Task<IActionResult> PorPropietario(int id)
        {
            var propietario = await _context.Propietarios.FindAsync(id);
            if (propietario == null)
            {
                return NotFound();
            }

            var inmuebles = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .Where(i => i.PropietarioId == id)
                .ToListAsync();

            ViewBag.Propietario = propietario;
            return View(inmuebles);
        }


        [Authorize]
        // GET: Inmuebles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles
                .Include(i => i.Propietario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                _context.Inmuebles.Remove(inmueble);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InmuebleExists(int id)
        {
            return _context.Inmuebles.Any(e => e.Id == id);
        }
    }
}