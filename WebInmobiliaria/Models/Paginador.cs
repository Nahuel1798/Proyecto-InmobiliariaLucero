using System;
using System.Collections.Generic;

public class Paginador<T>
{
    public IEnumerable<T> Items { get; set; }
    public int PaginaActual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItems { get; set; }
    public int TamañoPagina { get; set; }

    public bool HayAnterior => PaginaActual > 1;
    public bool HaySiguiente => PaginaActual < TotalPaginas;

    public Paginador(IEnumerable<T> items, int totalItems, int paginaActual, int tamañoPagina)
    {
        Items = items;
        TotalItems = totalItems;
        PaginaActual = paginaActual;
        TamañoPagina = tamañoPagina;
        TotalPaginas = (int)Math.Ceiling(totalItems / (double)tamañoPagina);
    }
}
