using System.Linq;
using Pedidos.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Entity;

namespace Pedidos.DAL
{
    public interface IArchivoEnDiscoRepository : IGenericRepository<ArchivoEnDisco>
    {
        ArchivoEnDisco ObtenerPorId(int id);
        Task<ArchivoEnDisco> ObtenerPorIdAsync(int id);
        IEnumerable<ArchivoEnDisco> ObtenerDesdeVector(int[] id);
        Task<IEnumerable<ArchivoEnDisco>> ObtenerDesdeVectorAsync(int[] id);
    }

    public class ArchivoEnDiscoRepository : GenericRepository<ArchivoEnDisco>, IArchivoEnDiscoRepository
    {

        public ArchivoEnDiscoRepository(PedidosDbContext context) : base(context) { }

        private IQueryable<ArchivoEnDisco> PlantillaBase
        {
            get
            {
                return context.ArchivosEnDisco;
            }
        }

        public virtual ArchivoEnDisco ObtenerPorId(int id)
        {
            int[] ids = new int[1] { id };
            return this.ObtenerDesdeVector(ids).FirstOrDefault();
        }

        public virtual async Task<ArchivoEnDisco> ObtenerPorIdAsync(int id)
        {
            int[] ids = new int[1] { id };
            return (await this.ObtenerDesdeVectorAsync(ids)).FirstOrDefault();
        }

        public virtual IEnumerable<ArchivoEnDisco> ObtenerDesdeVector(int[] id)
        {

            // Si el vector contiene datos,
            if (id != null && id.Length > 0)
            {
                return this.PlantillaBase
                    .Where(a => id.Contains(a.ArchivoEnDiscoId))
                    .ToList();
            }

            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }

        public virtual async Task<IEnumerable<ArchivoEnDisco>> ObtenerDesdeVectorAsync(int[] id)
        {

            // Si el vector contiene datos,
            if (id != null && id.Length > 0)
            {
                return (await this.PlantillaBase
                    .Where(a => id.Contains(a.ArchivoEnDiscoId))
                    .ToListAsync());
            }

            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }

    }

}