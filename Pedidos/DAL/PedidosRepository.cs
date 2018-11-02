using CustomExtensions;
using PagedList;
using Pedidos.Models;
using Pedidos.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Pedidos.DAL
{

    public interface IPedidosRepository : IGenericRepository<Pedido>
    {
        
        bool IncluyeHistorial { get;  set; }
        bool IncluyeGestion { get; set; }
        bool IncluyeCliente { get; set; }
        bool IncluyeAdjuntos { get; set; }
        bool IncluyeSeguimientoGlobal { get; set; }
        bool IncluyeDadosDeBaja { get; set; }
        Pedido ObtenerPorId(int pedidoId);
        Task<Pedido> ObtenerPorIdAsync(int pedidoId);
        IEnumerable<Pedido> ObtenerDesdeVector(int[] pedidoId);
        Task<IEnumerable<Pedido>> ObtenerDesdeVectorAsync(int[] pedidoId);
        IEnumerable<Pedido> ObtenerDesdeGestion(int gestionId);
        Task<IEnumerable<Pedido>> ObtenerDesdeGestionAsync(int gestionId);
        IEnumerable<Pedido> ObtenerDesdeCliente(string clienteId);
        Task<IEnumerable<Pedido>> ObtenerDesdeClienteAsync(string clienteId);
        IPagedList<Pedido> ListarPorGestion(int id, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
        IPagedList<Pedido> ListarPorZona(Zonas id, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
        IPagedList<Pedido> ListarPorCliente(string id, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
        List<Pedido> ListarHistorial(int id, string userLanguage);
        IPagedList<Pedido> ListarTodo(string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
        IPagedList<Pedido> ListarDemorados(string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage);
        List<Pedido> InformePedidosParaCorteDeLaminado();
        List<Pedido> InformePedidosActivosPorZona(Pedidos.Models.Enums.Zonas id);
        List<Pedido> InformePedidosActivosPorCliente(string id);

    }

    public class PedidoRepository : GenericRepository<Pedido>, IPedidosRepository
    {
        
        bool _incluyeGestion;
        bool _incluyeCliente;

        public PedidoRepository(PedidosDbContext context)
            : base(context)
        {
            this.Reset();
        }

        public void Reset()
        {
            this.IncluyeCliente = true;
            this.IncluyeDadosDeBaja = false;
            this.IncluyeGestion = true;
            this.IncluyeHistorial = false;
            this.IncluyeSeguimientoGlobal = true;
        }

        public bool IncluyeHistorial { get; set; }

        public bool IncluyeGestion
        {
            get { return _incluyeGestion; }
            set
            {
                if (value == false) _incluyeCliente = false;
                _incluyeGestion = value;
            }
        }

        public bool IncluyeCliente
        {
            get { return _incluyeCliente; }
            set
            {
                if (value == true) _incluyeGestion = true;
                _incluyeCliente = value;
            }
        }

        public bool IncluyeAdjuntos { get; set; }

        public bool IncluyeSeguimientoGlobal { get; set; }

        public bool IncluyeDadosDeBaja { get; set; }

        private IQueryable<Pedido> PlantillaBase
        {
            get
            {
                IQueryable<Pedido> linq = context.Pedidos.Include(p => p.Articulo);
                if (this.IncluyeGestion) linq = linq.Include(p => p.Gestion);
                if (this.IncluyeCliente) linq = linq.Include(p => p.Gestion.Cliente);
                if (this.IncluyeAdjuntos) linq = linq.Include(p => p.ArchivosAdjuntos);
                if (this.IncluyeSeguimientoGlobal) linq = linq.Include(p => p.SeguimientoGlobal);
                if (this.IncluyeSeguimientoGlobal) linq = linq.Include(p => p.SeguimientoGlobal.Detalle);
                if (!this.IncluyeDadosDeBaja) linq = linq.Where(p => p.FechaBaja == null);
                if (this.IncluyeHistorial) linq = linq.Include(p => p.Historial);
                return linq;
            }
        }

        public virtual Pedido ObtenerPorId(int pedidoId)
        {
            return this.PlantillaBase
                .Where(p => p.PedidoId == pedidoId)
                .FirstOrDefault();
        }

        public virtual async Task<Pedido> ObtenerPorIdAsync(int pedidoId)
        {
            return await this.PlantillaBase
                .Where(p => p.PedidoId == pedidoId)
                .FirstOrDefaultAsync();
        }

        public virtual IEnumerable<Pedido> ObtenerDesdeVector(int[] pedidoId)
        {
            return this.PlantillaBase
                .Where(p => pedidoId.Contains(p.PedidoId))
                .ToList();
        }

        public virtual async Task<IEnumerable<Pedido>> ObtenerDesdeVectorAsync(int[] pedidoId)
        {
            return await this.PlantillaBase
                .Where(p => pedidoId.Contains(p.PedidoId))
                .ToListAsync();
        }

        public virtual IEnumerable<Pedido> ObtenerDesdeGestion(int gestionId)
        {
            return this.PlantillaBase
                .Where(p => p.GestionId == gestionId)
                .ToList();
        }

        public virtual async Task<IEnumerable<Pedido>> ObtenerDesdeGestionAsync(int gestionId)
        {
            return await this.PlantillaBase
                .Where(p => p.GestionId == gestionId)
                .ToListAsync();
        }
        
        public IEnumerable<Pedido> ObtenerDesdeCliente(string clienteId)
        {
            return this.PlantillaBase
                .Where(p => p.Gestion.ClienteId == clienteId)
                .ToList();
        }

        public async Task<IEnumerable<Pedido>> ObtenerDesdeClienteAsync(string clienteId)
        {
            return await this.PlantillaBase
                .Where(p => p.Gestion.ClienteId == clienteId)
                .ToListAsync();
        }

        public IPagedList<Pedido> ListarPorGestion(int id, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = this.PlantillaBase
                .Where(p => p.GestionId == id);
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        public IPagedList<Pedido> ListarPorZona(Zonas id, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = this.PlantillaBase
                .Where(p => p.Gestion.Cliente.Zona == id);
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        public IPagedList<Pedido> ListarPorCliente(string id, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = this.PlantillaBase
                .Where(p => p.Gestion.ClienteId == id);
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        public List<Pedido> ListarHistorial(int id, string userLanguage)
        {

            var fromDb = context.Pedidos
                .Include(p => p.Gestion)
                .Include(p => p.Gestion.Cliente)
                .Include(p => p.SeguimientoGlobal)
                .Include(p => p.SeguimientoGlobal.Detalle)
                .Include(p => p.Articulo)
                .Where(p => p.RegistroOriginalId == id || p.PedidoId == id)
                .OrderBy(p => p.PedidoId)
                .ToList();

            if (fromDb.Count > 1)
            {
                List<Pedido> ordenFinal = fromDb.Skip(1).ToList();
                ordenFinal.Add(fromDb.First());
                return ordenFinal;
            }

            else return fromDb;

        }

        public Dictionary<int, List<CambioDeSeguimiento>> ObtenerVariacionesDelSeguimiento(int[] pedidos)
        {

            var resultadoFinal = new Dictionary<int, List<CambioDeSeguimiento>>();
            if (pedidos == null || pedidos.Length == 0) return resultadoFinal;

            var fromDb = context.SeguimientosIndividuales
                .Where(si => pedidos.Contains(si.SeguimientoGlobalId))
                .OrderBy(si => si.Fecha).ThenBy(si => si.EtapaDelNegocioInternaId)
                .ToList();
            if (fromDb == null || fromDb.Count == 0) return resultadoFinal;

            foreach (var pedido in pedidos)
            {

                var seguimientoDelPedido = fromDb.Where(si => si.SeguimientoGlobalId == pedido);
                List<DateTime> listaDeFechas = seguimientoDelPedido.Select(si => si.Fecha).Distinct().ToList();
                var resultadoDelPedido = new List<CambioDeSeguimiento>();
                var seguimientoAcumulado = new Dictionary<int, int>();
                seguimientoDelPedido.Select(si => si.EtapaDelNegocioInternaId).Distinct().ToList().ForEach(etapa => seguimientoAcumulado.Add(etapa, 0));

                foreach (var fechaDeSeguimiento in listaDeFechas)
                {

                    var seguimientoDeLaFecha = seguimientoDelPedido.Where(si => si.Fecha == fechaDeSeguimiento);
                    CambioDeSeguimiento resultadoDeLaFecha = new CambioDeSeguimiento { Fecha = fechaDeSeguimiento };

                    int[] enis = seguimientoAcumulado.Keys.ToArray();
                    foreach (var eni in enis)
                    {
                        var cantidadActual = seguimientoDeLaFecha.Where(si => si.EtapaDelNegocioInternaId == eni).FirstOrDefault() != null ? seguimientoDeLaFecha.Where(si => si.EtapaDelNegocioInternaId == eni).FirstOrDefault().Cantidad : 0;
                        var variacion = cantidadActual - seguimientoAcumulado[eni];
                        if (variacion != 0) resultadoDeLaFecha.Cambios.Add(eni, variacion);
                        seguimientoAcumulado[eni] = cantidadActual;
                    }

                    if (resultadoDeLaFecha.Cambios.Count != 0) resultadoDelPedido.Add(resultadoDeLaFecha);

                }

                if (resultadoDelPedido.Count != 0) resultadoFinal.Add(pedido, resultadoDelPedido);

            }

            return resultadoFinal;

        }

        public Dictionary<int, List<CambioDeSeguimiento>> ObtenerSeguimientoCompleto(int[] pedidos)
        {

            var resultadoFinal = new Dictionary<int, List<CambioDeSeguimiento>>();
            if (pedidos == null || pedidos.Length == 0) return resultadoFinal;

            var fromDb = context.SeguimientosIndividuales
                .Where(si => pedidos.Contains(si.SeguimientoGlobalId))
                .OrderBy(si => si.Fecha).ThenBy(si => si.EtapaDelNegocioInternaId)
                .ToList();
            if (fromDb == null || fromDb.Count == 0) return resultadoFinal;

            foreach (var pedido in pedidos)
            {

                var seguimientoDelPedido = fromDb.Where(si => si.SeguimientoGlobalId == pedido);
                List<DateTime> listaDeFechas = seguimientoDelPedido.Select(si => si.Fecha).Distinct().ToList();
                var resultadoDelPedido = new List<CambioDeSeguimiento>();

                foreach (var fechaDeSeguimiento in listaDeFechas)
                {
                    var seguimientoDeLaFecha = seguimientoDelPedido.Where(si => si.Fecha == fechaDeSeguimiento);
                    CambioDeSeguimiento resultadoDeLaFecha = new CambioDeSeguimiento { Fecha = fechaDeSeguimiento };
                    foreach (var si in seguimientoDeLaFecha) resultadoDeLaFecha.Cambios.Add(si.EtapaDelNegocioInternaId, si.Cantidad);
                    if (resultadoDeLaFecha.Cambios.Count != 0) resultadoDelPedido.Add(resultadoDeLaFecha);
                }

                if (resultadoDelPedido.Count != 0) resultadoFinal.Add(pedido, resultadoDelPedido);

            }

            return resultadoFinal;

        }

        public IPagedList<Pedido> ListarTodo(string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = this.PlantillaBase;
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        public IPagedList<Pedido> ListarDadosDeBaja(string cliente, DateTime desdeFecha, DateTime hastaFecha, int? page, int? pageSize, string userLanguage)
        {

            var backupBaja = this.IncluyeDadosDeBaja;
            var backupHistorial = this.IncluyeHistorial;
            this.IncluyeDadosDeBaja = true;
            this.IncluyeHistorial = true;
            var fromDb = this.PlantillaBase
                .Where(p => p.FechaBaja >= desdeFecha && p.FechaBaja <= hastaFecha && p.RegistroOriginalId == null && p.Gestion.ClienteId == cliente);
            this.IncluyeDadosDeBaja = backupBaja;
            this.IncluyeHistorial = backupHistorial;

            return ProcesaListado(fromDb, "InformeDePedidosActivos", "", "", page, pageSize, userLanguage);

        }

        public IPagedList<Pedido> ListarDemorados(string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            var fromDb = this.PlantillaBase
                .Where(p => p.SeguimientoGlobal.Detalle.Where(sg => sg.FechaBaja == null && sg.EtapaDelNegocioInternaId == 2).Count() > 0);
            return ProcesaListado(fromDb, sort, search, searchIn, page, pageSize, userLanguage);

        }

        public List<Pedido> InformePedidosParaCorteDeLaminado()
        {

            var UOW = new UnitOfWork(context);
            int eni = UOW.EtapaDelNegocioInternaRepository.ObtenerPorNivel(0).EtapaDelNegocioInternaId;
            int[] pedidosCoincidentes = UOW.SeguimientoIndividualRepository.ObtenerDesdeEtapa(eni).Select(si => si.SeguimientoGlobalId).ToArray();

            var entidades = this.PlantillaBase
                .Where(p => pedidosCoincidentes.Contains(p.PedidoId))
                .Where(p => (p.Articulo as Tapa) != null)
                .ToList()
                .OrderBy(p => (p.Articulo as Tapa).Melamina)
                .ThenBy(p => (p.Articulo as Tapa).TipoNombre)
                .ThenBy(p => (p.Articulo as Tapa).Medida)
                .ThenBy(p => (p.Articulo as Tapa).Borde.TipoNombre)
                .ThenBy(p => (p.Articulo as Tapa).Borde.EspesorNombre)
                .ThenBy(p => (p.Articulo as Tapa).Laminado_CodigoId)
                .ToList();
            return entidades;

        }

        public List<Pedido> InformePedidosCabina()
        {

            var UOW = new UnitOfWork(context);

            var entidades = this.PlantillaBase
                .Where(p => (p.Articulo as Tapa) != null)
                .Where(p => (p.Articulo as Tapa).Borde.Tipo == TiposDeBordesDeTapas.BordeMDF || (p.Articulo as Tapa).Borde.Tipo == TiposDeBordesDeTapas.BordeMDFINV)
                //.ToList()
                .OrderByDescending(p => p.FechaEntrega)
                .ThenBy(p => p.Gestion.FechaGestion)
                .ToList();
            return entidades;

        }

        public List<Pedido> InformePedidosActivosPorZona(Pedidos.Models.Enums.Zonas id)
        {

            var entidades = this.PlantillaBase
                .Where(p => p.Gestion.Cliente.Zona == id)
                .OrderBy(p => p.Gestion.ClienteId)
                .ToList();
            return entidades;

        }

        public List<Pedido> InformePedidosActivosPorCliente(string id)
        {

            var entidades = this.PlantillaBase
                .Where(p => p.Gestion.ClienteId == id)
                .OrderBy(p => p.PedidoId)
                .ToList();
            return entidades;

        }

        /// <summary>
        /// Termina el procesamiento del listado de pedidos en base a los datos pasados como argumentos.
        /// </summary>
        /// <param name="fromDb">Conjunto previamente seleccionados de pedidos.</param>
        /// <param name="sort">Nombre del ordenamiento elegido.</param>
        /// <param name="search">Valor a buscar dentro del/los registro/s.</param>
        /// <param name="searchIn">Indica dentro de qué registro buscar.</param>
        /// <param name="page">Número de página a recuperar.</param>
        /// <param name="pageSize">Cantidad de registros por página.</param>
        /// <param name="userLanguage">Lenguaje de preferencia del usuario, obtenido del objeto Request.</param>
        /// <returns>Devuelve el listado de pedidos solicitado.</returns>
        private IPagedList<Pedido> ProcesaListado(IQueryable<Pedido> fromDb, string sort, string search, string searchIn, int? page, int? pageSize, string userLanguage)
        {

            // Se extrae los datos culturales preferidos del cliente.
            var clientCulture = new CultureInfo(userLanguage);

            #region Filtrado de Datos

            // Si se pasaron datos de filtrado...
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                double searchNumber = 0;
                double.TryParse(search, out searchNumber);

                DateTime searchDate = DateTime.Now;
                DateTime.TryParseExact(search, clientCulture.DateTimeFormat.ShortDatePattern, clientCulture, DateTimeStyles.None, out searchDate);
                if (searchDate < new DateTime(1901, 1, 1)) searchDate = new DateTime(1753, 1, 1);

                var filteredBASEspesores = EnumExtensionMethods.EnumDisplayNamesToDictionary<EspesoresDeBases>().FilterKeys<EspesoresDeBases, string>(search);
                var filteredBASColores = EnumExtensionMethods.EnumDisplayNamesToDictionary<ColoresDeBases>().FilterKeys<ColoresDeBases, string>(search);
                var filteredBASProveedores = EnumExtensionMethods.EnumDisplayNamesToDictionary<Proveedores>().FilterKeys<Proveedores, string>(search);
                var filteredTAPTipos = EnumExtensionMethods.EnumDisplayNamesToDictionary<TiposDeTapas>().FilterKeys<TiposDeTapas, string>(search);
                var filteredTAPBORTipos = EnumExtensionMethods.EnumDisplayNamesToDictionary<TiposDeBordesDeTapas>().FilterKeys<TiposDeBordesDeTapas, string>(search);
                var filteredTAPBOREspesores = EnumExtensionMethods.EnumDisplayNamesToDictionary<EspesoresDeBordesDeTapas>().FilterKeys<EspesoresDeBordesDeTapas, string>(search);
                var filteredTAPBORColores = EnumExtensionMethods.EnumDisplayNamesToDictionary<ColoresDeBordesDeTapas>().FilterKeys<ColoresDeBordesDeTapas, string>(search);

                // Generación de la cadena que indica el tipo de artículo, por si se requiere en la búsqueda.
                // El tipo en sí no encontré como almacenarlo en una variable, por lo que al analizar el tipo de artículo
                // tuve que hacer un cast por cada tipo (como puede verse más abajo)
                string _typestring = "";
                _typestring = (search == "tapa") ? _typestring = "tapa" : _typestring;
                _typestring = (search == "base") ? _typestring = "base" : _typestring;
                _typestring = (search == "vitrea") ? _typestring = "vitrea" : _typestring;

                switch (searchIn)
                {

                    // Objeto Pedido.
                    case "id":
                        fromDb = fromDb.Where(p => p.PedidoId == searchNumber);
                        break;
                    case "cantidad":
                        fromDb = fromDb.Where(p => p.Cantidad == searchNumber);
                        break;
                    case "fechaentrega":
                        fromDb = fromDb.Where(p => ((p.FechaEntrega != null) ? p.FechaEntrega == searchDate : false));
                        break;
                    case "referencia":
                        fromDb = fromDb.Where(p => ((p.Referencia != null) ? p.Referencia.ToLower().Contains(search) : false));
                        break;
                    case "fechabaja":
                        fromDb = fromDb.Where(p => ((p.FechaBaja != null) ? p.FechaBaja == searchDate : false));
                        break;
                    case "estructurasolicitada":
                        fromDb = fromDb.Where(p => ((p.EstructuraSolicitada != null) ? p.EstructuraSolicitada.Contains(search) : false));
                        break;
                    case "observaciones":
                        fromDb = fromDb.Where(p => ((p.Observaciones != null) ? p.Observaciones.Contains(search) : false));
                        break;

                    // Análisis del tipo de artículo
                    case "tipodearticulo":
                        fromDb = fromDb.Where(p =>
                               ((_typestring == "tapa") ? ((p.Articulo as Tapa) != null) : false)
                            || ((_typestring == "base") ? ((p.Articulo as Base) != null) : false)
                            || ((_typestring == "vitrea") ? ((p.Articulo as Vitrea) != null) : false)
                            );
                        break;

                    // Objeto Artículo.
                    case "particularidades":
                        fromDb = fromDb.Where(p => ((p.Articulo.Particularidades != null) ? p.Articulo.Particularidades.ToLower().Contains(search) : false));
                        break;

                    // Objeto Tapa.
                    case "tapatipo":
                        fromDb = fromDb.Where(p => (((p.Articulo as Tapa) != null) ? filteredTAPTipos.Contains((p.Articulo as Tapa).Tipo.Value) : false));
                        break;
                    case "tapamedida":
                        fromDb = fromDb.Where(p => (((p.Articulo as Tapa) != null) ? (p.Articulo as Tapa).Medida.ToLower().Contains(search) : false));
                        break;
                    case "tapamelamina":
                        fromDb = fromDb.Where(p => ((p.Articulo as Tapa) != null) ? (search.Contains("melamina") ? (p.Articulo as Tapa).Melamina ?? false : false) : false);
                        break;
                    case "tapalaminadocodigo":
                        fromDb = fromDb.Where(p =>

                                // Si es una tapa
                                ((p.Articulo as Tapa) != null) ?

                                    // Si es una tapa, verifica que su valor no sea nulo
                                    ((p.Articulo as Tapa).Laminado_CodigoId != null) ?

                                        // Si es una tapa y su valor no es nulo, procede a buscar en el campo
                                        ((p.Articulo as Tapa).Laminado_CodigoId ?? "").ToString().ToLower().Contains(search)

                                        // Si es una tapa pero el campo es nulo, devuelve falso
                                        : false

                                    // Si el campo no es una tapa, devuelve falso
                                    : false

                                );
                        break;
                    case "tapalaminadomuestrario":
                        fromDb = fromDb.Where(p =>

                            // Si es una tapa
                            ((p.Articulo as Tapa) != null) ?

                                // Si es una tapa, verifica que su valor no sea nulo
                                ((p.Articulo as Tapa).Laminado_MuestrarioId != null) ?

                                    // Si es una tapa y su valor no es nulo, procede a buscar en el campo
                                    ((p.Articulo as Tapa).Laminado_MuestrarioId ?? 0) == searchNumber

                                    // Si es una tapa pero el campo es nulo, devuelve falso
                                    : false

                                // Si el campo no es una tapa, devuelve falso
                                : false

                            );
                        break;
                    case "tapabordetipo":
                        fromDb = fromDb.Where(p => (((p.Articulo as Tapa) != null) ? filteredTAPBORTipos.Contains((p.Articulo as Tapa).Borde.Tipo) : false));
                        break;
                    case "tapabordecolor":
                        fromDb = fromDb.Where(p => (((p.Articulo as Tapa) != null) ? (((p.Articulo as Tapa).Borde.Color != null) ? filteredTAPBORColores.Contains((p.Articulo as Tapa).Borde.Color.Value) : false) : false));
                        break;
                    case "tapabordeespesor":
                        fromDb = fromDb.Where(p => (((p.Articulo as Tapa) != null) ? filteredTAPBOREspesores.Contains((p.Articulo as Tapa).Borde.Espesor) : false));
                        break;

                    // Objeto Base
                    case "basemodelo":
                        fromDb = fromDb.Where(p => (((p.Articulo as Base) != null) ? (p.Articulo as Base).Modelo.ToLower().Contains(search) : false));
                        break;
                    case "baseespesor":
                        fromDb = fromDb.Where(p => (((p.Articulo as Base) != null) ? filteredBASEspesores.Contains((p.Articulo as Base).Espesor) : false));
                        break;
                    case "basecolor":
                        fromDb = fromDb.Where(p => (((p.Articulo as Base) != null) ? (((p.Articulo as Base).Color != null) ? filteredBASColores.Contains((p.Articulo as Base).Color.Value) : false) : false));
                        break;
                    case "baseproveedor":
                        fromDb = fromDb.Where(p => (((p.Articulo as Base) != null) ? filteredBASProveedores.Contains((p.Articulo as Base).Proveedor) : false));
                        break;

                    // Objeto Vitrea.
                    case "vitreatipo":
                        fromDb = fromDb.Where(p => (((p.Articulo as Vitrea) != null) ? (p.Articulo as Vitrea).Tipo.ToLower().Contains(search) : false));
                        break;
                    case "vitreamedida":
                        fromDb = fromDb.Where(p => (((p.Articulo as Vitrea) != null) ? (p.Articulo as Vitrea).Medida.ToLower().Contains(search) : false));
                        break;
                    case "vitreatransparente":
                        fromDb = fromDb.Where(p => ((p.Articulo as Vitrea) != null) ? (search.Contains("transparente") ? (p.Articulo as Vitrea).Transparente ?? false : false) : false);
                        break;

                    // Objeto FueraDeLista.
                    case "fueradelistatitulo":
                        fromDb = fromDb.Where(p => (((p.Articulo as FueraDeLista) != null) ? (p.Articulo as FueraDeLista).Titulo.ToLower().Contains(search) : false));
                        break;
                    case "fueradelistadetalle":
                        fromDb = fromDb.Where(p => (((p.Articulo as FueraDeLista) != null) ? (p.Articulo as FueraDeLista).Detalle.ToLower().Contains(search) : false));
                        break;

                    // Busca en todos los campos.
                    default:
                        fromDb = fromDb.Where(p =>

                            // Objeto Pedido
                               p.PedidoId == searchNumber
                            || p.Cantidad == searchNumber
                            || ((p.FechaEntrega != null) ? p.FechaEntrega == searchDate : false)
                            || ((p.Referencia != null) ? p.Referencia.ToLower().Contains(search) : false)
                            || ((p.FechaBaja != null) ? p.FechaBaja == searchDate : false)
                            || ((p.EstructuraSolicitada != null) ? p.EstructuraSolicitada.Contains(search) : false)
                            || ((p.Observaciones != null) ? p.Observaciones.Contains(search) : false)

                            // Análisis del tipo de artículo
                            || ((_typestring == "tapa") ? ((p.Articulo as Tapa) != null) : false)
                            || ((_typestring == "base") ? ((p.Articulo as Base) != null) : false)
                            || ((_typestring == "vitrea") ? ((p.Articulo as Vitrea) != null) : false)

                            // Objeto Artículo
                            || ((p.Articulo.Particularidades != null) ? p.Articulo.Particularidades.ToLower().Contains(search) : false)

                            // Objeto Tapa
                            || (((p.Articulo as Tapa) != null) ? filteredTAPTipos.Contains((p.Articulo as Tapa).Tipo.Value) : false)
                            || (((p.Articulo as Tapa) != null) ? (p.Articulo as Tapa).Medida.ToLower().Contains(search) : false)
                            || (((p.Articulo as Tapa) != null) ? (search.Contains("melamina") ? (p.Articulo as Tapa).Melamina ?? false : false) : false)

                            // Laminado_CodigoId
                            || (
                                   // Si es una tapa
                                ((p.Articulo as Tapa) != null) ?
                                   // Si es una tapa, verifica que su valor no sea nulo
                                    ((p.Articulo as Tapa).Laminado_CodigoId != null) ?
                                   // Si es una tapa y su valor no es nulo, procede a buscar en el campo
                                        ((p.Articulo as Tapa).Laminado_CodigoId ?? "").ToString().ToLower().Contains(search)
                                   // Si es una tapa pero el campo es nulo, devuelve falso
                                        : false
                                   // Si el campo no es una tapa, devuelve falso
                                    : false
                                )

                            // Laminado_MuestrarioId
                            || (
                                   // Si es una tapa
                                ((p.Articulo as Tapa) != null) ?
                                   // Si es una tapa, verifica que su valor no sea nulo
                                    ((p.Articulo as Tapa).Laminado_MuestrarioId != null) ?
                                   // Si es una tapa y su valor no es nulo, procede a buscar en el campo
                                        ((p.Articulo as Tapa).Laminado_MuestrarioId ?? 0) == searchNumber
                                   // Si es una tapa pero el campo es nulo, devuelve falso
                                        : false
                                   // Si el campo no es una tapa, devuelve falso
                                    : false
                                )

                            || (((p.Articulo as Tapa) != null) ? filteredTAPBORTipos.Contains((p.Articulo as Tapa).Borde.Tipo) : false)
                            || (((p.Articulo as Tapa) != null) ? (((p.Articulo as Tapa).Borde.Color != null) ? filteredTAPBORColores.Contains((p.Articulo as Tapa).Borde.Color.Value) : false) : false)
                            || (((p.Articulo as Tapa) != null) ? filteredTAPBOREspesores.Contains((p.Articulo as Tapa).Borde.Espesor) : false)

                            // Objeto Base
                            || (((p.Articulo as Base) != null) ? (p.Articulo as Base).Modelo.ToLower().Contains(search) : false)
                            || (((p.Articulo as Base) != null) ? filteredBASEspesores.Contains((p.Articulo as Base).Espesor) : false)
                            || (((p.Articulo as Base) != null) ? (((p.Articulo as Base).Color != null) ? filteredBASColores.Contains((p.Articulo as Base).Color.Value) : false) : false)
                            || (((p.Articulo as Base) != null) ? filteredBASProveedores.Contains((p.Articulo as Base).Proveedor) : false)

                            // Objeto Vitrea
                            || (((p.Articulo as Vitrea) != null) ? (p.Articulo as Vitrea).Tipo.ToLower().Contains(search) : false)
                            || (((p.Articulo as Vitrea) != null) ? (p.Articulo as Vitrea).Medida.ToLower().Contains(search) : false)
                            || (((p.Articulo as Vitrea) != null) ? (search.Contains("transparente") ? (p.Articulo as Vitrea).Transparente ?? false : false) : false)

                            // Objeto FueraDeLista
                            || (((p.Articulo as FueraDeLista) != null) ? (p.Articulo as FueraDeLista).Titulo.ToLower().Contains(search) : false)
                            || (((p.Articulo as FueraDeLista) != null) ? (p.Articulo as FueraDeLista).Detalle.ToLower().Contains(search) : false)

                            );
                        break;
                }
            }

            #endregion

            #region Ordenamiento de Datos

            switch (sort)
            {
                case "pedido":
                    fromDb = fromDb.OrderBy(p => p.PedidoId);
                    break;
                case "InformeDePedidosActivos":
                    fromDb = fromDb.OrderBy(p => p.Gestion.ClienteId).ThenBy(p => p.PedidoId);
                    break;
                case "historial":
                    fromDb = fromDb.OrderBy(p => p.PedidoId);
                    break;
                default:
                    fromDb = fromDb.OrderBy(p => p.PedidoId);
                    break;
            }

            #endregion

            // Pagina los resultados.
            pageSize = pageSize ?? 15;
            page = page ?? 1;
            return fromDb.ToPagedList(page.Value, pageSize.Value);

        }

    }

    public class CambioDeSeguimiento
    {

        public CambioDeSeguimiento()
        {
            Cambios = new Dictionary<int, int>();
        }

        public DateTime Fecha { get; set; }
        public Dictionary<int, int> Cambios { get; set; }

    }

}