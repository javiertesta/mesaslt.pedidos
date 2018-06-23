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
    public interface IEtapasDelNegocioInternasRepository : IGenericRepository<EtapaDelNegocioInterna>
    {
        EtapaDelNegocioInterna ObtenerPorId(int etapaDelNegocioInternaId);
        Task<EtapaDelNegocioInterna> ObtenerPorIdAsync(int etapaDelNegocioInternaId);
        EtapaDelNegocioInterna ObtenerPorNivel(int nivel);
        Task<EtapaDelNegocioInterna> ObtenerPorNivelAsync(int nivel);
    }

    public class EtapaDelNegocioInternaRepository : GenericRepository<EtapaDelNegocioInterna>, IEtapasDelNegocioInternasRepository
    {
        public EtapaDelNegocioInternaRepository(PedidosDbContext context) : base(context) { }

        private IQueryable<EtapaDelNegocioInterna> PlantillaBase
        {
            get
            {
                return context.EtapasDelNegocioInternas
                    .Include(eni => eni.EtapaDelNegocioPublica);
            }
        }

        public virtual EtapaDelNegocioInterna ObtenerPorId(int etapaDelNegocioInternaId)
        {
            return this.PlantillaBase
                .Where(eni => eni.EtapaDelNegocioInternaId == etapaDelNegocioInternaId)
                .FirstOrDefault();
        }

        public virtual async Task<EtapaDelNegocioInterna> ObtenerPorIdAsync(int etapaDelNegocioInternaId)
        {
            return await this.PlantillaBase
                .Where(eni => eni.EtapaDelNegocioInternaId == etapaDelNegocioInternaId)
                .FirstOrDefaultAsync();
        }

        public virtual EtapaDelNegocioInterna ObtenerPorNivel(int nivel)
        {
            return this.PlantillaBase
                .Where(eni => eni.Nivel == nivel)
                .FirstOrDefault();
        }

        public virtual async Task<EtapaDelNegocioInterna> ObtenerPorNivelAsync(int nivel)
        {
            return await this.PlantillaBase
                .Where(eni => eni.Nivel == nivel)
                .FirstOrDefaultAsync();
        }
    }
}