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

        public async Task<IActionResult> Index(string direccionInmueble,int pagina = 1, int tamañoPagina = 5)
        {
            var total = await _context.Inmuebles.CountAsync();

            var items = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .Where(i => i.Estado == true)
                .OrderBy(i => i.Direccion)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            if (!string.IsNullOrEmpty(direccionInmueble))
            {
                items = items.Where(i => i.Direccion.Contains(direccionInmueble)).ToList();
            }

            ViewBag.Direccion = direccionInmueble;

            var modelo = new Paginador<Inmueble>(items, total, pagina, tamañoPagina);
            return View(modelo);
        }

        [Authorize(Roles = "Administrador,Empleado")]

        public async Task<IActionResult> Todos(int pagina = 1, int tamañoPagina = 5)
        {
            var total = await _context.Inmuebles.CountAsync();

            var items = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .OrderBy(i => i.Direccion)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            var modelo = new Paginador<Inmueble>(items, total, pagina, tamañoPagina);
            return View("Index", modelo);
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
        public IActionResult Create(string nombrePropietario="")
        {
            var propietarios = _context.Propietarios.AsQueryable();
            if (!string.IsNullOrEmpty(nombrePropietario))
            {
                propietarios = propietarios.Where(p => p.Nombre.Contains(nombrePropietario) || p.Apellido.Contains(nombrePropietario));
            }

            ViewBag.NombreBuscado = nombrePropietario;
            ViewBag.Propietarios = propietarios.ToList();
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
        public async Task<IActionResult> Create([Bind("Id,Direccion,Uso,TipoInmuebleId,Ambientes,Latitud,Longitud,Precio,Estado,PropietarioId")] Inmueble inmueble,string nombrePropietario="")
        {
            if (ModelState.IsValid)
            {
                inmueble.Estado = true;
                _context.Add(inmueble);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var propietarios = _context.Propietarios.AsQueryable();
            if (!string.IsNullOrEmpty(nombrePropietario))
            {
                propietarios = propietarios.Where(p => p.Nombre.Contains(nombrePropietario) || p.Apellido.Contains(nombrePropietario));
            }

            ViewBag.NombreBuscado = nombrePropietario;
            ViewBag.Propietarios = propietarios.ToList();
            ViewData["TipoInmuebleId"] = new SelectList(
                _context.TipoInmueble
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Descripcion
                    }).ToList(),
                "Value", "Text"
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
                _context.Propietarios
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Nombre + " " + p.Apellido
                    }).ToList(),
                "Value", "Text", inmueble.PropietarioId
            );

            ViewData["TipoInmuebleId"] = new SelectList(
                _context.TipoInmueble
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Descripcion
                    }).ToList(),
                "Value", "Text", inmueble.TipoInmuebleId
            );

            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Direccion,Uso,TipoInmuebleId,Ambientes,Latitud,Longitud,Precio,Estado,PropietarioId")] Inmueble inmueble)
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
                _context.Propietarios
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Nombre + " " + p.Apellido
                    }).ToList(),
                "Value", "Text", inmueble.PropietarioId
            );

            ViewData["TipoInmuebleId"] = new SelectList(
                _context.TipoInmueble
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Descripcion
                    }).ToList(),
                "Value", "Text", inmueble.TipoInmuebleId
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

        public IActionResult InmueblesDisponibles(DateTime fechaInicio, DateTime fechaFin)
        {
            var disponibles = _context.Inmuebles
                .Where(i => !_context.Contratos.Any(c =>
                    c.InmuebleId == i.Id &&
                    c.FechaInicio <= fechaFin &&
                    c.FechaFin >= fechaInicio))
                .ToList();

            return View(disponibles);
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
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                inmueble.Estado = false; // Marcamos como inactivo
                _context.Update(inmueble); // No lo eliminamos físicamente
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        private bool InmuebleExists(int id)
        {
            return _context.Inmuebles.Any(e => e.Id == id);
        }
    }
}