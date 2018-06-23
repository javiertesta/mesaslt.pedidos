using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using Pedidos.Models;
using Pedidos.DAL;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Globalization;
using PagedList;

namespace Pedidos.DAL
{

    public interface IPedidosListadosRepository : IGenericRepository<PedidoListado>
    {
        PedidoListado ObtenerPorId(int id);
        Task<PedidoListado> ObtenerPorIdAsync(int id);
        PedidoListado ObtenerPorFecha(DateTime fecha);
        Task<PedidoListado> ObtenerPorFechaAsync(DateTime fecha);
        IEnumerable<PedidoListado> ObtenerTodos();
        Task<IEnumerable<PedidoListado>> ObtenerTodosAsync();
    }

    public class PedidosListadosRepository : GenericRepository<PedidoListado>, IPedidosListadosRepository
    {

        public PedidosListadosRepository(PedidosDbContext context) : base(context) { }

        private DbSet<PedidoListado> PlantillaBase
        {
            get
            {
                return context.PedidosListados;
            }
        }

        public virtual PedidoListado ObtenerPorFecha(DateTime fecha)
        {
            return this.PlantillaBase
                .Where(pl => pl.Fecha == fecha)
                .FirstOrDefault();
        }

        public virtual async Task<PedidoListado> ObtenerPorFechaAsync(DateTime fecha)
        {
            return await this.PlantillaBase
                .Where(pl => pl.Fecha == fecha)
                .FirstOrDefaultAsync();
        }

        public virtual PedidoListado ObtenerPorId(int id)
        {
            return this.PlantillaBase.Find(id);
        }

        public virtual async Task<PedidoListado> ObtenerPorIdAsync(int id)
        {
            return await this.PlantillaBase.FindAsync(id);
        }

        public virtual IEnumerable<PedidoListado> ObtenerTodos()
        {
            return this.PlantillaBase.ToList();
        }

        public virtual async Task<IEnumerable<PedidoListado>> ObtenerTodosAsync()
        {
            return await this.PlantillaBase.ToListAsync();
        }

        private void EliminarCaducos()
        {
            this.PlantillaBase.RemoveRange(context.PedidosListados
                .Where(le => le.Fecha < DateTime.Now.AddDays(-15)));
        }

    }

}