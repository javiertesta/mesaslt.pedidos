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
    public interface IGestionRepository : IGenericRepository<Gestion>
    {
        Gestion ObtenerPorId(int gestionId);
        Task<Gestion> ObtenerPorIdAsync(int gestionId);
        IEnumerable<Gestion> ObtenerDesdeVector(int[] gestionId);
        Task<IEnumerable<Gestion>> ObtenerDesdeVectorAsync(int[] gestionId);
        IPagedList<Gestion> ListarTodo(string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
        IPagedList<Gestion> ListarPorCliente(string clienteId, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
        IPagedList<Gestion> ListarHistorial(int gestionId, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
    }

    public class GestionRepository : GenericRepository<Gestion>, IGestionRepository
    {
        public GestionRepository(PedidosDbContext context) : base(context) { }

        private IQueryable<Gestion> PlantillaBase
        {
            get
            {
                return context.Gestiones
                    .Include(g => g.Cliente)
                    .Where(g => g.FechaBaja == null);
            }
        }

        public virtual Gestion ObtenerPorId(int gestionId)
        {
            int[] GestionesId = new int[1] { gestionId };
            return this.ObtenerDesdeVector(GestionesId).FirstOrDefault();
        }

        public virtual async Task<Gestion> ObtenerPorIdAsync(int gestionId)
        {
            int[] GestionesId = new int[1] { gestionId };
            return (await this.ObtenerDesdeVectorAsync(GestionesId)).FirstOrDefault();
        }

        public virtual IEnumerable<Gestion> ObtenerDesdeVector(int[] gestionId)
        {
            
            // Si el vector contiene datos,
            if (gestionId != null && gestionId.Length > 0)
            {
                return this.PlantillaBase
                    .Where(g => gestionId.Contains(g.GestionId))
                    .ToList();
            }
            
            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }

        public virtual async Task<IEnumerable<Gestion>> ObtenerDesdeVectorAsync(int[] gestionId)
        {

            // Si el vector contiene datos,
            if (gestionId != null && gestionId.Length > 0)
            {
                return (await this.PlantillaBase
                    .Where(g => gestionId.Contains(g.GestionId))
                    .ToListAsync());
            }

            // Si el vector estaba vacío,
            else
            {
                return null;
            }

        }
        
        /// <summary>
        /// Lista todas las gestiones de un cliente determinado que cumplen con los criterios indicados.
        /// </summary>
        /// <param name="clienteId">Código del cliente que se desea listar.</param>
        /// <param name="sort">Nombre del ordenamiento elegido.</param>
        /// <param name="search">Valor a buscar dentro del/los registro/s.</param>
        /// <param name="searchIn">Indica dentro de qué registro buscar.</param>
        /// <param name="page">Número de página a recuperar.</param>
        /// <param name="pageSize">Cantidad de registros por página.</param>
        /// <param name="userLanguage">Lenguaje de preferencia del usuario, obtenido del objeto Request.</param>
        /// <returns>Devuelve el listado de gestiones solicitado.</returns>
        public virtual IPagedList<Gestion> ListarPorCliente(string clienteId, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = this.PlantillaBase
                .Where(g => g.ClienteId == clienteId);
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        /// <summary>
        /// Lista el historial de una gestión, con los registros que además cumplen con los criterios indicados.
        /// </summary>
        /// <param name="clienteId">Código de la gestión que se desea listar.</param>
        /// <param name="sort">Nombre del ordenamiento elegido.</param>
        /// <param name="search">Valor a buscar dentro del/los registro/s.</param>
        /// <param name="searchIn">Indica dentro de qué registro buscar.</param>
        /// <param name="page">Número de página a recuperar.</param>
        /// <param name="pageSize">Cantidad de registros por página.</param>
        /// <param name="userLanguage">Lenguaje de preferencia del usuario, obtenido del objeto Request.</param>
        /// <returns>Devuelve el historial de la gestión solicitado.</returns>
        public virtual IPagedList<Gestion> ListarHistorial(int gestionId, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = context.Gestiones
                    .Include(g => g.Cliente)
                    .Where(g => g.RegistroOriginalId == gestionId || g.GestionId == gestionId);
            
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        /// <summary>
        /// Lista todas las gestiones que cumplen con los criterios indicados.
        /// </summary>
        /// <param name="sort">Nombre del ordenamiento elegido.</param>
        /// <param name="search">Valor a buscar dentro del/los registro/s.</param>
        /// <param name="searchIn">Indica dentro de qué registro buscar.</param>
        /// <param name="page">Número de página a recuperar.</param>
        /// <param name="pageSize">Cantidad de registros por página.</param>
        /// <param name="userLanguage">Lenguaje de preferencia del usuario, obtenido del objeto Request.</param>
        /// <returns>Devuelve el listado de gestiones solicitado.</returns>
        public virtual IPagedList<Gestion> ListarTodo(string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = this.PlantillaBase;
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        /// <summary>
        /// Termina el procesamiento del listado de gestiones en base a los datos pasados como argumentos.
        /// </summary>
        /// <param name="fromDb">Conjunto previamente seleccionados de gestiones.</param>
        /// <param name="sort">Nombre del ordenamiento elegido.</param>
        /// <param name="search">Valor a buscar dentro del/los registro/s.</param>
        /// <param name="searchIn">Indica dentro de qué registro buscar.</param>
        /// <param name="page">Número de página a recuperar.</param>
        /// <param name="pageSize">Cantidad de registros por página.</param>
        /// <param name="userLanguage">Lenguaje de preferencia del usuario, obtenido del objeto Request.</param>
        /// <returns>Devuelve el listado de gestiones solicitado.</returns>
        private IPagedList<Gestion> ProcesaListado(IQueryable<Gestion> fromDb, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            // Se extrae los datos culturales preferidos del cliente.
            var clientCulture = new CultureInfo(userLanguage);

            #region Filtrado de Datos

            // Si se pasaron datos de filtrado...
            if (!String.IsNullOrEmpty(search))
            {
                // Intenta convertir la cadena de texto de búsqueda en un número.
                double searchNumber = 0;
                double.TryParse(search, out searchNumber);

                // Intenta convertir la cadena de texto de búsqueda en una fecha.
                DateTime searchDate = DateTime.Now;
                DateTime.TryParseExact(search, clientCulture.DateTimeFormat.ShortDatePattern, clientCulture, DateTimeStyles.None, out searchDate);

                // Busca de acuerdo al valor en serachIn.
                switch (searchIn)
                {

                    case "codigo":
                        fromDb = fromDb.Where(c => c.GestionId == searchNumber);
                        break;

                    case "cliente":
                        fromDb = fromDb.Where(c => c.ClienteId.ToLower().Contains(search));
                        break;

                    case "fecha":
                        fromDb = fromDb.Where(c => c.FechaGestion == searchDate);
                        break;

                    case "usuario":
                        fromDb = fromDb.Where(c => c.UserName.ToLower().Contains(search));
                        break;

                    case "observaciones":
                        fromDb = fromDb.Where(c => c.Observaciones.ToLower().Contains(search));
                        break;

                    default:
                        fromDb = fromDb.Where(c => c.GestionId == searchNumber
                                                   || c.FechaGestion == searchDate
                                                   || c.UserName.ToLower().Contains(search)
                                                   || c.Observaciones.ToLower().Contains(search)
                                                   || c.ClienteId.ToLower().Contains(search));
                        break;

                }

            }

            #endregion

            #region Ordenamiento de Datos

            // Ordenación de datos
            switch (sort)
            {
                case "gestionid_desc":
                    fromDb = fromDb.OrderByDescending(c => c.GestionId);
                    break;
                case "fechagestion_desc":
                    fromDb = fromDb.OrderByDescending(c => c.FechaGestion);
                    break;
                case "username_desc":
                    fromDb = fromDb.OrderByDescending(c => c.UserName);
                    break;
                case "observaciones_desc":
                    fromDb = fromDb.OrderByDescending(c => c.Observaciones);
                    break;
                case "clienteid_desc":
                    fromDb = fromDb.OrderByDescending(c => c.ClienteId);
                    break;
                case "gestionid":
                    fromDb = fromDb.OrderBy(c => c.GestionId);
                    break;
                case "username":
                    fromDb = fromDb.OrderBy(c => c.UserName);
                    break;
                case "observaciones":
                    fromDb = fromDb.OrderBy(c => c.Observaciones);
                    break;
                case "clienteid":
                    fromDb = fromDb.OrderBy(c => c.ClienteId);
                    break;
                default:
                    fromDb = fromDb.OrderBy(c => c.FechaGestion);
                    break;
            }

            #endregion

            // Pagina los resultados.
            pageSize = pageSize ?? 15;
            page = page ?? 1;
            return fromDb.ToPagedList(page.Value, pageSize.Value);

        }
    }
}