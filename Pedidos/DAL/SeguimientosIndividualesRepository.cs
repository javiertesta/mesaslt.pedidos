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
    public interface ISeguimientoIndividualRepository : IGenericRepository<SeguimientoIndividual>
    {
        bool IncluyeDadosDeBaja { get; set; }
        IEnumerable<SeguimientoIndividual> ObtenerDesdePedido(int pedidoId);
        Task<IEnumerable<SeguimientoIndividual>> ObtenerDesdePedidoAsync(int pedidoId);
        IEnumerable<SeguimientoIndividual> ObtenerDesdeVectorDePedidos(int[] pedidoId);
        Task<IEnumerable<SeguimientoIndividual>> ObtenerDesdeVectorDePedidosAsync(int[] pedidoId);
    }

    public class SeguimientoIndividualRepository : GenericRepository<SeguimientoIndividual>, ISeguimientoIndividualRepository
    {
        public SeguimientoIndividualRepository(PedidosDbContext context) : base(context) { }

        public bool IncluyeDadosDeBaja { get; set; }
        
        private IQueryable<SeguimientoIndividual> PlantillaBase
        {
            get
            {
                IQueryable<SeguimientoIndividual> linq = context.SeguimientosIndividuales
                    .Include(si => si.SeguimientoGlobal)
                    .Include(si => si.SeguimientoGlobal.Pedido)
                    .Include(si => si.EtapaDelNegocioInterna)
                    .Include(si => si.EtapaDelNegocioInterna.EtapaDelNegocioPublica);
                if (!this.IncluyeDadosDeBaja) linq = linq.Where(si => si.FechaBaja == null);
                return linq;
                    
            }
        }

        public virtual IEnumerable<SeguimientoIndividual> ObtenerDesdeVectorDePedidos(int[] pedidoId)
        {
            return this.PlantillaBase
                .Where(si => pedidoId.Contains(si.SeguimientoGlobal.Pedido.PedidoId) && si.FechaBaja == null)
                .ToList();
        }

        public virtual async Task<IEnumerable<SeguimientoIndividual>> ObtenerDesdeVectorDePedidosAsync(int[] pedidoId)
        {
            return await this.PlantillaBase
                .Where(si => pedidoId.Contains(si.SeguimientoGlobal.Pedido.PedidoId) && si.FechaBaja == null)
                .ToListAsync();
        }

        public virtual IEnumerable<SeguimientoIndividual> ObtenerDesdePedido(int pedidoId)
        {
            return this.PlantillaBase
                .Where(si => si.SeguimientoGlobal.Pedido.PedidoId == pedidoId && si.FechaBaja == null)
                .ToList();
        }

        public virtual async Task<IEnumerable<SeguimientoIndividual>> ObtenerDesdePedidoAsync(int pedidoId)
        {
            return await this.PlantillaBase
                .Where(si => si.SeguimientoGlobal.Pedido.PedidoId == pedidoId && si.FechaBaja == null)
                .ToListAsync();
        }

        public virtual IEnumerable<SeguimientoIndividual> ObtenerDesdeEtapa(int etapaDelNegocioInternaId)
        {
            return this.PlantillaBase
                .Where(si => si.EtapaDelNegocioInternaId == etapaDelNegocioInternaId)
                .ToList();
        }

        public virtual async Task<IEnumerable<SeguimientoIndividual>> ObtenerDesdeEtapaAsync(int etapaDelNegocioInternaId)
        {
            return await this.PlantillaBase
                .Where(si => si.EtapaDelNegocioInternaId == etapaDelNegocioInternaId)
                .ToListAsync();
        }
    }
}