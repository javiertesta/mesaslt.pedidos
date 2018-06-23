using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Pedidos.Models;
using System.Threading.Tasks;
using LinqKit;

namespace Pedidos.DAL
{
    public interface ILaminadoRepository : IGenericRepository<Laminado>
    {
        Laminado ObtenerPorId(Tuple<int, string> laminadoId);
        Task<Laminado> ObtenerPorIdAsync(Tuple<int, string> laminadoId);
        IEnumerable<Laminado> ObtenerDesdeVector(Tuple<int, string>[] laminadosId);
        Task<IEnumerable<Laminado>> ObtenerDesdeVectorAsync(Tuple<int, string>[] laminadosId);
        IEnumerable<Laminado> ObtenerTodos();
        Task<IEnumerable<Laminado>> ObtenerTodosAsync();
    }

    public class LaminadoRepository : GenericRepository<Laminado>, ILaminadoRepository
    {
        public LaminadoRepository(PedidosDbContext context) : base(context) { }

        private IQueryable<Laminado> PlantillaBase
        {
            get
            {
                return context.Laminados
                    .Include(l => l.Muestrario);
            }
        }

        public virtual Laminado ObtenerPorId(Tuple<int, string> laminadoId)
        {
            Tuple<int, string>[] laminadosId = new Tuple<int, string>[1] { laminadoId };
            return this.ObtenerDesdeVector(laminadosId).FirstOrDefault();
        }

        public virtual async Task<Laminado> ObtenerPorIdAsync(Tuple<int, string> laminadoId)
        {
            Tuple<int, string>[] laminadosId = new Tuple<int, string>[1] { laminadoId };
            return (await this.ObtenerDesdeVectorAsync(laminadosId)).FirstOrDefault();
        }

        public virtual IEnumerable<Laminado> ObtenerDesdeVector(Tuple<int, string>[] laminadosId)
        {
            
            // Si el vector contiene datos,
            if (laminadosId != null && laminadosId.Length > 0)
            {
                
                var predicate = PredicateBuilder.New<Laminado>();
                foreach (var laminadoId in laminadosId)
                {
                    predicate = predicate.Or(l => l.Laminado_MuestrarioId == laminadoId.Item1 && l.Laminado_CodigoId == laminadoId.Item2);
                }
                
                return this.PlantillaBase.AsExpandable()
                    .Where(predicate)
                    .ToList();
            }
            
            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }

        public virtual async Task<IEnumerable<Laminado>> ObtenerDesdeVectorAsync(Tuple<int, string>[] laminadosId)
        {

            // Si el vector contiene datos,
            if (laminadosId != null && laminadosId.Length > 0)
            {

                var predicate = PredicateBuilder.New<Laminado>();
                foreach (var laminadoId in laminadosId)
                {
                    predicate = predicate.Or(l => l.Laminado_MuestrarioId == laminadoId.Item1 && l.Laminado_CodigoId == laminadoId.Item2);
                }

                return (await this.PlantillaBase.AsExpandable()
                    .Where(predicate)
                    .ToListAsync());
            }

            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }

        public virtual IEnumerable<Laminado> ObtenerTodos()
        {
            return this.PlantillaBase.ToList();
        }

        public virtual async Task<IEnumerable<Laminado>> ObtenerTodosAsync()
        {
            return await this.PlantillaBase.ToListAsync();
        }

    }
}