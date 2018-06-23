using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Pedidos.Models;
using System.Threading.Tasks;

namespace Pedidos.DAL
{
    public interface IMuestrarioRepository : IGenericRepository<Muestrario>
    {
        Muestrario ObtenerPorId(int muestrarioId);
        Task<Muestrario> ObtenerPorIdAsync(int muestrarioId);
        IEnumerable<Muestrario> ObtenerDesdeVector(int[] muestrarioId);
        Task<IEnumerable<Muestrario>> ObtenerDesdeVectorAsync(int[] muestrarioId);
        IEnumerable<Muestrario> ObtenerTodos();
        Task<IEnumerable<Muestrario>> ObtenerTodosAsync();
    }

    public class MuestrarioRepository : GenericRepository<Muestrario>, IMuestrarioRepository
    {
        public MuestrarioRepository(PedidosDbContext context) : base(context) { }

        private IQueryable<Muestrario> PlantillaBase
        {
            get
            {
                return context.Muestrarios
                    .Include(m => m.Cliente);
            }
        }

        public virtual Muestrario ObtenerPorId(int muestrarioId)
        {
            int[] muestrariosId = new int[1] { muestrarioId };
            return this.ObtenerDesdeVector(muestrariosId).FirstOrDefault();
        }

        public virtual async Task<Muestrario> ObtenerPorIdAsync(int muestrarioId)
        {
            int[] muestrariosId = new int[1] { muestrarioId };
            return (await this.ObtenerDesdeVectorAsync(muestrariosId)).FirstOrDefault();
        }

        public virtual IEnumerable<Muestrario> ObtenerDesdeVector(int[] muestrariosId)
        {
            
            // Si el vector contiene datos,
            if (muestrariosId != null && muestrariosId.Length > 0)
            {
                return this.PlantillaBase
                    .Where(m => muestrariosId.Contains(m.MuestrarioId))
                    .ToList();
            }
            
            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }

        public virtual async Task<IEnumerable<Muestrario>> ObtenerDesdeVectorAsync(int[] muestrariosId)
        {

            // Si el vector contiene datos,
            if (muestrariosId != null && muestrariosId.Length > 0)
            {
                return (await this.PlantillaBase
                    .Where(m => muestrariosId.Contains(m.MuestrarioId))
                    .ToListAsync());
            }

            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }

        public virtual IEnumerable<Muestrario> ObtenerTodos()
        {
            return this.PlantillaBase.ToList();
        }

        public virtual async Task<IEnumerable<Muestrario>> ObtenerTodosAsync()
        {
            return await this.PlantillaBase.ToListAsync();
        }

    }
}