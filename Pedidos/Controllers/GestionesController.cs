using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Pedidos.DAL;
using Pedidos.Models;
using CustomExtensions;
using PagedList;
using System.Web.Routing;
using System.Collections.Generic;
using System.Globalization;
using Pedidos.ViewModels;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Diagnostics;
using Pedidos.ViewModels.Gestiones;

namespace Pedidos.Controllers
{
    [Authorize(Roles = "Webmaster,Gerencia")]
    public class GestionesController : UOWController
    {

        private GestionesBusiness business = new GestionesBusiness();

        public ActionResult Todas(string sort, string search, string searchIn, int? page, string output)
        {

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se deshabilita la creación de proxies por si se requiere serializar la salida.
            UOW.LazyLoadingEnabled = false;
            UOW.ProxyCreationEnabled = false;

            #endregion

            #region Obtiene Datos

            // Obtiene las gestiones desde la base de datos.
            var Gestiones = UOW.GestionRepository.ListarTodo(sort, search, searchIn, page, 15, userLanguage);

            #endregion

            var VM = new GestionesIndexVM();
            VM.Titulo = "Listado de Gestiones";
            VM.Controller = "Gestiones";
            VM.ActionMethod = "Todas";
            VM.Gestiones = Gestiones;

            return ProcesaSalidaDelListado(VM, sort, output);

        }

        [HttpPost]
        public ActionResult Todas(string newSearch, string searchIn)
        {
            return Index_ProcesaPOST(newSearch, searchIn, "Todas", "Gestiones");
        }

