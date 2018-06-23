using System;
using System.Collections.Generic;
using PagedList;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Pedidos.DAL;
using Pedidos.Models;
using System.Diagnostics;
using Pedidos.ViewModels;
using System.Globalization;
using CustomExtensions;
using Pedidos.Models.Enums;
using System.Web.Routing;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Pedidos.Controllers
{
    /// <summary>
    /// Controlador que administra los artículos.
    /// </summary>
    [Authorize(Roles = "Webmaster")]
    public class ArticulosController : Controller
    {
        private PedidosDbContext db = new PedidosDbContext();

        /// <summary>
        /// Acepta y gestiona la modificación por parte del usuario del cuadro de búsqueda.
        /// </summary>
        /// <param name="command">Especifica la acción a realizarse.</param>
        /// <param name="newSearch">Especifica el nuevo valor de filtrado.</param>
        /// <param name="searchIn">Indica si busca en todos los campos disponibles, o en alguno en particular.</param>
        [HttpPost]
        public ActionResult Listado(string command, string newSearch, string searchIn)
        {
            // Pasa los valores recibidos a una función más amplia.
            return IndexPost(command, newSearch, searchIn, "Listado", "Articulos");
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lista los artículos del sistema.
        /// </summary>
        /// <param name="sort">Indica el ordenamiento a utilizar en la visualización.</param>
        /// <param name="search">Indica el valor del cuadro de búsqueda, que implica un filtro sobre los datos obtenidos desde la base de datos.</param>
        /// <param name="searchIn">Especifica si el valor del cuadro de búsqueda debe aplicarse en todos los campos disponibles o en alguno en particular.</param>
        /// <param name="page">Indica el número de página a devolver.</param>
        /// <param name="output">Indica si los datos deben salir por pantalla (por defecto), XML o PDF.</param>
        /// <param name="id">El parámetro <paramref name="id"/> debe ser siempre nulo para esta acción.</param>
        public ActionResult Listado(string sort, string search, string searchIn, int? page, string output, int? id)
        {
            // Comprueba que el usuario no haya especificado el parámetro "id".
            if (id != null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // La serialización de datos requiere que dichos valores estén desactivados.
            // Se guardan los valores actuales de LazyLoading y ProxyCreation
            // para recuperarlos al final del ActionMethod.
            var _LazyLoadingEnabled = db.Configuration.LazyLoadingEnabled;
            var _ProxyCreationEnabled = db.Configuration.ProxyCreationEnabled;

            // Si se especificó algún tipo de salida.
            if (!String.IsNullOrEmpty(output))
            {
                // Se deshabilita ProxyCreation y LazyLoading porque afectará la serialización de datos.
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
            }

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            db.Database.Log = s => Debug.WriteLine(s);

            // Se crea el ViewModel y se lo inicializa.
            var viewModel = new ArticulosIndexVM();
            viewModel.Titulo = "Listado de Artículos";
            viewModel.Action = "Listado";
            viewModel.Controller = "Articulos";
            viewModel.VolverA = Request.RawUrl;

            // Consulta la Base de Datos
            IQueryable<Articulo> fromDb = from a in db.Articulos
                                        where a.CodigoTango != null
                                        select a;

            // Filtra
            IndexGetFilter(ref fromDb, search, searchIn);

            // Ordena
            IndexGetSort(ref fromDb, sort);

            // Últimas Acciones
            var Resultado = IndexGetOutput(output, viewModel, ref fromDb, page);

            // Restituye los valores originales de LazyLoading y ProxyCreation
            db.Configuration.LazyLoadingEnabled = _LazyLoadingEnabled;
            db.Configuration.ProxyCreationEnabled = _ProxyCreationEnabled;

            // Finaliza
            return Resultado;
        }

        /// <summary>
        /// Función que se encarga de aplicar los filtros correctos a la secuencia IQueryable que posteriormente accionará sobre la base de datos.
        /// </summary>
        /// <param name="fromDb">Objeto que contiene los parámetros LinQ básicos sobre el cual se añadirá el filtrado requerido.</param>
        /// <param name="search">El valor del cuadro de búsqueda, que servirá para crear un filtro en base a palabras clave solicitadas por el usuario.</param>
        /// <param name="searchIn">Indica si el valor del cuadro de búsqueda se aplicará sobre todos los campos disponibles o alguno en particular.</param>
        private void IndexGetFilter(ref IQueryable<Articulo> fromDb, string search, string searchIn)
        {
            
            // Se extrae los datos culturales preferidos del cliente.
            var clientCulture = new CultureInfo(Request.UserLanguages[0]);

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
                var filteredBASProveedores = EnumExtensionMethods.EnumDisplayNamesToDictionary<Proveedores>().FilterKeys<Proveedores, string>(search);
                var filteredTAPTipos = EnumExtensionMethods.EnumDisplayNamesToDictionary<TiposDeTapas>().FilterKeys<TiposDeTapas, string>(search);
                var filteredTAPBORTipos = EnumExtensionMethods.EnumDisplayNamesToDictionary<TiposDeBordesDeTapas>().FilterKeys<TiposDeBordesDeTapas, string>(search);
                var filteredTAPBOREspesores = EnumExtensionMethods.EnumDisplayNamesToDictionary<EspesoresDeBordesDeTapas>().FilterKeys<EspesoresDeBordesDeTapas, string>(search);

                // Generación de la cadena que indica el tipo de artículo, por si se requiere en la búsqueda.
                // El tipo en sí no encontré como almacenarlo en una variable, por lo que al analizar el tipo de artículo
                // tuve que hacer un cast por cada tipo (como puede verse más abajo)
                string _typestring = "";
                _typestring = search == "tapa" ? _typestring = "tapa" : _typestring;
                _typestring = search == "base" ? _typestring = "base" : _typestring;
                _typestring = search == "vitrea" ? _typestring = "vitrea" : _typestring;

                switch (searchIn)
                {

                    // Objeto Pedido.
                    case "codigotango":
                        fromDb = fromDb.Where(a => ((a.CodigoTango != null) ? a.CodigoTango.ToLower().Contains(search) : false));
                        break;

                    // Análisis del tipo de artículo
                    case "tipodearticulo":
                        fromDb = fromDb.Where(a =>
                               ((_typestring == "tapa") ? ((a as Tapa) != null) : false)
                            || ((_typestring == "base") ? ((a as Base) != null) : false)
                            || ((_typestring == "vitrea") ? ((a as Vitrea) != null) : false)
                            );
                        break;

                    // Objeto Artículo.
                    case "particularidades":
                        fromDb = fromDb.Where(a => ((a.Particularidades != null) ? a.Particularidades.ToLower().Contains(search) : false));
                        break;

                    // Objeto Tapa.
                    case "tapatipo":
                        fromDb = fromDb.Where(a => (((a as Tapa) != null) ? filteredTAPTipos.Contains((a as Tapa).Tipo.Value) : false));
                        break;
                    case "tapamedida":
                        fromDb = fromDb.Where(a => (((a as Tapa) != null) ? (a as Tapa).Medida.ToLower().Contains(search) : false));
                        break;
                    case "tapamelamina":
                        fromDb = fromDb.Where(a => ((a as Tapa) != null) ? (search.Contains("melamina") ? ((a as Tapa).Melamina ?? false) : false) : false);
                        break;
                    case "tapabordetipo":
                        fromDb = fromDb.Where(a => (((a as Tapa) != null) ? filteredTAPBORTipos.Contains((a as Tapa).Borde.Tipo) : false));
                        break;
                    case "tapabordeespesor":
                        fromDb = fromDb.Where(a => (((a as Tapa) != null) ? filteredTAPBOREspesores.Contains((a as Tapa).Borde.Espesor) : false));
                        break;

                    // Objeto Base
                    case "basemodelo":
                        fromDb = fromDb.Where(a => (((a as Base) != null) ? (a as Base).Modelo.ToLower().Contains(search) : false));
                        break;
                    case "baseespesor":
                        fromDb = fromDb.Where(a => (((a as Base) != null) ? filteredBASEspesores.Contains((a as Base).Espesor) : false));
                        break;
                    case "baseproveedor":
                        fromDb = fromDb.Where(a => (((a as Base) != null) ? filteredBASProveedores.Contains((a as Base).Proveedor) : false));
                        break;

                    // Objeto Vitrea.
                    case "vitreatipo":
                        fromDb = fromDb.Where(a => (((a as Vitrea) != null) ? (a as Vitrea).Tipo.ToLower().Contains(search) : false));
                        break;
                    case "vitreamedida":
                        fromDb = fromDb.Where(a => (((a as Vitrea) != null) ? (a as Vitrea).Medida.ToLower().Contains(search) : false));
                        break;
                    case "vitreatransparente":
                        fromDb = fromDb.Where(a => ((a as Vitrea) != null) ? (search.Contains("transparente") ? ((a as Vitrea).Transparente ?? false) : false) : false);
                        break;

                    // Objeto FueraDeLista.
                    case "fueradelistatitulo":
                        fromDb = fromDb.Where(a => (((a as FueraDeLista) != null) ? (a as FueraDeLista).Titulo.ToLower().Contains(search) : false));
                        break;
                    case "fueradelistadetalle":
                        fromDb = fromDb.Where(a => (((a as FueraDeLista) != null) ? (a as FueraDeLista).Detalle.ToLower().Contains(search) : false));
                        break;

                    // Busca en todos los campos.
                    default:
                        fromDb = fromDb.Where(a =>

                            // Objeto Pedido
                            ((a.CodigoTango != null) ? a.CodigoTango.ToLower().Contains(search) : false)

                            // Análisis del tipo de artículo
                            || ((_typestring == "tapa") ? ((a as Tapa) != null) : false)
                            || ((_typestring == "base") ? ((a as Base) != null) : false)
                            || ((_typestring == "vitrea") ? ((a as Vitrea) != null) : false)

                            // Objeto Artículo
                            || ((a.Particularidades != null) ? a.Particularidades.ToLower().Contains(search) : false)

                            // Objeto Tapa
                            || (((a as Tapa) != null) ? filteredTAPTipos.Contains((a as Tapa).Tipo.Value) : false)
                            || (((a as Tapa) != null) ? (a as Tapa).Medida.ToLower().Contains(search) : false)
                            || (((a as Tapa) != null) ? (search.Contains("melamina") ? ((a as Tapa).Melamina ?? false) : false) : false)
                            || (((a as Tapa) != null) ? filteredTAPBORTipos.Contains((a as Tapa).Borde.Tipo) : false)
                            || (((a as Tapa) != null) ? filteredTAPBOREspesores.Contains((a as Tapa).Borde.Espesor) : false)

                            // Objeto Base
                            || (((a as Base) != null) ? (a as Base).Modelo.ToLower().Contains(search) : false)
                            || (((a as Base) != null) ? filteredBASEspesores.Contains((a as Base).Espesor) : false)
                            || (((a as Base) != null) ? filteredBASProveedores.Contains((a as Base).Proveedor) : false)

                            // Objeto Vitrea
                            || (((a as Vitrea) != null) ? (a as Vitrea).Tipo.ToLower().Contains(search) : false)
                            || (((a as Vitrea) != null) ? (a as Vitrea).Medida.ToLower().Contains(search) : false)
                            || (((a as Vitrea) != null) ? (search.Contains("transparente") ? ((a as Vitrea).Transparente ?? false) : false) : false)

                            // Objeto FueraDeLista
                            || (((a as FueraDeLista) != null) ? (a as FueraDeLista).Titulo.ToLower().Contains(search) : false)
                            || (((a as FueraDeLista) != null) ? (a as FueraDeLista).Detalle.ToLower().Contains(search) : false)

                            );
                        break;
                }
            }
        }

        /// <summary>
        /// Función que se encarga de aplicar el ordenamiento adecuado a la secuencia IQueryable que posteriormente accionará sobre la base de datos.
        /// </summary>
        /// <param name="fromDb">Objeto que contiene ciertos parámetros LinQ sobre los cuales se añadirá un parámetro de ordenamiento según se requiera.</param>
        /// <param name="sort">Define el nombre del ordenamiento solicitado, el cual representa uno o más parámetros de ordenamiento para las entidades afectadas.</param>
        private void IndexGetSort(ref IQueryable<Articulo> fromDb, string sort)
        {
            switch (sort)
            {
                //    case "contactoid_desc":
                //        //fromDb = fromDb.OrderByDescending(c => c.ContactoId);
                //        break;
                //    case "fechacontacto_desc":
                //        //fromDb = fromDb.OrderByDescending(c => c.FechaContacto);
                //        break;
                //    case "username_desc":
                //        //fromDb = fromDb.OrderByDescending(c => c.UserName);
                //        break;
                //    case "observaciones_desc":
                //        //fromDb = fromDb.OrderByDescending(c => c.Observaciones);
                //        break;
                //    case "clienteid_desc":
                //        //fromDb = fromDb.OrderByDescending(c => c.ClienteId);
                //        break;
                //    case "contactoid":
                //        //fromDb = fromDb.OrderBy(c => c.ContactoId);
                //        viewModel.SortLinks["contactoid"] = "contactoid_desc";
                //        break;
                //    case "username":
                //        //fromDb = fromDb.OrderBy(c => c.UserName);
                //        viewModel.SortLinks["username"] = "username_desc";
                //        break;
                //    case "observaciones":
                //        //fromDb = fromDb.OrderBy(c => c.Observaciones);
                //        viewModel.SortLinks["observaciones"] = "observaciones_desc";
                //        break;
                //    case "clienteid":
                //        //fromDb = fromDb.OrderBy(c => c.ClienteId);
                //        viewModel.SortLinks["clienteid"] = "clienteid_desc";
                //        break;
                default:
                    fromDb = fromDb.OrderBy(a => (a as Tapa).Medida);
                    //viewModel.SortLinks["fechacontacto"] = "fechacontacto_desc";
                    break;
            }
        }

        /// <summary>
        /// Prepara la salida de los datos solicitados por el usuario.
        /// </summary>
        /// <param name="output">Especifica el modo de salida, como por ejemplo XML o PDF. Si este valor se omite o es nulo, sale por pantalla.</param>
        /// <param name="viewModel">Especifica el VM que se completará para ser enviado a la Vista, en el caso en que los datos salgan por pantalla.</param>
        /// <param name="fromDb">Secuencia IQueryable que contiene los datos necesarios para accionar finalmente sobre la base de datos y obtener las entidades solicitadas.</param>
        /// <param name="page">Especifica el número de página a recuperar.</param>
        private ActionResult IndexGetOutput(string output, ArticulosIndexVM viewModel, ref IQueryable<Articulo> fromDb, int? page)
        {
            int pageSize = 15; // Cantidad de ítems por defecto.
            int pageNumber = (page ?? 1);

            // Selecciona según la salida elegida.
            switch ((output ?? "").ToLower())
            {

                // Se requiere una salida a XML
                case "xml":
                    return new XmlResult<List<Articulo>>(fromDb.ToList());

                // Por default sale a pantalla mediante el listado usual de pedidos.
                default:

                    // Selecciona según el ActionMethod actual.
                    switch (viewModel.Action)
                    {
                        default:
                            viewModel.Items = fromDb.ToPagedList(pageNumber, pageSize);
                            return View("Listado", viewModel);
                    }
            }
        }

        /// <summary>
        /// Acepta y gestiona la modificación por parte del usuario del cuadro de búsqueda, en este caso derivado desde otros métodos con la misma estructura.
        /// </summary>
        /// <param name="command">Especifica la acción a realizarse.</param>
        /// <param name="newSearch">Especifica el nuevo valor de filtrado.</param>
        /// <param name="searchIn">Indica si busca en todos los campos disponibles, o en alguno en particular.</param>
        /// <param name="action">Nombre del ActionMethod actual.</param>
        /// <param name="controller">Nombre del Controlador actual.</param>
        [HttpPost]
        public ActionResult IndexPost(string command, string newSearch, string searchIn, string action, string controller)
        {
            switch (command)
            {
                case "Buscar":
                    var qs = Request.QueryString.toRouteValueDictionary();
                    if (!String.IsNullOrEmpty(newSearch))
                    {
                        RouteValueDictionary newParams = new RouteValueDictionary();
                        newParams.Add("search", newSearch.ToLower());
                        if (!String.IsNullOrEmpty(searchIn))
                        {
                            newParams.Add("searchIn", searchIn.ToLower());
                        }
                        else
                        {
                            newParams.Add("searchIn", String.Empty);
                        }
                        newParams.Add("page", "1");
                        return RedirectToAction(action, controller, qs.Merge(newParams));
                    }
                    else
                    {
                        if (qs.ContainsKey("page")) qs.Remove("page");
                        if (qs.ContainsKey("search")) qs.Remove("search");
                        if (qs.ContainsKey("searchIn")) qs.Remove("searchIn");
                        return RedirectToAction(action, controller, qs);
                    }

                default:
                    break;
            }
            return RedirectToAction(action, controller, Request.QueryString.toRouteValueDictionary());
        }

        /// <summary>
        /// Acepta y gestiona la acción del usuario sobre ítems seleccionados en pantalla. Acomoda y deriva dichos datos a otras funciones específicas.
        /// </summary>
        /// <param name="ArticuloId">Contiene el listado de todos los IDs de los artículos en pantalla. Debe permanecer en coordinación con el parámetro <paramref name="Seleccionado"/>.</param>
        /// <param name="Seleccionado">Contiene el listado del estado de selección de todos los artículos en pantalla. Debe permanecer en coordinación con el parámetro <paramref name="ArticuloId"/>.</param>
        /// <param name="VolverA">Especifica la URL que se utilizará en el caso de finalizar este proceso, tanto en una eventual cancelación como en su correcta finalización.</param>
        /// <param name="command">Especifica la acción solicitada a aplicarse sobre los ítems seleccionados.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Select(int?[] ArticuloId, bool?[] Seleccionado, string VolverA, string command)
        {
            if (String.IsNullOrEmpty(command)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Si ambas variables son nulas, entonces no se seleccionó ningún pedido. Vuelve.
            if (ArticuloId == null && Seleccionado == null) return Redirect(VolverA ?? "");

            // Si alguna de las dos variables, pero no las dos, es nula, entonces ha ocurrido algún error no controlado. Devuelve BadRequest.
            if (ArticuloId == null || Seleccionado == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Si ambas variables tienen distinto número de elementos, devuelve BadRequest.
            if (ArticuloId.Length != Seleccionado.Length) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Se coloca en un diccionario todas los pares clave/valor de los pedidos seleccionados.
            var Articulos = ArticuloId.Zip(Seleccionado, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v).Where(i => i.Value ?? false).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();

            switch (command)
            {
                case "Editar Seleccionados":
                    return await Editar_Paso1_RecibeSeleccion(Articulos, VolverA);
                case "Borrar Seleccionados":
                    return Borrar_Paso1_ConfirmaAcciones(Articulos, VolverA);
                default:
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Muestra el detalle de un artículo.
        /// </summary>
        /// <param name="id">El Id del que se obtendrá información detallada.</param>
        /// <param name="returnto">URL que se usará cuando se desee regresar a la pantalla anterior.</param>
        public async Task<ActionResult> Details(int? id, string returnto)
        {
            // Si no se pasó un Id.
            if (id == null)
            {
                // Muestra un error.
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            // Busca el artículo en la base de datos.
            Articulo articulo = await db.Articulos.FindAsync(id);
            
            // Si no se encontró dicho artículo
            if (articulo == null)
            {
                // Muestra un error.
                return HttpNotFound();
            }
            
            // Pasa el artículo a la vista.
            ViewBag.ReturnURL = returnto ?? "";
            return View(articulo);
        }

        // CREAR CREAR CREAR
        // CREAR CREAR CREAR
        /// <summary>
        /// Muestra la vista que permite seleccionar el tipo de artículo a dar de alta.
        /// </summary>
        public ActionResult Crear_Paso1_SeleccionaTipo()
        {
            ViewBag.ReturnURL = Request.QueryString["volvera"];
            return View();
        }

        /// <summary>
        /// Crea un artículo del tipo solicitado y lo envia a la vista como un artículo nuevo en proceso de alta.
        /// </summary>
        /// <param name="tipo">Tipo de artículo a crear.</param>
        public ActionResult Crear_Paso2_Alta(string tipo)
        {
            // Si no se especificó un tipo de artículo
            if (String.IsNullOrEmpty(tipo))
            {
                // Devuelve un error
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            // Esta lista de artículos se crea porque así lo requiere el VM
            // (el VM acepta más de un artículo)
            var articulos = new List<Articulo>();
            
            switch (tipo)
            {
                case "Tapa":
                    articulos.Add(new Tapa());
                    break;
                case "Base":
                    articulos.Add(new Base());
                    break;
                case "Vitrea":
                    articulos.Add(new Vitrea());
                    break;
                case "FueraDeLista":
                    articulos.Add(new FueraDeLista());
                    break;
                default:
                    // El tipo especificado se desconoce. Devuelve un error.
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Se crea el VM y se inicializa con la lista de artículos recientemente creada
            var VM = new ArticulosCreateVM(articulos);
            VM.ReturnURL = Request.QueryString["volvera"];
            
            return View(VM);
        }

        /// <summary>
        /// Genera un artículo en base a una copia de otro y lo envía a la vista como parte del proceso de alta.
        /// </summary>
        /// <param name="id">Id del artículo que se usará como molde para generar el nuevo.</param>
        /// <returns></returns>
        public ActionResult Copiar(int? id)
        {
            // Si no se especificó un Id de artículo
            if (id == null)
            {
                // Devuelve un error
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            // Esta lista de artículos se crea porque así lo requiere el VM
            // (el VM acepta más de un artículo)
            var articulos = new List<Articulo>();
            
            // Se recupera el artículo desde el cual se va a hacer la copia.
            Articulo articulo;
            var fromDb = db.Articulos.Find(id.Value);

            // Si no se encontró el artículo solicitado como origen de copia.
            if (fromDb == null)
            {
                // Devuelve un error.
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            else
            {
                // Crea una copia del artículo original y
                // setea algunos campos dado que la copia ahora
                // representa la carga de un artículo nuevo.
                articulo = fromDb.Copiar();
                articulo.CodigoTango = "";
                
                // Este artículo será la base del nuevo artículo a crearse.
                articulos.Add(articulo);
            }

            // Se crea el VM y se inicializa con la lista de artículos recientemente creada
            var VM = new ArticulosCreateVM(articulos);
            VM.ReturnURL = Request.QueryString["volvera"];
            
            return View("Crear_Paso2_Alta", VM);
        }

        /// <summary>
        /// Recibe el nuevo articulo desde la vista listo para las últimas comprobaciones y el proceso de adición a la base de datos.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Crear_Paso3_RecibeAlta()
        {
            
            // Se carga el ViewModel con los datos del formulario.
            var VM = new Pedidos.ViewModels.ArticulosCreateVM();
            if (!TryUpdateModel(VM))
            {
                // Hubo un error al cargar el ViewModel. Se genera un error y se devuelve a la pantalla de edición.
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al actualizar los datos. Por favor revise lo ingresado y reintente nuevamente.");
                return View("Crear_Paso2_Alta", VM);
            }

            // Comprueba que los Códigos Tango ingresados no se repitan entre sí.
            var CodigosTango = new List<string>();
            foreach (var item in VM.items)
            {
                
                if (!CodigosTango.Contains(item.CodigoTango)) CodigosTango.Add(item.CodigoTango);
                else
	            {
                    ModelState.AddModelError(String.Empty, "Uno o más Códigos Tango se repiten en los artículos a dar de alta.");
                    return View("Crear_Paso2_Alta", VM);
                }
                
            }

            // Se recorre el ViewModel.
            for (var i = 0; i < VM.items.Count; i++)
            {
                var VMitem = VM.items[i];
                // Comprueba si el Código Tango del ítem ya existe en la base de datos
                var EsUnico = await db.Articulos.Where(a => a.CodigoTango == VMitem.CodigoTango).CountAsync() > 0 ? false : true;

                if (!EsUnico)
                {
                    ModelState.AddModelError(String.Empty, "El Código Tango especificado en el ítem " + (i + 1).ToString() + " ya existe.");
                    return View("Crear_Paso2_Alta", VM);
                }

                var newArt = VMitem.NewInstance();
                VMitem.CopiarEn(newArt);
                db.Articulos.Add(newArt);
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                    ModelState.AddModelError(String.Empty, "Ha ocurrido un error inesperado al grabar los datos. Intente nuevamente en unos instantes.");
                    return View("Crear_Paso2_Alta", VM);
                throw;
            }

            // Termina el proceso y regresa a la pantalla inicial.
            return Redirect(VM.ReturnURL);
        }

        // BORRAR BORRAR BORRAR
        // BORRAR BORRAR BORRAR
        /// <summary>
        /// Recibe una lista de artículos para confirmar su eliminación.
        /// </summary>
        /// <param name="Articulos">Vector con los IDs a borrar.</param>
        /// <param name="VolverA">URL que se usará para cancelar el proceso y regresar a la pantalla inicial, o al finalizar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Borrar_Paso1_ConfirmaAcciones(int?[] Articulos, string VolverA)
        {
            // Si no se pasó el vector Articulos, regresa.
            if (Articulos == null) return Redirect(VolverA ?? "");
            
            // Si el vector está vacío, regresa.
            if (Articulos.Count() == 0) return Redirect(VolverA ?? "");

            // Pasa los datos a la vista.
            ViewBag.Articulos = Articulos;
            ViewBag.ReturnURL = VolverA;
            return View("Borrar_Paso1_ConfirmaAccion");
        }

        /// <summary>
        /// Recibe una lista de artículos confirmados, para proceder a su eliminación.
        /// </summary>
        /// <param name="Articulos">Vector con los IDs a borrar.</param>
        /// <param name="VolverA">URL que se usará para cancelar el proceso y regresar a la pantalla inicial, o al finalizar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Borrar_Paso2_EjecutaAcciones(int?[] Articulos, string VolverA)
        {

            // Pasa los artículos a un vector que no sea Nullable
            var ArticulosArray = Articulos.ToArray();
            
            // Recupera todos los registros que cumplen con los IDs pasados en el vector.
            var fromDb = await (from a in db.Articulos
                         where ArticulosArray.Contains(a.ArticuloId)
                         select a).ToListAsync();

            // Recorre los resultados sin foreach y desde atrás
            // debido a que se irán borrando y los topes del vector cambiarán,
            // además perder validez su enumerador.
            for(var i = fromDb.Count; i > 0; i--)
            {
                // Borra el ítem.
                db.Articulos.Remove(fromDb[i-1]);
            }

            try
            {
                // Intenta guardar los datos.
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Error de grabación.
                ModelState.AddModelError(String.Empty, "No se pudieron guardar los cambios. Reintente nuevamente.");
                ViewBag.Articulos = Articulos;
                ViewBag.ReturnURL = VolverA;
                return Borrar_Paso1_ConfirmaAcciones(Articulos, VolverA);
            }

            // Los datos se borraron correctamente. Regresa.
            return Redirect(VolverA);
        }

        // EDICION EDICION EDICION
        // EDICION EDICION EDICION
        /// <summary>
        /// Recibe una lista de artículos para editarlos.
        /// </summary>
        /// <param name="Articulos">Vector con los IDs a editar.</param>
        /// <param name="VolverA">URL que se usará para cancelar el proceso y regresar a la pantalla inicial, o al finalizar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar_Paso1_RecibeSeleccion(int?[] Articulos, string VolverA)
        {
            
            var fromDb = from a in db.Articulos.Where(a => Articulos.Contains(a.ArticuloId))
                         select a;

            var VM = new Pedidos.ViewModels.ArticulosEditVM(await fromDb.ToListAsync());
            VM.ReturnURL = VolverA ?? "";
            return View("Edit", VM);

        }

        /// <summary>
        /// Recibe una lista de artículos ya editados, para proceder al guardado en la base de datos.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar_Paso2_RecibeEdicion()
        {
            var Advertencias = new List<string>();
            var HubieronCambios = false;

            // Se carga el ViewModel con los datos del formulario.
            var entradaVM = new Pedidos.ViewModels.ArticulosEditVM();
            if (!TryUpdateModel(entradaVM))
            {
                // Hubo un error al cargar el ViewModel. Se genera un error y se devuelve a la pantalla de edición.
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al actualizar los datos. Por favor revise lo ingresado y reintente nuevamente.");
                return View("Edit", entradaVM);
            }

            // Se inicializa un VM de confirmación en base al VM recibido.
            var salidaVM = new ArticulosConfirmEditVM(entradaVM);

            // Se pasan los datos del ViewModel a los correspondientes objetos.
            // Se recorren todos los items que vinieron del formulario y están actualmente en el ViewModel.
            for (var i = 0; i < entradaVM.items.Count; i++)
            {
                // Carga los datos del pedido.
                var VMitem = entradaVM.items[i];
                var fromDb = await db.Articulos.FindAsync(VMitem.ArticuloId);

                // Se comprueba si el usuario modificó algún campo.
                if (VMitem.Difiere(fromDb))
                {
                    HubieronCambios = true;

                    // Se comprueba si CódigoTango está en algún pedido vigente.
                    var CodigosTangoPresenteEn = await db.Pedidos.Where(p => p.EstructuraSolicitada == fromDb.CodigoTango).CountAsync();
                    if (CodigosTangoPresenteEn > 0)
                    {
                        // Se marca dicho código para advertirle al usuario que si procede con la modificación,
                        // todos los pedidos que antes estaban vinculados a ese código
                        // serán enviados a Tango con un código de artículo fuera de lista.
                        Advertencias.Add(fromDb.CodigoTango);
                    }
                }
            }

            // Si surgió alguna advertencia, se envia nuevamente el VM a la pantalla de confirmación.
            if (Advertencias.Count > 0)
            {
                salidaVM.Codigos = String.Join(", ", Advertencias.ToArray());
                return View("ConfirmEdit", salidaVM);
            }
            
            // Si no se modificó ningún registro, avisa y vuelve a la misma pantalla de edición.
            if (!HubieronCambios)
            {
                ModelState.AddModelError(String.Empty, "No se ha modificado ningún campo, y por lo tanto no se ha modificado la base de datos.");
                return View("Edit", entradaVM);
            }
            else
            {
                return await Editar_Paso3_RecibeConfirmacion(salidaVM);
            }
        }

        /// <summary>
        /// Recibe los artículos editados y ya confirmados, dado que la edición ha provocado cambios en otras partes del sistema que debían ser confirmados por el usuario.
        /// </summary>
        /// <param name="VM">El VM con los datos editados.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar_Paso3_RecibeConfirmacion(ArticulosConfirmEditVM VM)
        {
            
            // Devuelve un error interno si no se recibieron correctamente los datos
            // desde la pantalla de confirmación.
            if (VM == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            // Se procesan los datos.
            for (var i = 0; i < VM.items.Count; i++)
            {
                // Carga los datos del pedido.
                var VMitem = VM.items[i];
                var fromDb = await db.Articulos.FindAsync(VMitem.ArticuloId);

                // Hace una comprobación necesaria por si algún componente del código de laminado es nulo.
                var laminadoCoherente = !(VMitem.TAP_LAM_MuestrarioId == null ^ VMitem.TAP_LAM_Codigo == null);
                if (!laminadoCoherente)
                {
                    // Regresa a la edición de los artículos mostrando el mensaje de error correspondiente.
                    ModelState.AddModelError(String.Empty, "Uno de los dos componentes del código de laminado es nulo. Se aceptan ambos valores nulos a la vez, o ambos valores completos y correctos.");
                    return View("Edit", (ArticulosEditVM)VM);
                }

                // Se comprueba si el usuario modificó algún campo.
                if (VMitem.Difiere(fromDb))
                {
                    // Pasa los datos del VM a la base de datos.
                    VMitem.CopiarEn(fromDb);

                    // Comprueba si CódigoTango está en algún pedido vigente.
                    await db.Pedidos.Where(p => p.EstructuraSolicitada == fromDb.CodigoTango && p.FechaBaja == null).ForEachAsync(p => p.EstructuraSolicitada = null);
                }
            }
            
            // Intenta guardar los cambios.
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Recupera el VM de edición.
                var salidaVM = (ArticulosEditVM)VM;
                
                // Regresa a la edición de los artículos mostrando el mensaje de error correspondiente.
                ModelState.AddModelError(String.Empty, "Surgieron errores al guardar los cambios. Intente nuevamente en unos segundos.");
                return View("Edit", salidaVM);
                
                throw;
            }

            // Se han guardado los cambios de forma satisfactoria. Se termina el proceso de edición.
            return RedirectToAction("Listado");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
