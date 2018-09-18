using CustomExtensions;
using Pedidos.Models;
using Pedidos.Models.Enums;
using Pedidos.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Pedidos.Controllers
{

    /// <summary>
    /// Controlador que administra los pedidos.
    /// </summary>
    [Authorize]
    public class PedidosController : UOWController
    {
        
        private PedidosBusiness business = new PedidosBusiness();

        /// <summary>
        /// Recibe mediante POST cualquier cambio solicitado desde la vista Pedidos/Index.cshtml y lo pasa a una función más amplia preparada para gestionar cualquier grupo de pedidos.
        /// </summary>
        /// <param name="comando">Identifica el botón que se presionó para aceptar cierto grupo de opciones.</param>
        /// <param name="nuevaBusqueda">Especifica el nuevo valor a buscar elegido por el usuario.</param>
        /// <param name="buscarEn">Especifica el ámbito de búsqueda dentro el cual se buscará <paramref name="nuevaBusqueda"/></param>
        /// <returns>Devuelve directamente el valor de esta función global: una nueva vista con los valores elegidos ya aplicados.</returns>
        [HttpPost]
        public ActionResult PorGestion(string comando, string nuevaBusqueda, string buscarEn, string queryString)
        {

            // Si no se pasó ningún comando, se pasó un comando vacío, o el comando no está en la lista de comandos admitidos, devuelve error.
            string[] comandos = new string[1] { "Buscar" };
            if (String.IsNullOrEmpty(comando) || !comandos.Contains(comando)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            // Si no se pasó ningún campo de búsqueda, se pasó un campo de búsqueda vacío, o el campo de búsqueda no está en la lista de campos admitidos, devuelve error.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (String.IsNullOrEmpty(buscarEn) || !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            nuevaBusqueda = nuevaBusqueda ?? String.Empty;

            // Pasa los valores recibidos a una función más amplia.
            return Index_ProcesaPOST(comando, nuevaBusqueda, buscarEn, "PorGestion", "Pedidos", queryString);

        }

        /// <summary>
        /// Lista Pedidos en base a una gestión espeficicada.
        /// </summary>
        /// <param name="gestionId">El código de gestión solicitado.</param>
        /// <param name="salida">Indica en qué formato se solicita el resultado. Si no se especifica el parámetro, la salida es por pantalla. Otros valores posibles son: "xml".</param>
        /// <param name="pagina">Indica el número de página a listar.</param>
        /// <param name="buscar">Indica el texto a buscar dentro de <paramref name="buscarEn"/>.</param>
        /// <param name="buscarEn">Indica en donde buscar <paramref name="buscar"/>.</param>
        /// <param name="orden">Indica el código de ordenamiento del listado.</param>
        public ActionResult PorGestion(string orden, string buscar, string buscarEn, int? pagina, string salida, int? gestionId)
        {

            #region Inicialización

            var solicitaDatoVM = new Pedidos.ViewModels.PedidosVM.SolicitaDatoVM();
            solicitaDatoVM.ControlPrincipal.Nombre = "gestionId";
            solicitaDatoVM.ControlPrincipal.Placeholder = "Ingrese una gestión.";
            solicitaDatoVM.Formulario.Accion = "PorGestion";
            solicitaDatoVM.Formulario.Controlador = "Pedidos";
            solicitaDatoVM.Formulario.BotonDeEnvio.Etiqueta = "Confirmar";
            solicitaDatoVM.Formulario.Metodo = FormMethod.Get;
            solicitaDatoVM.Origen = Request.RawUrl;
            solicitaDatoVM.Titulo = "Ingrese una gestión";
            solicitaDatoVM.Jumbotron.Titulo = solicitaDatoVM.Titulo;
            solicitaDatoVM.Jumbotron.Descripción = "Consulte los pedidos que corresponden a una gestión determinada.";

            #endregion

            #region Comprobaciones Comunes

            // Comprueba que la variable "orden" contenga un valor válido.
            string[] ordenamientos = new string[1] { "Falta hacer." };
            if (!String.IsNullOrEmpty(orden) && !orden.Contains(orden)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "buscar"
            buscar = buscar ?? String.Empty;

            // Comprueba que la variable "buscarEn" contenga un valor válido.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (!String.IsNullOrEmpty(buscarEn) && !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "pagina"
            pagina = pagina ?? 1;

            // Comprueba que la variable "salida" contenga un valor válido.
            string[] salidas = new string[1] { "xml" };
            if (!String.IsNullOrEmpty(salida) && !salidas.Contains(salida)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            #endregion

            #region Comprobaciones Específicas

            // Si es la primera vez que se ingresa a la acción,
            if (!gestionId.HasValue && !Request.QueryString.AllKeys.Contains("gestionId")) return View("SolicitaDato", solicitaDatoVM);

            // Si el usuario no ingresó cliente alguno,
            if (!gestionId.HasValue && Request.QueryString.AllKeys.Contains("gestionId"))
            {
                ModelState.AddModelError("ControlPrincipal", "Debe ingresar un código de gestión.");
                solicitaDatoVM.gestionId = gestionId;
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Si el código de cliente ingresado no es válido,
            if (UOW.GestionRepository.ObtenerPorId(gestionId.Value) == null)
            {
                ModelState.AddModelError("ControlPrincipal", "El código de gestión ingresado no es válido.");
                solicitaDatoVM.gestionId = gestionId;
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            // UOW.Log = s => Debug.WriteLine(s);

            #endregion

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se guardan los valores actuales de LazyLoading y ProxyCreation.
            var _LazyLoadingEnabled = UOW.LazyLoadingEnabled;
            var _ProxyCreationEnabled = UOW.ProxyCreationEnabled;

            // Si se especificó alguna salida alternativa a la salida usual,
            // significa que quizás se necesite serializar la salida.
            if (!String.IsNullOrEmpty(salida))
            {
                // Se deshabilita ProxyCreation y LazyLoading porque afecta la serialización.
                UOW.LazyLoadingEnabled = false;
                UOW.ProxyCreationEnabled = false;
            }

            #endregion

            #region Obtiene Datos

            // Obtiene los pedidos desde la base de datos.
            var Pedidos = UOW.PedidoRepository.ListarPorGestion(gestionId.Value, orden, buscar, buscarEn, pagina, 15, userLanguage);

            #endregion

            // Se crea el ViewModel y se lo inicializa.
            var VM = new PedidosIndexVM();
            VM.Titulo = String.Format("Listado de pedidos para la gestión {0}", gestionId.ToString());
            VM.Action = "PorGestion";
            VM.Controller = "Pedidos";
            VM.Parametro = gestionId.Value.ToString();
            VM.VolverA = Request.RawUrl;
            VM.Pedidos = Pedidos;

            // Antes de finalizar, se restituyen los valores originales en LazyLoading y ProxyCreation.
            UOW.LazyLoadingEnabled = _LazyLoadingEnabled;
            UOW.ProxyCreationEnabled = _ProxyCreationEnabled;

            // Termina.
            return ProcesaSalidaDelListado(VM, salida);

        }

        /// <summary>
        /// Recibe mediante POST cualquier cambio solicitado desde la vista Pedidos/Index.cshtml y lo pasa a una función más amplia preparada para gestionar cualquier grupo de pedidos.
        /// </summary>
        /// <param name="comando">Identifica el botón que se presionó para aceptar cierto grupo de opciones.</param>
        /// <param name="nuevaBusqueda">Especifica el nuevo valor a buscar elegido por el usuario.</param>
        /// <param name="buscarEn">Especifica el ámbito de búsqueda dentro el cual se buscará <paramref name="nuevaBusqueda"/></param>
        /// <returns>Devuelve directamente el valor de esta función global: una nueva Vista con los valores elegidos ya aplicados.</returns>
        [HttpPost]
        public ActionResult PorZona(string comando, string nuevaBusqueda, string buscarEn, string queryString)
        {

            // Si no se pasó ningún comando, se pasó un comando vacío, o el comando no está en la lista de comandos admitidos, devuelve error.
            string[] comandos = new string[1] { "Buscar" };
            if (String.IsNullOrEmpty(comando) || !comandos.Contains(comando)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Si no se pasó ningún campo de búsqueda, se pasó un campo de búsqueda vacío, o el campo de búsqueda no está en la lista de campos admitidos, devuelve error.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (String.IsNullOrEmpty(buscarEn) || !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            nuevaBusqueda = nuevaBusqueda ?? String.Empty;

            // Pasa los valores recibidos a una función más amplia.
            return Index_ProcesaPOST(comando, nuevaBusqueda, buscarEn, "PorZona", "Pedidos", queryString);

        }

        /// <summary>
        /// Lista Pedidos en base a una zona espeficicada.
        /// </summary>
        /// <param name="id">El código de zona solicitada.</param>
        /// <param name="salida">Indica en qué formato se solicita el resultado. Si no se especifica el parámetro, la salida es por pantalla. Otros valores posibles son: "xml".</param>
        /// <param name="pagina">Indica el número de página a listar.</param>
        /// <param name="buscar">Indica el texto a buscar dentro de <paramref name="buscarEn"/>.</param>
        /// <param name="buscarEn">Indica en donde buscar <paramref name="buscar"/>.</param>
        /// <param name="orden">Indica el código de ordenamiento del listado.</param>
        public ActionResult PorZona(string orden, string buscar, string buscarEn, int? pagina, string salida, Zonas? zonaId)
        {

            #region Inicialización

            var solicitaDatoVM = new Pedidos.ViewModels.PedidosVM.SolicitaDatoVM();
            solicitaDatoVM.ControlPrincipal.Nombre = "zonaId";
            solicitaDatoVM.ControlPrincipal.Placeholder = "Seleccione una zona.";
            solicitaDatoVM.Formulario.Accion = "PorZona";
            solicitaDatoVM.Formulario.Controlador = "Pedidos";
            solicitaDatoVM.Formulario.BotonDeEnvio.Etiqueta = "Obtener Pedidos";
            solicitaDatoVM.Formulario.Metodo = FormMethod.Get;
            solicitaDatoVM.zonaId = Zonas.CABACentro;
            solicitaDatoVM.Origen = Request.RawUrl;
            solicitaDatoVM.Titulo = "Seleccione una zona";
            solicitaDatoVM.Jumbotron.Titulo = solicitaDatoVM.Titulo;
            solicitaDatoVM.Jumbotron.Descripción = "Consulte los pedidos que corresponden a una zona geográfica.";
            
            #endregion

            #region Comprobaciones Comunes

            // Comprueba que la variable "orden" contenga un valor válido.
            string[] ordenamientos = new string[1] { "Falta hacer." };
            if (!String.IsNullOrEmpty(orden) && !orden.Contains(orden)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "buscar"
            buscar = buscar ?? String.Empty;

            // Comprueba que la variable "buscarEn" contenga un valor válido.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (!String.IsNullOrEmpty(buscarEn) && !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "pagina"
            pagina = pagina ?? 1;

            // Comprueba que la variable "salida" contenga un valor válido.
            string[] salidas = new string[1] { "xml" };
            if (!String.IsNullOrEmpty(salida) && !salidas.Contains(salida)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            #endregion
            
            #region Comprobaciones Específicas
            
            // Si no se especificó una zona.
            if (zonaId == null)
            {
                // Muestra una pantalla de selección.
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Verifica que el parámetro "id" contenga un Código de Zona válido.
            if (!Enum.IsDefined(typeof(Zonas), zonaId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            // UOW.Log = s => Debug.WriteLine(s);

            #endregion

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se guardan los valores actuales de LazyLoading y ProxyCreation.
            var _LazyLoadingEnabled = UOW.LazyLoadingEnabled;
            var _ProxyCreationEnabled = UOW.ProxyCreationEnabled;

            // Si se especificó alguna salida alternativa a la salida usual.
            if (!String.IsNullOrEmpty(salida))
            {
                // Se deshabilita ProxyCreation y LazyLoading porque afecta la serialización.
                UOW.LazyLoadingEnabled = false;
                UOW.ProxyCreationEnabled = false;
            }

            #endregion

            #region Obtiene Datos

            // Obtiene los pedidos desde la base de datos.
            var Pedidos = UOW.PedidoRepository.ListarPorZona(zonaId.Value, orden, buscar, buscarEn, pagina, 15, userLanguage);

            #endregion

            // Se crea el ViewModel y se lo inicializa.
            var VM = new PedidosIndexVM();
            VM.Titulo = String.Format("Listado de pedidos para: {0}", zonaId.GetEnumMemberDisplayName<Zonas>().ToString());
            VM.Action = "PorZona";
            VM.Controller = "Pedidos";
            VM.Parametro = zonaId.Value.ToString();
            VM.VolverA = Request.RawUrl;
            VM.Pedidos = Pedidos;

            // Antes de finalizar, se restituyen los valores originales en LazyLoading y ProxyCreation.
            UOW.LazyLoadingEnabled = _LazyLoadingEnabled;
            UOW.ProxyCreationEnabled = _ProxyCreationEnabled;

            // Termina.
            return ProcesaSalidaDelListado(VM, salida);

        }

        /// <summary>
        /// Recibe mediante POST cualquier cambio solicitado desde la vista Pedidos/Index.cshtml y lo pasa a una función más amplia preparada para tratar cualquier grupo de pedidos.
        /// </summary>
        /// <param name="comando">Identifica el botón que se presionó para aceptar cierto grupo de opciones.</param>
        /// <param name="nuevaBusqueda">Especifica el nuevo valor a buscar elegido por el usuario.</param>
        /// <param name="buscarEn">Especifica el ámbito de búsqueda dentro el cual se buscará <paramref name="nuevaBusqueda"/></param>
        /// <returns>Devuelve directamente el valor de esta función global: una nueva Vista con los valores elegidos ya aplicados.</returns>
        [HttpPost]
        public ActionResult PorCliente(string comando, string nuevaBusqueda, string buscarEn, string queryString)
        {

            // Si no se pasó ningún comando, se pasó un comando vacío, o el comando no está en la lista de comandos admitidos, devuelve error.
            string[] comandos = new string[1] { "Buscar" };
            if (String.IsNullOrEmpty(comando) || !comandos.Contains(comando)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Si no se pasó ningún campo de búsqueda, se pasó un campo de búsqueda vacío, o el campo de búsqueda no está en la lista de campos admitidos, devuelve error.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (String.IsNullOrEmpty(buscarEn) || !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            nuevaBusqueda = nuevaBusqueda ?? String.Empty;

            // Pasa los valores recibidos a una función más amplia.
            return Index_ProcesaPOST(comando, nuevaBusqueda, buscarEn, "PorCliente", "Pedidos", queryString);

        }

        /// <summary>
        /// Lista pedidos en base a un cliente espeficicado.
        /// </summary>
        /// <param name="id">El código de cliente solicitado.</param>
        /// <param name="salida">Indica en qué formato se solicita el resultado. Si no se especifica el parámetro, la salida es por pantalla. Otros valores posibles son: "xml".</param>
        /// <param name="pagina">Indica el número de página a listar.</param>
        /// <param name="buscar">Indica el texto a buscar dentro de <paramref name="buscarEn"/>.</param>
        /// <param name="buscarEn">Indica en donde buscar <paramref name="buscar"/>.</param>
        /// <param name="orden">Indica el código de ordenamiento del listado.</param>
        public ActionResult PorCliente(string orden, string buscar, string buscarEn, int? pagina, string salida, string clienteId)
        {

            #region Inicialización

            var solicitaDatoVM = new Pedidos.ViewModels.PedidosVM.SolicitaDatoVM();
            solicitaDatoVM.ControlPrincipal.Nombre = "clienteId";
            solicitaDatoVM.ControlPrincipal.Placeholder = "Ingrese un cliente.";
            solicitaDatoVM.Formulario.Accion = "PorCliente";
            solicitaDatoVM.Formulario.Controlador = "Pedidos";
            solicitaDatoVM.Formulario.BotonDeEnvio.Etiqueta = "Confirmar";
            solicitaDatoVM.Formulario.Metodo = FormMethod.Get;
            solicitaDatoVM.Origen = Request.RawUrl;
            solicitaDatoVM.Titulo = "Ingrese un cliente";
            solicitaDatoVM.Jumbotron.Titulo = solicitaDatoVM.Titulo;
            solicitaDatoVM.Jumbotron.Descripción = "Consulte los pedidos que corresponden a un cliente específico.";

            #endregion

            #region Comprobaciones Comunes

            // Comprueba que la variable "orden" contenga un valor válido.
            string[] ordenamientos = new string[1] { "Falta hacer." };
            if (!String.IsNullOrEmpty(orden) && !orden.Contains(orden)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "buscar"
            buscar = buscar ?? String.Empty;

            // Comprueba que la variable "buscarEn" contenga un valor válido.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (!String.IsNullOrEmpty(buscarEn) && !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "pagina"
            pagina = pagina ?? 1;

            // Comprueba que la variable "salida" contenga un valor válido.
            string[] salidas = new string[1] { "xml" };
            if (!String.IsNullOrEmpty(salida) && !salidas.Contains(salida)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            #endregion

            #region Comprobaciones Específicas

            // Si es la primera vez que se ingresa a la acción,
            if (String.IsNullOrWhiteSpace(clienteId) && !Request.QueryString.AllKeys.Contains("clienteId")) return View("SolicitaDato", solicitaDatoVM);

            // Si el usuario no ingresó código de cliente alguno,
            if (String.IsNullOrWhiteSpace(clienteId) && Request.QueryString.AllKeys.Contains("clienteId"))
            {
                ModelState.AddModelError("ControlPrincipal", "Debe ingresar un código de cliente.");
                solicitaDatoVM.clienteId = clienteId;
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Si el código de cliente ingresado no es válido,
            if (UOW.ClienteRepository.ObtenerPorId(clienteId) == null)
            {
                ModelState.AddModelError("ControlPrincipal", "El código de cliente ingresado no es válido.");
                solicitaDatoVM.clienteId = clienteId;
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            // UOW.Log = s => Debug.WriteLine(s);

            #endregion

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se guardan los valores actuales de LazyLoading y ProxyCreation.
            var _LazyLoadingEnabled = UOW.LazyLoadingEnabled;
            var _ProxyCreationEnabled = UOW.ProxyCreationEnabled;

            // Si se especificó alguna salida alternativa a la salida usual.
            if (!String.IsNullOrEmpty(salida))
            {
                // Se deshabilita ProxyCreation y LazyLoading porque afecta la serialización.
                UOW.LazyLoadingEnabled = false;
                UOW.ProxyCreationEnabled = false;
            }

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            UOW.Log = s => Debug.WriteLine(s);

            #endregion

            #region Obtiene Datos

            // Obtiene los pedidos desde la base de datos.
            var Pedidos = UOW.PedidoRepository.ListarPorCliente(clienteId, orden, buscar, buscarEn, pagina, 15, userLanguage);

            #endregion

            // Se crea el ViewModel y se lo inicializa.
            var VM = new PedidosIndexVM();
            VM.Titulo = String.Format("Listado de pedidos del cliente {0}", clienteId.ToString());
            VM.Action = "PorCliente";
            VM.Controller = "Pedidos";
            VM.Parametro = clienteId;
            VM.VolverA = Request.RawUrl;
            VM.Pedidos = Pedidos;

            // Antes de finalizar, se restituyen los valores originales en LazyLoading y ProxyCreation.
            UOW.LazyLoadingEnabled = _LazyLoadingEnabled;
            UOW.ProxyCreationEnabled = _ProxyCreationEnabled;

            return ProcesaSalidaDelListado(VM, salida);

        }

        /// <summary>
        /// Lista el historial de un pedido.
        /// </summary>
        /// <param name="id">El código de pedido solicitado.</param>
        /// <param name="salida">Indica en qué formato se solicita el resultado. Si no se especifica el parámetro, la salida es por pantalla. Otros valores posibles son: "xml".</param>
        public ActionResult Historial(int? id, string salida)
        {

            #region Inicialización

            var solicitaDatoVM = new Pedidos.ViewModels.PedidosVM.SolicitaDatoVM();
            solicitaDatoVM.ControlPrincipal.Nombre = "id";
            solicitaDatoVM.ControlPrincipal.Placeholder = "Ingrese un pedido.";
            solicitaDatoVM.Formulario.Accion = "Historial";
            solicitaDatoVM.Formulario.Controlador = "Pedidos";
            solicitaDatoVM.Formulario.BotonDeEnvio.Etiqueta = "Confirmar";
            solicitaDatoVM.Formulario.Metodo = FormMethod.Get;
            solicitaDatoVM.Origen = Request.RawUrl;
            solicitaDatoVM.Titulo = "Historial de pedido";
            solicitaDatoVM.Jumbotron.Titulo = solicitaDatoVM.Titulo;
            solicitaDatoVM.Jumbotron.Descripción = "El sistema conserva una copia del original por cada registro que es sometido a un cambio. Este proceso lista el historial de modificaciones que corresponde a un determinado pedido.";

            // Comprueba que la variable "salida" contenga un valor válido.
            string[] salidas = new string[1] { "xml" };
            if (!String.IsNullOrEmpty(salida) && !salidas.Contains(salida)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            salida = "historial" + salida;

            #endregion

            #region Comprobaciones Específicas

            // Si es la primera vez que se ingresa a la acción,
            if (!id.HasValue && !Request.QueryString.AllKeys.Contains("id")) return View("SolicitaDato", solicitaDatoVM);

            // Si el usuario no ingresó código de cliente alguno,
            if (!id.HasValue && Request.QueryString.AllKeys.Contains("id"))
            {
                ModelState.AddModelError("ControlPrincipal", "Debe ingresar un código de pedido.");
                solicitaDatoVM.id = id;
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Si el código de cliente ingresado no es válido,
            UOW.PedidoRepository.IncluyeDadosDeBaja = true;
            if (UOW.PedidoRepository.ObtenerPorId(id.Value) == null)
            {
                ModelState.AddModelError("ControlPrincipal", "El código de pedido ingresado no es válido.");
                solicitaDatoVM.id = id;
                return View("SolicitaDato", solicitaDatoVM);
            }
            UOW.PedidoRepository.Reset();

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            // UOW.Log = s => Debug.WriteLine(s);

            #endregion

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se guardan los valores actuales de LazyLoading y ProxyCreation.
            var _LazyLoadingEnabled = UOW.LazyLoadingEnabled;
            var _ProxyCreationEnabled = UOW.ProxyCreationEnabled;

            // Si se especificó alguna salida alternativa a la salida usual.
            if (!String.IsNullOrEmpty(salida))
            {
                // Se deshabilita ProxyCreation y LazyLoading porque afecta la serialización.
                UOW.LazyLoadingEnabled = false;
                UOW.ProxyCreationEnabled = false;
            }

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            UOW.Log = s => Debug.WriteLine(s);

            #endregion

            #region Obtiene Datos

            // Obtiene los pedidos desde la base de datos.
            var _pedidos = UOW.PedidoRepository.ListarHistorial(id.Value, userLanguage);
            var _variacionesDelSeguimiento = UOW.PedidoRepository.ObtenerVariacionesDelSeguimiento(_pedidos.Select(p => p.PedidoId).ToArray());
            var _seguimientosCompletos = UOW.PedidoRepository.ObtenerSeguimientoCompleto(_pedidos.Select(p => p.PedidoId).ToArray());

            #endregion

            // Se crea el ViewModel y se lo inicializa.
            var VM = new PedidosHistorialVM();
            VM.Titulo = String.Format("Historial del pedido {0}", id.ToString());
            VM.Action = "Historial";
            VM.Controller = "Pedidos";
            VM.Pedidos = _pedidos;
            VM.VariacionesDeSeguimientos = _variacionesDelSeguimiento;
            VM.SeguimientosCompletos = _seguimientosCompletos;

            // Antes de finalizar, se restituyen los valores originales en LazyLoading y ProxyCreation.
            UOW.LazyLoadingEnabled = _LazyLoadingEnabled;
            UOW.ProxyCreationEnabled = _ProxyCreationEnabled;

            // Termina.
            return View("Historial", VM);

        }

        /// <summary>
        /// Recibe mediante POST cualquier cambio solicitado desde la vista Pedidos/Index.cshtml y lo pasa a una función más amplia preparada para tratar cualquier grupo de pedidos.
        /// </summary>
        /// <param name="comando">Identifica el botón que se presionó para aceptar cierto grupo de opciones.</param>
        /// <param name="nuevaBusqueda">Especifica el nuevo valor a buscar elegido por el usuario.</param>
        /// <param name="buscarEn">Especifica el ámbito de búsqueda dentro el cual se buscará <paramref name="nuevaBusqueda"/></param>
        /// <returns>Devuelve directamente el valor de esta función global: una nueva Vista con los valores elegidos ya aplicados.</returns>
        [HttpPost]
        public ActionResult Todos(string comando, string nuevaBusqueda, string buscarEn, string queryString)
        {

            // Si no se pasó ningún comando, se pasó un comando vacío, o el comando no está en la lista de comandos admitidos, devuelve error.
            string[] comandos = new string[1] { "Buscar" };
            if (String.IsNullOrEmpty(comando) || !comandos.Contains(comando)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Si no se pasó ningún campo de búsqueda, se pasó un campo de búsqueda vacío, o el campo de búsqueda no está en la lista de campos admitidos, devuelve error.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (String.IsNullOrEmpty(buscarEn) || !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            nuevaBusqueda = nuevaBusqueda ?? String.Empty;

            // Pasa los valores recibidos a una función más amplia.
            return Index_ProcesaPOST(comando, nuevaBusqueda, buscarEn, "Todos", "Pedidos", queryString);

        }

        /// <summary>
        /// Lista todos los pedidos en el sistema.
        /// </summary>
        /// <param name="id">Este parámetro debe ser nulo o debe obviarse.</param>
        /// <param name="salida">Indica en qué formato se solicita el resultado. Si no se especifica el parámetro, la salida es por pantalla. Otros valores posibles son: "xml".</param>
        /// <param name="pagina">Indica el número de página a listar.</param>
        /// <param name="buscar">Indica el texto a buscar dentro de <paramref name="buscarEn"/>.</param>
        /// <param name="buscarEn">Indica en donde buscar <paramref name="buscar"/>.</param>
        /// <param name="orden">Indica el código de ordenamiento del listado.</param>
        public ActionResult Todos(string orden, string buscar, string buscarEn, int? pagina, string salida)
        {

            #region Comprobaciones Comunes

            // Comprueba que la variable "orden" contenga un valor válido.
            string[] ordenamientos = new string[1] { "Falta hacer." };
            if (!String.IsNullOrEmpty(orden) && !orden.Contains(orden)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "buscar"
            buscar = buscar ?? String.Empty;

            // Comprueba que la variable "buscarEn" contenga un valor válido.
            string[] camposDeBusqueda = new Pedidos.ViewModels.PedidosIndexVM().SearchInList.Select(i => i.Value).ToArray();
            if (!String.IsNullOrEmpty(buscarEn) && !camposDeBusqueda.Contains(buscarEn)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Acomoda la variable "pagina"
            pagina = pagina ?? 1;

            // Comprueba que la variable "salida" contenga un valor válido.
            string[] salidas = new string[1] { "xml" };
            if (!String.IsNullOrEmpty(salida) && !salidas.Contains(salida)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            #endregion

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se guardan los valores actuales de LazyLoading y ProxyCreation.
            var _LazyLoadingEnabled = UOW.LazyLoadingEnabled;
            var _ProxyCreationEnabled = UOW.ProxyCreationEnabled;

            // Si se especificó alguna salida alternativa a la salida usual.
            if (!String.IsNullOrEmpty(salida))
            {
                // Se deshabilita ProxyCreation y LazyLoading porque afecta la serialización.
                UOW.LazyLoadingEnabled = false;
                UOW.ProxyCreationEnabled = false;
            }

            // Habilitar para examinar secuencias SQL pasadas al motor de base de datos desde Entity Framework
            UOW.Log = s => Debug.WriteLine(s);

            #endregion

            #region Obtiene Datos

            // Obtiene los pedidos desde la base de datos.
            var Pedidos = UOW.PedidoRepository.ListarTodo(orden, buscar, buscarEn, pagina, 15, userLanguage);

            #endregion

            // Se crea el ViewModel y se lo inicializa.
            var VM = new PedidosIndexVM();
            VM.Titulo = "Listado de pedidos";
            VM.Action = "Todos";
            VM.Controller = "Pedidos";
            VM.VolverA = Request.RawUrl;
            VM.Pedidos = Pedidos;

            // Antes de finalizar, se restituyen los valores originales en LazyLoading y ProxyCreation.
            UOW.LazyLoadingEnabled = _LazyLoadingEnabled;
            UOW.ProxyCreationEnabled = _ProxyCreationEnabled;

            // Termina.
            return ProcesaSalidaDelListado(VM, salida);

        }

        /// <summary>
        /// ActionMethod presente con el solo fin de redirigir la vista por defecto hacia otro método específico.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Recoge los datos de los pedidos a listar y los presenta según corresponda.
        /// </summary>
        /// <param name="VM">VM con los datos necesarios para cualquiera de los modos de salida.</param>
        /// <param name="salida">Tipo de salida de los datos.</param>
        private ActionResult ProcesaSalidaDelListado(PedidosIndexVM VM, string salida)
        {

            // Acomoda variable "salida"
            salida = salida ?? "";

            // Selecciona según la salida elegida.
            switch (salida.ToLower())
            {

                // Se requiere una salida a XML
                case "xml":
                    return new XmlResult<List<Pedido>>(VM.Pedidos.ToList());

                case "historial":
                    return View("Historial", VM);

                case "historialpantalla":
                    return View("Historial", VM);

                // Por default sale a pantalla mediante el listado usual de pedidos.
                default:

                    // Selecciona según el ActionMethod actual.
                    switch (VM.Action)
                    {
                        default:
                            return View("Listado", VM);
                    }
            }

        }

        /// <summary>
        /// Acepta y gestiona la modificación por parte del usuario del cuadro de búsqueda, en este caso derivado desde otros métodos con la misma estructura.
        /// </summary>
        /// <param name="comando">Especifica la acción a realizarse.</param>
        /// <param name="nuevaBusqueda">Especifica el nuevo valor de filtrado.</param>
        /// <param name="buscarEn">Indica si busca en todos los campos disponibles, o en alguno en particular.</param>
        /// <param name="accion">Nombre del ActionMethod actual.</param>
        /// <param name="controlador">Nombre del Controlador actual.</param>
        private ActionResult Index_ProcesaPOST(string comando, string nuevaBusqueda, string buscarEn, string accion, string controlador, string queryString)
        {

            var oldQueryString = new RouteValueDictionary();

            if (!String.IsNullOrWhiteSpace(queryString))
            {
                queryString.Split(Char.Parse("&")).ToList().ForEach( e =>
                    {
                        var elemento = e.Split(Char.Parse("="));
                        if (elemento.Length != 2) throw new Exception("Ha ocurrido una situación no controlada al desglosar un QueryString. Consulte al administrador.");
                        oldQueryString.Add(elemento[0], elemento[1]);
                    }
                );
            }

            switch (comando)
            {
                case "Buscar":
                    
                    // Si se detecta un nuevo valor de búsqueda,
                    if (!String.IsNullOrEmpty(nuevaBusqueda))
                    {
                        
                        // Crea un nuevo RouteValueDictionary
                        RouteValueDictionary newParams = new RouteValueDictionary();
                        
                        // Agrega ese valor al nuevo RouteValueDictionary.
                        newParams.Add("buscar", nuevaBusqueda.ToLower());
                        
                        // Si "buscarEn" contiene algún valor,
                        if (!String.IsNullOrEmpty(buscarEn))
                        {
                            // Agrega ese valor al nuevo RouteValueDictionary.
                            newParams.Add("buscarEn", buscarEn.ToLower());
                        }
                        
                        // Si "buscarEn" está vacío,
                        else
                        {
                            // Si existe la variable "buscarEn" en el QueryString, se elimina.
                            if (oldQueryString.ContainsKey("buscarEn")) oldQueryString.Remove("buscarEn");
                        }
                        
                        // Al cambiar los parámetros de búsqueda, la paginación se reinicia.
                        newParams.Add("pagina", "1");
                        
                        // Retorna con la combinación de los nuevos parámetros dentro del QueryString actual.
                        return RedirectToAction(accion, controlador, oldQueryString.Merge(newParams));

                    }

                    // Si no se detectó ningún nuevo valor de búsqueda,
                    // significa que el usuario está intentando eliminar
                    // la búsqueda actual.
                    else
                    {
                        if (oldQueryString.ContainsKey("pagina")) oldQueryString.Remove("pagina");
                        if (oldQueryString.ContainsKey("buscar")) oldQueryString.Remove("buscar");
                        if (oldQueryString.ContainsKey("buscarEn")) oldQueryString.Remove("buscarEn");
                        return RedirectToAction(accion, controlador, oldQueryString);
                    }

                default:
                    break;
            }

            // Si llegó a este punto es porque algo salió mal.
            // La instrucción "return" está sólo para cumplir con ciertas formalidades.
            return RedirectToAction(accion, controlador);

        }

        /// <summary>
        /// Muestra el detalle de un pedido.
        /// </summary>
        /// <param name="id">El Id del que se obtendrá su información detallada.</param>
        /// <param name="volverA">URL que se usará cuando se desee regresar a la pantalla de partida.</param>
        public async Task<ActionResult> Detalle(int? id, string volverA)
        {

            // Si no se especificó un Id, devuelve un error.
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            // Se intenta leer el pedido solicitado desde la base de datos.
            UOW.PedidoRepository.IncluyeAdjuntos = true;
            UOW.PedidoRepository.IncluyeDadosDeBaja = true;
            Pedido pedido = await UOW.PedidoRepository.ObtenerPorIdAsync(id ?? 0);
            
            // Si el pedido no pudo ser ubicado, devuelve error.
            if (pedido == null) return HttpNotFound();

            ViewBag.ReturnURL = volverA ?? String.Empty;
            return View(pedido);

        }

        /// <summary>
        /// Acomoda y deriva la acción del usuario sobre ítems seleccionados hacia otras funciones específicas.
        /// </summary>
        /// <param name="pedidoId">Contiene el listado de todos los IDs de los artículos en pantalla. Debe permanecer en coordinación con el parámetro <paramref name="seleccionado"/>.</param>
        /// <param name="seleccionado">Contiene el listado del estado de selección de todos los artículos en pantalla. Debe permanecer en coordinación con el parámetro <paramref name="pedidoId"/>.</param>
        /// <param name="volverA">Especifica la URL que se utilizará en el caso de finalizar este proceso, tanto en una eventual cancelación como en su correcta finalización.</param>
        /// <param name="comando">Especifica la acción solicitada a aplicarse sobre los ítems seleccionados.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Webmaster,Gerencia")]
        public async Task<ActionResult> SeleccionaAccion(int[] pedidoId, bool[] seleccionado, string volverA, string comando)
        {

            #region Comprobaciones Previas

            // Si ambas variables son nulas, entonces no se seleccionó ningún pedido. Vuelve.
            if ((pedidoId == null || pedidoId.Length < 1) && (seleccionado == null || seleccionado.Length < 1)) return Redirect(volverA ?? "");

            // Si alguna de las dos variables (pero no las dos) es nula, entonces ha ocurrido algún error no controlado. Devuelve BadRequest.
            if ((pedidoId == null || pedidoId.Length < 1) || (seleccionado == null || seleccionado.Length < 1)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Si ambas variables tienen distinto número de elementos, devuelve BadRequest.
            if (pedidoId.Length != seleccionado.Length) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Se coloca en un diccionario todas los pares clave/valor de los pedidos seleccionados.
            var Pedidos = pedidoId.Zip(seleccionado, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v).Where(i => i.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();

            // Acomoda la variable "volverA"
            volverA = volverA ?? String.Empty;

            // Corrobora que el comando sea válido.
            string[] comandos = new string[2] { "Editar Seleccionados", "Borrar Seleccionados" };
            if (String.IsNullOrEmpty(comando) || !comandos.Contains(comando)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            #endregion

            switch (comando)
            {
                case "Editar Seleccionados":
                    return await ModificacionPaso1(Pedidos, volverA);
                case "Borrar Seleccionados":
                    return BajaPaso1(Pedidos, volverA);
                default:
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

        }

        /// <summary>
        /// Genera el alta de un nuevo pedido.
        /// </summary>
        /// <param name="contactoid">Id de la gestión a la que se asociará el pedido.</param>
        /// <param name="returnto">URL que se usará al finalizar para saber desde dónde partió el proceso.</param>
        [Authorize(Roles = "Webmaster,Gerencia")]
        public ActionResult AltaPaso1(int? gestionId, string volverA)
        {
            
            // Si no se especificó un código de gestión,
            if (gestionId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Se intenta obtener la gestión desde la base de datos.
            var gestion = UOW.GestionRepository.ObtenerPorId(gestionId);

            // La gestión puede encontrarse en proceso de baja.
            // Se comprobó que el usuario luego de eliminar el último pedido
            // queda adentro de una gestión dada de baja, bajo la cual no puede
            // permitirse el ingreso de nuevos pedidos.
            
            // Si el código de gestión indicado es inexistente o está dado de baja,
            if (gestion == null || gestion.FechaBaja != null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var VM1 = new PedidosCreateStep1VM(gestionId.Value);
            VM1.VolverA = volverA;
            return View("AltaPaso1", VM1);

        }

        /// <summary>
        /// Recibe la primer pantalla de alta de nuevos pedidos y procesa esos datos, enviando a la siguiente vista los nuevos artículos correctos para su continuación.
        /// </summary>
        /// <param name="VM1">VM que contiene los datos de la pantalla inicial de creación de pedidos.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AltaPaso2(PedidosCreateStep1VM VM1)
        {

            #region Primeras Comprobaciones

            // Si el número de iteraciones está fuera del intervalo esperado, vuelve a la pantalla anterior.
            if (VM1.Iteraciones < 1 || VM1.Iteraciones > 15)
            {
                ModelState.AddModelError(String.Empty, "El número de iteraciones debe ser estar entre 1 y 15.");
                return View("AltaPaso1", VM1);
            }

            // Si no se han especificado códigos en absoluto, vuelve a la pantalla anterior.
            if (String.IsNullOrWhiteSpace(VM1.Codigo1) && String.IsNullOrWhiteSpace(VM1.Codigo2))
            {
                ModelState.AddModelError(String.Empty, "No se han especificado códigos.");
                return View("AltaPaso1", VM1);
            }

            #endregion

            #region Creación de Variables

            // Contenedor donde se alojarán los nuevos pedidos.
            List<Pedido> pedidos = new List<Pedido>();

            Articulo fromDb;
            Articulo newArt;
            SeguimientoGlobal newSG;
            Pedido newPed;

            #endregion

            #region Análisis del Primer Código Tango Solicitado

            // Si se especificó un código en el primer campo, se comprueba.
            if (!String.IsNullOrWhiteSpace(VM1.Codigo1))
            {
                
                // Se intenta obtener dicho código desde la base de datos.
                fromDb = await UOW.ArticuloRepository.ObtenerPorCodigoTangoAsync(VM1.Codigo1);

                // Si el código indicado no existe, regresa a la pantalla anterior.
                if (fromDb == null)
                {
                    ModelState.AddModelError(String.Empty, "El archivo especificado en el primer campo no es válido.");
                    return View("AltaPaso1", VM1);
                }

                // El nuevo artículo del nuevo pedido será
                // una copia del indicado anteriormente.
                newArt = fromDb.Copiar();
                newPed = new Pedido();
                newPed.Articulo = newArt;
                newPed.SeguimientoGlobal = new SeguimientoGlobal(newPed.Cantidad, User);
                
                // El nuevo artículo ya no poseerá el campo "CodigoTango".
                // En su lugar, el pedido contendrá el campo "EstructuraSolicitada" con dicho código.
                newPed.EstructuraSolicitada = newArt.CodigoTango;
                newArt.CodigoTango = "";

                // Agrega el item a la lista de pedidos nuevos.
                pedidos.Add(newPed);

            }

            #endregion

            #region Análisis del Segundo Código Tango Solicitado

            // Si se especificó un código en el segundo campo, se comprueba.
            if (!String.IsNullOrWhiteSpace(VM1.Codigo2))
            {
                
                // Se intenta obtener dicho código desde la base de datos.
                fromDb = await UOW.ArticuloRepository.ObtenerPorCodigoTangoAsync(VM1.Codigo2);

                // Si el código indicado no existe, regresa a la pantalla anterior.
                if (fromDb == null)
                {
                    ModelState.AddModelError(String.Empty, "El archivo especificado en el segundo campo no es válido.");
                    return View("AltaPaso1", VM1);
                }

                // El nuevo artículo del nuevo pedido será
                // una copia del indicado anteriormente.
                newArt = fromDb.Copiar();
                newPed = new Pedido();
                newPed.Articulo = newArt;
                newPed.SeguimientoGlobal = new SeguimientoGlobal(newPed.Cantidad, User);

                // El nuevo artículo ya no poseerá el campo "CodigoTango".
                // En su lugar, el pedido contendrá el campo "EstructuraSolicitada" con dicho código.
                newPed.EstructuraSolicitada = newArt.CodigoTango;
                newArt.CodigoTango = "";

                // Agrega el item a la lista de pedidos nuevos.
                pedidos.Add(newPed);

            }

            #endregion

            #region Procesamiento de Iteraciones

            // Cuenta la cantidad de Códigos Tango indicados.
            var ArticulosSolicitados = pedidos.Count();

            // Si se solicitaron 2 iteraciones o más,
            if (VM1.Iteraciones > 1)
            {
                
                // Cuenta las iteraciones exceptuando la primera, que ya está cargada.
                for (var CuentaIteraciones = 1; CuentaIteraciones <= VM1.Iteraciones - 1; CuentaIteraciones++)
                {
                    
                    // Recorre los primeros dos items de la lista de pedidos (item 0, item 1) si se indicaron dos Códigos Tango.
                    // Recorre sólo el primer item de la lista de pedidos (item 0) si se indicó un único Código Tango. 
                    for (var CuentaArticulosSolicitados = 0; CuentaArticulosSolicitados < ArticulosSolicitados; CuentaArticulosSolicitados++)
                    {
                        
                        // Agrega al final de la lista una copia del pedido actual, seteando los Ids a cero
                        // debido a que se trata de entidades a agregar.
                        newArt = pedidos[CuentaArticulosSolicitados].Articulo.Copiar();
                        newArt.ArticuloId = 0;
                        newSG = pedidos[CuentaArticulosSolicitados].SeguimientoGlobal.Copiar();
                        newSG.SeguimientoGlobalId = 0;
                        newPed = pedidos[CuentaArticulosSolicitados].Copiar();
                        newPed.Articulo = newArt;
                        newPed.SeguimientoGlobal = newSG;
                        newPed.PedidoId = 0;
                        pedidos.Add(newPed);

                    }

                }

            }

            #endregion

            var VM2out = new Pedidos.ViewModels.PedidosCreateStep2VM(pedidos, VM1.GestionId);
            VM2out.VolverA = VM1.VolverA;

            if (VM1.ModoRapido)
            {
                VM2out.ModoRapido = true;
                return View("AltaPaso2Rapido", VM2out);
            }
            else
            {
                VM2out.ModoRapido = false;
                return View("AltaPaso2Normal", VM2out);
            }
        }

        /// <summary>
        /// Recibe los pedidos ingresados en su totaliad, listos para ser agregados a la base de datos.
        /// </summary>
        /// <param name="VM2in">VM con los datos de los nuevos pedidos.</param>
        /// <param name="gestionId">Id de la gestión a la que se asociarán los pedidos.</param>
        /// <param name="volverA">URL que contiene la pantalla desde donde se partió.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Webmaster,Gerencia")]
        public async Task<ActionResult> AltaPaso3(PedidosCreateStep2VM VM2in, int? gestionId, string volverA)
        {

            #region Comprobaciones Previas

            // Si el código de gestión es inválido,
            if (gestionId == null || UOW.GestionRepository.ObtenerPorId(gestionId) == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Adapta la variable "volverA"
            volverA = volverA ?? String.Empty;

            #endregion

            #region Traspaso de Datos desde el VM

            // Contenedor
            var Pedidos = new List<Pedido>();

            // Recorre los pedidos recibidos en el ViewModel
            foreach (var item in VM2in.items)
            {
                
                // Crea una entidad del mismo tipo del artículo a dar de alta,
                // y vuelca los valores desde el ViewModel.
                var nuevoarticulo = item.NewInstance();
                var nuevopedido = new Pedido();
                nuevopedido.Articulo = nuevoarticulo;
                item.CopiarEn(nuevopedido);
                
                // Asigna el código de gestión.
                nuevopedido.GestionId = gestionId.Value;
                
                // Asigna una nueva entidad de seguimiento,
                // que inicializa en cero (pedido nuevo)
                nuevopedido.SeguimientoGlobal = new SeguimientoGlobal(nuevopedido.Cantidad, User);
                
                // Identifica el usuario que intervino en el alta.
                nuevopedido.UserName = Request.IsAuthenticated ? User.Identity.Name : null; 
                
                // Si los datos recibidos desde el cliente ya no respetan
                // la estructura solicitada al comienzo,
                if (!nuevopedido.ComprobarEstructura()) nuevopedido.EstructuraSolicitada = null;

                // La entidad está completa y lista para ser insertada a la base de datos.
                Pedidos.Add(nuevopedido);

            }

            #endregion

            #region Genera el Alta

            try
            {
                // Se genera el alta.
                await business.Alta(this, Pedidos);
                UOW.SaveChanges();
            }
            catch (Exception)
            {
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al grabar el registro. Sugerencia: revise los valores del código de muestrario y código de laminado.");
                return View(VM2in.ModoRapido ? "AltaPaso2Rapido" : "AltaPaso2Normal", VM2in);
            }

            #endregion

            // La creación de pedidos resultó satisfactoria. Regresa a la pantalla de partida.
            return Redirect(volverA);

        }

        /// <summary>
        /// Recibe una lista de pedidos para confirmar su eliminación.
        /// </summary>
        /// <param name="pedidos">Vector con los IDs a borrar.</param>
        /// <param name="volverA">URL que se usará para cancelar el proceso y regresar a la pantalla inicial, o al finalizar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Webmaster,Gerencia")]
        public ActionResult BajaPaso1(int[] pedidos, string volverA)
        {

            // Comprobaciones Previas
            volverA = volverA ?? String.Empty;
            if (pedidos == null || pedidos.Length < 1) return Redirect(volverA);

            ViewBag.Pedidos = pedidos;
            ViewBag.VolverA = volverA;
            return View("Baja");

        }

        /// <summary>
        /// Recibe una lista de pedidos confirmados para proceder a su baja.
        /// </summary>
        /// <param name="pedidos">Vector con los IDs a borrar.</param>
        /// <param name="volverA">URL que se usará para cancelar el proceso y regresar a la pantalla inicial, o al finalizar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Webmaster,Gerencia")]
        public async Task<ActionResult> BajaPaso2(int[] pedidos, string volverA)
        {

            // Comprobaciones Previas
            volverA = volverA ?? String.Empty;
            if (pedidos == null || pedidos.Length < 1) return Redirect(volverA);

            try
            {
                // Borra los pedidos indicados.
                await business.Baja(this, pedidos, null);
                await UOW.SaveChangesAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al plasmar las modificaciones en la base de datos. Reintente nuevamente en unos instantes.");
                ViewBag.Pedidos = pedidos;
                ViewBag.VolverA = volverA;
                return View("Baja");
            }

            // Termina
            return Redirect(volverA);

        }

        /// <summary>
        /// Recibe una lista de pedidos para modificarlos.
        /// </summary>
        /// <param name="pedidos">Vector con los IDs a modificar.</param>
        /// <param name="volverA">URL que se usará para cancelar el proceso y regresar a la pantalla inicial, o al finalizar.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Webmaster,Gerencia")]
        public async Task<ActionResult> ModificacionPaso1(int[] pedidos, string volverA)
        {

            // Comprobaciones Previas
            volverA = volverA ?? String.Empty;
            if (pedidos == null || pedidos.Length < 1) return Redirect(volverA);

            // Obtiene las entidades desde la base de datos.
            var fromDb = await UOW.PedidoRepository.ObtenerDesdeVectorAsync(pedidos);

            var VM = new Pedidos.ViewModels.PedidosEditVM(fromDb);
            VM.VolverA = volverA;
            return View("Modificacion", VM);

        }

        /// <summary>
        /// Recibe una lista de pedidos ya modificados, para proceder al guardado en la base de datos.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Webmaster,Gerencia")]
        public async Task<ActionResult> ModificacionPaso2()
        {

            #region Comprobaciones Previas

            // Se carga el ViewModel con los datos del formulario.
            var VM = new Pedidos.ViewModels.PedidosEditVM();
            if (!TryUpdateModel(VM))
            {
                // Hubo un error al cargar el ViewModel. Se genera un error y se devuelve a la pantalla de edición.
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al actualizar los datos. Por favor revise lo ingresado y reintente nuevamente.");
                return View("Modificacion", VM);
            }

            // Si el ViewModel se cargó correctamente pero no se obtuvo ningún item (error interno / hack), devuelve error.
            if (VM.items == null) return new HttpStatusCodeResult(HttpStatusCode.NoContent);

            #endregion

            #region Actualización de la Base de Datos

            try
            {
                await business.Modificacion(this, VM, DateTime.Now);
                await UOW.SaveChangesAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al guardar los datos. Verifique nuevamente y cerciórese que el muestrario exista y el código de laminado sea correcto y pertenezca al mismo.");
                return View("Modificacion", VM);
            }

            #endregion

            // Todo ha salido bien. Vuelve a la pantalla de donde partió la solicitud de edición.
            return Redirect(VM.VolverA);

        }

    }

    public class PedidosBusiness : IDisposable
    {

        private bool disposed = false;

        /// <summary>
        /// Genera el alta de uno o más pedidos.
        /// </summary>
        /// <param name="controlador">Controlador vigente al momento de llamar a esta función.</param>
        /// <param name="pedidos">Lista de entidades a dar de alta.</param>
        public async Task<bool> Alta(UOWController controlador, List<Pedido> pedidos)
        {

            #region Comprobaciones Previas

            await ValidarLaminado(controlador, pedidos);

            #endregion

            #region Genera el Alta

            // Se genera el alta.
            controlador.UOW.PedidoRepository.Insert(pedidos);

            #endregion

            return true;

        }

        /// <summary>
        /// Modifica uno o más pedidos.
        /// </summary>
        /// <param name="controlador">Controlador vigente al momento de llamar a esta función.</param>
        /// <param name="pedidos">Lista de entidades a modificar.</param>
        public async Task<bool> Modificacion(UOWController controlador, PedidosEditVM VM, DateTime? fecha)
        {

            // Comprueba que se haya pasado al menos una entidad.
            if (VM.items == null || VM.items.Count == 0) return true;

            // Prepara la variable "fecha"
            fecha = fecha ?? DateTime.Now;

            // Crea variables de trabajo.
            Articulo backArt;
            Pedido backPed;

            // Recorre las entidades a modificar.
            foreach (var item in VM.items)
            {
                
                // Obtiene la entidad desde la base de datos.
                var fromDb = await controlador.UOW.PedidoRepository.ObtenerPorIdAsync(item.PedidoId);

                // Comprueba el pedido exista ya en la base de datos y que hayan habido cambios que justifiquen la modificación.
                if (fromDb != null && item.Difiere(fromDb))
                {

                    // Corrobora que el VM contenga coherencia 
                    #region Copia para el Historial

                    // Crea una copia del pedido, seteando los Ids a cero
                    // dato que el pedido de historial es nuevo.
                    backArt = fromDb.Articulo.Copiar();
                    backArt.ArticuloId = 0;
                    backPed = fromDb.Copiar();
                    backPed.Articulo = backArt;
                    backPed.PedidoId = 0;
                    
                    // La copia del historial no debe poseer un registro de seguimientos.
                    backPed.SeguimientoGlobal = null;
                    
                    // Setea dicha copia como dada de baja y le marca su RegistroOriginal.
                    backPed.FechaBaja = fecha.Value;
                    backPed.RegistroOriginalId = fromDb.PedidoId;

                    // Inserta los datos.
                    controlador.UOW.ArticuloRepository.Insert(backArt);
                    controlador.UOW.PedidoRepository.Insert(backPed);

                    #endregion

                    // Copia los datos del VM en la entidad a modificar.
                    item.CopiarEn(fromDb);

                    // Comprueba si la dupla Muestrario / Laminado tiene coherencia.
                    await ValidarLaminado(controlador, new List<Pedido> { fromDb });

                    // Comprueba si el código Tango sigue teniendo validez en el pedido modificado.
                    if (!fromDb.ComprobarEstructura()) fromDb.EstructuraSolicitada = null;

                    // Graba los datos del pedido modificado en la base de datos.
                    if (controlador.Request.IsAuthenticated)
                    {

                        // Si el usuario es un cliente y si el pedido ya fue pasado a Producción,
                        // queda marcado para ser verificado de forma manual.
                        if (controlador.User.IsInRole("Cliente") &&
                            fromDb.SeguimientoGlobal.ConjuntoAtrasado.EtapaDelNegocioInterna.Nivel != 0)
                            fromDb.RequiereAprobacion = true;

                        // Graba el usuario que intervino en la modificación.
                        fromDb.UserName = controlador.User.Identity.Name;

                    }

                    // Guarda las modificaciones.
                    controlador.UOW.ArticuloRepository.Update(fromDb.Articulo);
                    controlador.UOW.PedidoRepository.Update(fromDb);

                }

            }

            // Termina.
            return true;

        }

        /// <summary>
        /// Da de baja los pedidos indicados.
        /// </summary>
        /// <param name="controlador">Controlador vigente al momento de llamar a esta función.</param>
        /// <param name="pedidos">Lista de Códigos de Pedido a dar de baja.</param>
        /// <param name="fechaBaja">Fecha con la que se marcarán las bajas correspondientes.</param>
        /// <param name="cascada">Indica si la función debe intentar dar de baja los elementos asociados.</param>
        public async Task<bool> Baja(UOWController controlador, int[] pedidos, DateTime? fechaBaja, Control.Propagacion propagacion = Control.Propagacion.HaciaAbajo | Control.Propagacion.HaciaArriba)
        {

            #region Inicializa Variables

            Articulo backArt;
            Pedido backPed;

            // Recupera el código de etapa interna de nivel 101 (dada de baja)
            var etapaDelNegocioInternaId = (await controlador.UOW.EtapaDelNegocioInternaRepository.ObtenerPorNivelAsync(101)).EtapaDelNegocioInternaId;
            
            // Recupera los pedidos pasados como argumentos desde la base de datos.
            var entidades = await controlador.UOW.PedidoRepository.ObtenerDesdeVectorAsync(pedidos);

            #endregion

            #region Comprobaciones Iniciales

            // Acomoda la variable "fechaBaja"
            fechaBaja = fechaBaja ?? DateTime.Now;

            // si no se encontraron pedidos, vuelve normalmente.
            if (entidades == null || entidades.Count() == 0) return true;

            #endregion

            #region Procesamiento de Pedidos

            // Recorre los pedidos a dar de baja.
            foreach (var entidad in entidades)
            {

                // Genera una copia del pedido para el historial y lo marca como dado de baja,
                // seteando los Ids a cero dado que la copia es en sí misma un nuevo pedido.
                entidad.FechaBaja = fechaBaja;
                backArt = entidad.Articulo.Copiar();
                backArt.ArticuloId = 0;
                backPed = entidad.Copiar();
                backPed.Articulo = backArt;
                backPed.PedidoId = 0;
                backPed.RegistroOriginalId = entidad.PedidoId;

                // La copia del historial no debe poseer registro de seguimientos.
                backPed.SeguimientoGlobal = null;

                // Graba en el pedido vigente el usuario que intervino.
                if (controlador.Request.IsAuthenticated) entidad.UserName = controlador.User.Identity.Name;

                // Genera los cambios.
                controlador.UOW.ArticuloRepository.Insert(backArt);
                controlador.UOW.PedidoRepository.Insert(backPed);
                controlador.UOW.PedidoRepository.Update(entidad);

            }

            #endregion

            #region Procesa Seguimientos (Hacia Abajo)

            // Procesa los seguimientos de los pedidos que se están dando de baja.
            if (propagacion.HasFlag(Control.Propagacion.HaciaAbajo))
            {
                using (Seguimientos seguimientosBusiness = new Seguimientos(controlador))
                {
                    await seguimientosBusiness.AfectaPedidosEnteros(pedidos, etapaDelNegocioInternaId, fechaBaja, Control.Propagacion.HaciaAbajo);
                }
            }

            #endregion

            #region Verifica Gestiones (Hacia Arriba)

            if (propagacion.HasFlag(Control.Propagacion.HaciaArriba))
            {

                using (GestionesBusiness gestionesBusiness = new GestionesBusiness())
                {

                    // Recorre los pedidos que se indicaron para darse de baja.
                    // Dichos pedidos están ya procesados, por lo tanto no aparecerán
                    // como pedidos activos de las gestiones a las que corresponden.
                    foreach (var entidad in entidades)
                    {

                        // Se comprueban nuevamente los registros para corroborar que no tengan bajas pendientes. Entity Framework recoge desde
                        // la base de datos y obtiene registros sin tener en cuenta las modificaciones que están pendientes en la
                        // transacción actual. En este caso, el pedido recientemente dado de baja continúa apareciendo y fuerza al sistema
                        // a hacer comprobaciones posteriores al pasaje entre IQueryable y IEnumerable.
                        //
                        // Si la gestión ya no posee pedidos activos,
                        if ((await controlador.UOW.PedidoRepository.ObtenerDesdeGestionAsync(entidad.GestionId)).Where(p => p.FechaBaja == null).Count() == 0)
                        {
                            // Procede a dar de baja dicha gestión.
                            await gestionesBusiness.Baja(controlador, entidad.GestionId, fechaBaja, Control.Propagacion.HaciaArriba);
                        }

                    }

                }

            }

            #endregion

            // Termina.
            return true;

        }

        /// <summary>
        /// Hace una primera comprobación de los campos correspondientes al muestrario y al laminado,
        /// que no puede llevar a cabo el motor de base de datos.
        /// </summary>
        /// <param name="pedidos">Lista de pedidos a corroborar y/o retocar.</param>
        public async Task<bool> ValidarLaminado(UOWController controlador, List<Pedido> pedidos)
        {

            #region Comprobaciones Previas

            // Filtra el listado de pedidos para procesar solamente los artículos del tipo "Tapa".
            var soloTapas = pedidos.Where(p => p.Articulo.TipoDeArticulo == "Tapa").ToList();
            if (soloTapas.Count == 0) return true;

            #endregion

            #region Inicializa Variables

            Tapa tapa;
            string mensajeDeExcepcion = "Uno de los códigos de muestrario y/o laminado es incorrecto.";

            // Recolecta el listado completo de muestrarios y laminados.
            var muestrarios = (await controlador.UOW.MuestrarioRepository.ObtenerTodosAsync()).OrderByDescending(m => m.Nivel);
            var laminados = (await controlador.UOW.LaminadoRepository.ObtenerTodosAsync()).OrderByDescending(l => l.Muestrario.Nivel);

            #endregion

            // Recorre cada una de las tapas de la lista.
            foreach (var pedido in soloTapas)
            {

                // Se pasa el artículo a una variable del tipo "Tapa"
                // para poder acceder a las propiedades específicas de este tipo de artículo.
                tapa = (Tapa)pedido.Articulo;
                
                // Si ambos campos de la clave compuesta son nulos, la clave compuesta en general es nula. Eso es correcto.
                // Si ambos campos de la clave compuesta contiene un valor, es el motor de base de datos el que decide su validez.
                // En ambos casos, no se analiza.

                // Si sólo un campo de la clave compuesta es nulo, debe analizarse.
                if (tapa.Laminado_MuestrarioId == null ^ tapa.Laminado_CodigoId == null)
                {
                    
                    // Si el muestrario es nulo, el sistema trata de ubicarlo.
                    if (tapa.Laminado_MuestrarioId == null)
                    {
                        
                        // Se buscan los laminados dentro de los muestrarios generales,
                        // o dentro del muestrario específico del cliente del pedido.
                        // Notar que el listado de laminados debe estar previamente ordenado de forma descendente por nivel.
                        // De los laminados resultantes se extrae el primero, que coincidirá con el que posee el código de muestrario de mayor nivel.
                        var busqueda = laminados
                                       .Where(l => l.Muestrario.ClienteId == null || l.Muestrario.ClienteId == pedido.Gestion.ClienteId)
                                       .Where(l => l.Laminado_CodigoId == tapa.Laminado_CodigoId)
                                       .FirstOrDefault();
                        
                        // Si no se encontró ningún laminado, devuelve error. El código de laminado no es válido.
                        if (busqueda == null) throw new Exception(mensajeDeExcepcion);
                        
                        // Si se encontró uno, se extrae el código de muestrario que se usará como muestrario del color de la tapa del pedido.
                        tapa.Laminado_MuestrarioId = busqueda.Laminado_MuestrarioId;
                    }
                    
                    // Si el código de laminado es nulo, se devuelve un error.
                    else
                    {
                        throw new Exception(mensajeDeExcepcion);
                    }

                }

            }

            return true;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Espacio para desechar objetos.
                    // Está en blanco sólo por cumplir con la interfaz IDisposable.
                }
            }
            this.disposed = true;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}