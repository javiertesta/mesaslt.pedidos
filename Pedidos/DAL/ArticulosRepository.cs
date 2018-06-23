using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using Pedidos.Models;
using Pedidos.DAL;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pedidos.DAL
{
    public interface IArticuloRepository : IGenericRepository<Articulo>
    {

        Articulo ObtenerPorCodigoTango(string codigoTango);
        Task<Articulo> ObtenerPorCodigoTangoAsync(string codigoTango);

    }

    public class ArticuloRepository : GenericRepository<Articulo>, IArticuloRepository
    {
        
        public ArticuloRepository(PedidosDbContext context) : base(context) { }

        public Articulo ObtenerPorCodigoTango(string codigoTango)
        {
            return context.Articulos
                .Where(a => a.CodigoTango == codigoTango).FirstOrDefault();
        }

        public async Task<Articulo> ObtenerPorCodigoTangoAsync(string codigoTango)
        {
            return await context.Articulos
                .Where(a => a.CodigoTango == codigoTango).FirstOrDefaultAsync();
        }

    }
}