        public ActionResult PorCliente(string sort, string search, string searchIn, int? page, string output, string id, string accion)
        {

            #region Inicialización

            var solicitaDatoVM = new SolicitaDatoVM();
            solicitaDatoVM.Titulo = "Gestiones por Cliente";
            solicitaDatoVM.Jumbotron.Titulo = solicitaDatoVM.Titulo;
            solicitaDatoVM.Jumbotron.Descripción = "Presenta un listado de todas las gestiones activas de un determinado cliente.";
            solicitaDatoVM.BotonAlta.Disabled = false;
            solicitaDatoVM.BotonPrimario.Etiqueta = "Obtener";
            solicitaDatoVM.CuadroDeTexto.Placeholder = "Ingrese un Cliente";
            solicitaDatoVM.Origen = HttpContext.Request.RawUrl;
            solicitaDatoVM.CuadroDeTexto.Nombre = "id";
            solicitaDatoVM.Controlador = "Gestiones";
            solicitaDatoVM.Accion = "PorCliente";

            #endregion

            #region Comprobaciones Previas

            // Si es la primera vez que se ejecuta este ActionMethod,
            if (String.IsNullOrWhiteSpace(id) && String.IsNullOrWhiteSpace(accion))
            {
                // Presenta la pantalla de selección de cliente.
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Si viniendo de la pantalla de selección no se detecta un cliente ingresado,
            if (!String.IsNullOrWhiteSpace(accion) && String.IsNullOrWhiteSpace(id))
            {
                
                // Genera un error para dicho campo y vuelve a presentar la pantalla de selección.
                ModelState.AddModelError("Id", "Debe especificar un código de cliente.");
                return View("SolicitaDato", solicitaDatoVM);

            }
                
            // Si el cliente ingresado no es válido,
            if (UOW.ClienteRepository.ObtenerPorId(id) == null)
            {

                // Genera un error para dicho campo y vuelve a presentar la pantalla de selección.
                ModelState.AddModelError("Id", "El cliente ingresado no es válido.");
                solicitaDatoVM.Id = id;
                return View("SolicitaDato", solicitaDatoVM);

            }
            
            #endregion

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se deshabilita la creación de proxies por si se requiere serializar la salida.
            UOW.LazyLoadingEnabled = false;
            UOW.ProxyCreationEnabled = false;

            #endregion

            #region Obtiene Datos

            // Obtiene las gestiones desde la base de datos.
            var Gestiones = UOW.GestionRepository.ListarPorCliente(id, sort, search, searchIn, page, 15, userLanguage);

            #endregion

            var VM = new GestionesIndexVM();
            VM.Titulo = String.Format("Listado de gestiones del cliente {0}", id);
            VM.Controller = "Gestiones";
            VM.ActionMethod = "PorCliente";
            VM.ClienteId = id;
            VM.Gestiones = Gestiones;

            return ProcesaSalidaDelListado(VM, sort, output);

        }

        [HttpPost]
        public ActionResult PorCliente(string newSearch, string searchIn)
        {
            return Index_ProcesaPOST(newSearch, searchIn, "PorCliente", "Gestiones");
        }

        public ActionResult Historial(string sort, string search, string searchIn, int? page, string output, int? id, string accion)
        {

            #region Inicialización

            var solicitaDatoVM = new SolicitaDatoVM();
            solicitaDatoVM.Titulo = "Hitorial de Gestión";
            solicitaDatoVM.Jumbotron.Titulo = solicitaDatoVM.Titulo;
            solicitaDatoVM.Jumbotron.Descripción = "El sistema conserva una copia del original por cada registro que es sometido a un cambio. Este proceso lista el historial de modificaciones que corresponde a una determinada gestión.";
            solicitaDatoVM.BotonAlta.Disabled = true;
            solicitaDatoVM.BotonPrimario.Etiqueta = "Obtener";
            solicitaDatoVM.CuadroDeTexto.Placeholder = "Ingrese una Gestión";
            solicitaDatoVM.Origen = HttpContext.Request.RawUrl;
            solicitaDatoVM.CuadroDeTexto.Nombre = "id";
            solicitaDatoVM.Controlador = "Gestiones";
            solicitaDatoVM.Accion = "Historial";

            #endregion

            #region Comprobaciones Previas

            // Si es la primera vez que se ejecuta este ActionMethod,
            if (!id.HasValue && String.IsNullOrWhiteSpace(accion))
            {
                // Presenta la pantalla de selección de cliente.
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Si viniendo de la pantalla de selección no se detecta un cliente ingresado,
            if (!String.IsNullOrWhiteSpace(accion) && !id.HasValue)
            {

                // Genera un error para dicho campo y vuelve a presentar la pantalla de selección.
                ModelState.AddModelError("Id", "Debe especificar un código de gestión.");
                return View("SolicitaDato", solicitaDatoVM);

            }

            // Si el cliente ingresado no es válido,
            if (UOW.GestionRepository.ObtenerPorId(id.Value) == null)
            {

                // Genera un error para dicho campo y vuelve a presentar la pantalla de selección.
                ModelState.AddModelError("Id", "El código de gestión ingresado no es válido.");
                solicitaDatoVM.Id = id.Value;
                return View("SolicitaDato", solicitaDatoVM);

            }

            #endregion

            #region Inicializa Variables

            // Recupera el lenguaje de preferencia desde el objeto Request.
            var userLanguage = Request.UserLanguages[0];

            // Se deshabilita la creación de proxies por si se requiere serializar la salida.
            UOW.LazyLoadingEnabled = false;
            UOW.ProxyCreationEnabled = false;

            #endregion

            #region Obtiene Datos

            // Obtiene las gestiones desde la base de datos.
            var Gestiones = UOW.GestionRepository.ListarHistorial(id ?? 0, sort, search, searchIn, page, 15, userLanguage);

            #endregion

            var VM = new GestionesIndexVM();
            VM.Titulo = "Historial de la Gestión " + id.ToString();
            VM.Controller = "Gestiones";
            VM.ActionMethod = "Historial";
            VM.Gestiones = Gestiones;

            return ProcesaSalidaDelListado(VM, sort, output);

        }

        [HttpPost]
        public ActionResult Historial(string newSearch, string searchIn)
        {
            return Index_ProcesaPOST(newSearch, searchIn, "Historial", "Gestiones");
        }

        /// <summary>
        /// Recoge los datos de las gestiones a listar y los presenta según corresponda.
        /// </summary>
        private ActionResult ProcesaSalidaDelListado(GestionesIndexVM VM, string sort, string output)
        {
            
            // Elige la salida apropiada para los datos.
            switch (output ?? "")
            {

                // Exporta los registros a un archivo XML.
                case "xml":

                    return new XmlResult<List<Gestion>>(VM.Gestiones.ToList());

                // Lista los datos por pantalla.
                default:

                    // Setea el valor de los enlaces de ordenamiento.
                    switch (sort)
                    {
                        case "gestionid":
                            VM.SortLinks["gestionid"] = "gestionid_desc";
                            break;
                        case "username":
                            VM.SortLinks["username"] = "username_desc";
                            break;
                        case "observaciones":
                            VM.SortLinks["observaciones"] = "observaciones_desc";
                            break;
                        case "clienteid":
                            VM.SortLinks["clienteid"] = "clienteid_desc";
                            break;
                        default:
                            VM.SortLinks["fechagestion"] = "fechagestion_desc";
                            break;
                    }

                    return View("Listado", VM);

            }

        }

        /// <summary>
        /// Da curso a los requerimientos POST de las distintas pantallas de listados.
        /// </summary>
        private ActionResult Index_ProcesaPOST(string newSearch, string searchIn, string action, string controller)
        {
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
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Detalle(int? id, string VolverA)
        {
            
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var Gestion = await UOW.GestionRepository.ObtenerPorIdAsync(id);
            if (Gestion == null) return HttpNotFound();
            
            // Mapeo
            GestionesDetalleVM VM = new GestionesDetalleVM();
            VM.ClienteId = Gestion.ClienteId;
            VM.GestionId = Gestion.GestionId;
            VM.FechaGestion = Gestion.FechaGestion;
            VM.Observaciones = Gestion.Observaciones;
            VM.UserName = Gestion.UserName;
            VM.VolverA = VolverA;

            return View(VM);

        }

        public ActionResult Alta(string clienteId, string accion, string volverA)
        {
            
            #region Inicialización

            var solicitaDatoVM = new SolicitaDatoVM();
            solicitaDatoVM.Accion = "Alta";
            solicitaDatoVM.Controlador = "Gestiones";
            solicitaDatoVM.BotonAlta.Disabled = true;
            solicitaDatoVM.BotonPrimario.Etiqueta = "Confirmar";
            solicitaDatoVM.CuadroDeTexto.Nombre = "clienteId";
            solicitaDatoVM.Titulo = "Alta de Gestión";
            solicitaDatoVM.CuadroDeTexto.Placeholder = "Ingrese un Cliente";
            solicitaDatoVM.Origen = String.IsNullOrWhiteSpace(volverA) ? Url.Action("Index") : volverA;
            solicitaDatoVM.Jumbotron.Titulo = solicitaDatoVM.Titulo;
            solicitaDatoVM.Jumbotron.Descripción = "Genera el alta de una nueva gestión para un determinado cliente.";

            #endregion

            #region Comprobaciones Previas

            // Si no está especificado un código de cliente,
            if (String.IsNullOrWhiteSpace(clienteId))
            {
                // Presenta la pantalla de selección.
                if (!String.IsNullOrWhiteSpace(accion)) ModelState.AddModelError("Id", "El campo debe contener un código de cliente válido.");
                return View("SolicitaDato", solicitaDatoVM);
            }

            // Si el cliente no es válido,
            if (UOW.ClienteRepository.ObtenerPorId(clienteId) == null)
            {
                // Muestra la pantalla de selección
                // con el mensaje de error correspondiente.
                solicitaDatoVM.Id = clienteId;
                ModelState.AddModelError("Id", "El cliente especificado no es válido.");
                return View("SolicitaDato", solicitaDatoVM);
            }

            #endregion

            var gestionesAltaVM = new GestionesAltaVM();
            gestionesAltaVM.FechaGestion = DateTime.Now;
            gestionesAltaVM.VolverA = volverA ?? Url.Action("Index");
            gestionesAltaVM.ClienteId = clienteId;
            return View(gestionesAltaVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Alta(GestionesAltaVM VM)
        {
            
            var Gestion = new Gestion();
            
            if (ModelState.IsValid)
            {
                
                if (Request.IsAuthenticated) Gestion.UserName = User.Identity.Name;

                Gestion.ClienteId = VM.ClienteId;
                Gestion.FechaGestion = VM.FechaGestion;
                Gestion.Observaciones = VM.Observaciones;

                try
                {
                    UOW.GestionRepository.Insert(Gestion);
                    await UOW.SaveChangesAsync();
                    return Redirect(VM.VolverA);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(String.Empty, "Error al agregar el contacto. Verifique nuevamente en unos instantes.");
                }

            }

            if (Gestion.FechaGestion == new DateTime(0)) Gestion.FechaGestion = DateTime.Today;
            
            return View(VM);

        }

        public async Task<ActionResult> Modificacion(int? id, string VolverA)
        {
            
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            Gestion Gestion = await UOW.GestionRepository.ObtenerPorIdAsync(id);
            if (Gestion == null) return HttpNotFound();
            
            var VM = new GestionesModificacionVM();
            VM.GestionId = Gestion.GestionId;
            VM.FechaGestion = Gestion.FechaGestion;
            VM.Observaciones = Gestion.Observaciones;
            VM.RowVersion = Gestion.RowVersion;
            VM.VolverA = VolverA;
            
            return View(VM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Modificacion()
        {
            
            var VM = new GestionesModificacionVM();
            
            if (TryUpdateModel(VM, includeProperties: new[] { "GestionId", "FechaGestion", "Observaciones", "RowVersion" }))
            {
                
                var Gestion = await UOW.GestionRepository.ObtenerPorIdAsync(VM.GestionId);
                var Copia = Gestion.GetCopy();
                Gestion.UserName = Request.IsAuthenticated ? User.Identity.Name : null;
                if (Gestion.RegistroOriginalId == null) Copia.RegistroOriginalId = Gestion.GestionId;
                Gestion.FechaGestion = VM.FechaGestion;
                Gestion.Observaciones = VM.Observaciones;
                Copia.FechaBaja = DateTime.Now;

                try
                {
                    UOW.GestionRepository.Update(Gestion);
                    UOW.GestionRepository.Insert(Copia);
                    await UOW.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("Error al grabar", "Se ha producido un error al grabar los cambios.");
                }

            }

            return View(VM);

        }

        public async Task<ActionResult> Baja(int? Id, string VolverA)
        {
            
            // Si no se especificó un Id, presenta un error.
            if (Id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Intenta obtener la Gestión mediante su Id.
            var fromDb = await UOW.GestionRepository.ObtenerPorIdAsync(Id ?? 0);

            // Si el Id resultó ser incorrecto, presenta un error.
            if (fromDb == null) return HttpNotFound();
            
            // Mapeo
            GestionesDetalleVM VM = new GestionesDetalleVM();
            VM.ClienteId = fromDb.ClienteId;
            VM.GestionId = fromDb.GestionId;
            VM.FechaGestion = fromDb.FechaGestion;
            VM.Observaciones = fromDb.Observaciones;
            VM.UserName = fromDb.UserName;
            VM.VolverA = VolverA;

            // Presenta la pantalla de confirmación.
            return View(VM);

        }

        [HttpPost, ActionName("Baja")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BajaConfirmada(GestionesDetalleVM VM)
        {
            
            // Si se pudo construir correctamente el ViewModel,
            if (ModelState.IsValid)
            {
                
                // Procesa los datos.
                try
                {
                    await business.Baja(this, VM.GestionId, DateTime.Now, Control.Propagacion.HaciaAbajo);
                    UOW.SaveChanges();
                }

                // Capta cualquier error producido en el bloque "try".
                catch (Exception)
                {
                    ModelState.AddModelError(String.Empty, "No se pudo dar de baja el registro. Intenta nuevamente en unos instantes o compruebe que el mismo no haya sido dado de baja en el medio del proceso.");
                    return View("Baja", VM);
                }

                // El proceso se realizó correctamente.
                // Regresa.
                return Redirect(VM.VolverA);

            }
            
            // No pudo armarse el VM con los datos rescatados del formulario.
            // Vuelve a presentar la pantalla de confirmación.
            ModelState.AddModelError(String.Empty, "Ocurrió un error en el pasaje entre pantallas. Intentá nuevamente en unos segundos o reintentá la operación desde cero.");
            return View("Baja", VM);

        }

    }

    public class GestionesBusiness : IDisposable
    {

        private bool disposed = false;
        
        /// <summary>
        /// Da de baja la gestión indicada.
        /// </summary>
        /// <param name="controlador">Controlador vigente al momento de llamar a esta función.</param>
        /// <param name="gestionId">Código de la gestión que se quiere dar de baja.</param>
        /// <param name="fechaBaja">Fecha con la que se marcarán las bajas correspondientes.</param>
        /// <param name="cascada">Indica si la función debe intentar dar de baja los elementos asociados.</param>
        public async Task<bool> Baja(UOWController controlador, int gestionId, DateTime? fechaBaja, Control.Propagacion propagacion = Control.Propagacion.HaciaAbajo)
        {

            #region Inicializa Variables

            Gestion backGestion;
            var pedidosBusiness = new PedidosBusiness();
            var gestion = await controlador.UOW.GestionRepository.ObtenerPorIdAsync(gestionId);
            var etapaDelNegocioInternaId = (await controlador.UOW.EtapaDelNegocioInternaRepository.ObtenerPorNivelAsync(101)).EtapaDelNegocioInternaId;

            #endregion

            #region Comprobaciones Previas

            // Acomoda la variable "fechaBaja".
            fechaBaja = fechaBaja ?? DateTime.Now;

            // Si la gestión no existe o ya fue dada de baja, vuelve normalmente.
            //
            // Nota:
            // Puede pasar que el pedido ya contenga modificaciones en la transacción en curso
            // y que aún resten guardarse los cambios.
            // Entity Framework busca en el contenido original de las entidades y devuelve los resultados,
            // aún cuando hayan cambios pendientes que alteren ese contenido original.
            //
            if (gestion == null || gestion.FechaBaja != null) return true;

            #endregion

            #region Procesa la Gestión

            // Se marca el registro activo como dado de baja.
            // Debe estar acá para que luego la copia del mismo quede con el mismo valor.
            gestion.FechaBaja = fechaBaja;
            backGestion = gestion.GetCopy();
            if (gestion.RegistroOriginalId == null) backGestion.RegistroOriginalId = gestion.GestionId;
            controlador.UOW.GestionRepository.Insert(backGestion);
            gestion.UserName = controlador.Request.IsAuthenticated ? controlador.User.Identity.Name : null;
            controlador.UOW.GestionRepository.Update(gestion);

            #endregion

            #region Procesa Pedidos (Hacia Abajo)

            if (propagacion.HasFlag(Control.Propagacion.HaciaAbajo))
            {

                // Busca todos los pedidos aún activos de la gestión a dar de baja.
                var Pedidos = (await controlador.UOW.PedidoRepository.ObtenerDesdeGestionAsync(gestionId)).Select(p => p.PedidoId).ToArray();

                // Si se encontró alguno, los da de baja.
                if (Pedidos != null && Pedidos.Length > 0) await pedidosBusiness.Baja(controlador, Pedidos, fechaBaja, Control.Propagacion.HaciaAbajo);

            }

            #endregion

            // Finaliza.
            return true;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Desechar objetos.
                    // En este caso no hay ninguno. Sólo se emplea para cumplir con la interfaz IDisposable.
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
