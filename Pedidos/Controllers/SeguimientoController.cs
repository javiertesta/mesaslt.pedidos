using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Pedidos.Models;
using Pedidos.ViewModels.Seguimiento;
using System.Net;

namespace Pedidos.Controllers
{
    [Authorize(Roles = "Webmaster,Gerencia,Jefe")]
    [RoutePrefix("seguimiento")]
    public class SeguimientoController : UOWController
    {

        private Seguimientos business;

        public SeguimientoController()
        {
            business = new Seguimientos(this);
        }

        [Route("~/seguimiento", Name = "Seguimiento")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("porlistado", Name = "SeguimientoPorListado")]
        public async Task<ActionResult> PorListado_Paso1_Seleccion(int? id)
        {

            // Estructura los datos que se pasarán al VM.
            var pedidosListadosPDF = new List<SelectListItem>();
            (await UOW.PedidosListadosRepository.ObtenerTodosAsync()).OrderByDescending(pl => pl.Fecha).ToList().ForEach(pl =>
            {
                var item = new SelectListItem { Text = pl.Fecha.ToString("dd-MM-yyyy HH:mm"), Value = pl.Id.ToString() };
                pedidosListadosPDF.Add(item);
            });

            // Si no hay ningún informe guardado, crea un ítem que indica que la lista está vacía.
            if (pedidosListadosPDF.Count == 0) pedidosListadosPDF.Add(new SelectListItem { Text = "Vacío", Value = "0" });

            ViewBag.ListaDeListadosPDF = new SelectList(pedidosListadosPDF, "Value", "Text", pedidosListadosPDF[0]);

            // Si no se especificó ningún informe, muestra la pantalla de selección.
            if (!id.HasValue || id.Value == 0)
            {
                return View("PorListado");
            }

            // Si se especificó un informe pero el mismo no existe, muestra la pantalla de selección.
            if (await UOW.PedidosListadosRepository.ObtenerPorIdAsync(id) == null)
            {
                return View("PorListado");
            }

            // El informe especificado es finalmente correcto.
            // Pasa a la etapa siguiente.
            return RedirectToRoute("SeguimientoPorListado_Paso2_Analisis", new { @id = id });

        }

        [Route("porlistado/analisis", Name = "SeguimientoPorListado_Paso2_Analisis")]
        public async Task<ActionResult> PorListado_Paso2_Analisis(int id)
        {

            #region ComprobacionesPrevias

            // Si no se especificó ningún código de informe, muestra la pantalla de selección.
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Recupera los datos del mismo.
            PedidoListado informePDF = await UOW.PedidosListadosRepository.ObtenerPorIdAsync(id);

            // Si el código del informe es erroneo, devuelve un error.
            if (informePDF == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // La lista de pedidos asociados al listado se encuentra en una cadena separados por comas.
            // Pasa dicha lista a un vector.
            char[] delimiters = { ',' };
            var split = informePDF.Pedidos.Replace(" ", String.Empty).Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Convierte las cadenas a números enteros y genera un vector de ese tipo.
            var codigosDePedido = new List<int>();
            split.ForEach(item =>
            {
                short number;
                if (Int16.TryParse(item, out number)) codigosDePedido.Add(number);
            });

            // Si alguna cadena no pudo ser convertida a un número entero, muestra un error.
            if (split.Count != codigosDePedido.Count)
            {
                throw new Exception("Alguno de los códigos de pedido no pudo ser debidamente rescatado. No se puede continuar con el procesamiento del informe.");
            }

            #endregion

            #region VerificaPedidos

            var detallesDetectados = new Dictionary<int, Tuple<Seguimientos.DetallesDetectados, string>>();
            var puedeContinuar = await business.AnalizaPedidos(codigosDePedido, detallesDetectados, informePDF.Fecha);

            #endregion

            #region AnalizaResultado

            // Si luego de verificar cada pedido aún quedan items para procesar en la etapa siguiente,
            if (puedeContinuar)
            {

                // Si se detectaron detalles o fallas en el análisis anterior,
                // muestra una pantalla con el detalle del análisis.
                if (detallesDetectados.Count > 0)
                {
                    ViewBag.DetallesDetectados = detallesDetectados.Values.Select(tuple => tuple.Item2).ToList();
                    ViewBag.Id = informePDF.Id;
                    return View("PorListado_Paso2");
                }

                // Si no hubieron fallas o detalles de observación para mencionar,
                // pasa a la siguiente etapa.
                else
                {
                    return RedirectToRoute("SeguimientoPorListado_Paso3_Cantidades", new { @id = id });
                }

            }

            // Si todos los pedidos del listado contenían detalles,
            // el proceso termina aquí.
            else
            {
                throw new Exception("Después de filtrar los pedidos con detalles no resta ningún pedido para procesar.");
            }

            #endregion

        }

        [Route("porlistado/cantidades", Name = "SeguimientoPorListado_Paso3_Cantidades")]
        public async Task<ActionResult> PorListado_Paso3_Cantidades(int id)
        {

            if (id == 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            PedidoListado informePDF = await UOW.PedidosListadosRepository.ObtenerPorIdAsync(id);
            if (informePDF == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return await Paso3_Cantidades(informePDF.Pedidos);

        }

        [Route("porpedido", Name = "SeguimientoPorPedido")]
        public ActionResult Paso1_PorPedido_Seleccion(string ids)
        {
            if (String.IsNullOrEmpty(ids))
            {
                return View("PorPedido");
            }
            else
            {
                return RedirectToRoute("SeguimientoPorPedido_Paso2_Analisis", new { @ids = ids });
            }
        }

        [Route("porpedido/analisis", Name = "SeguimientoPorPedido_Paso2_Analisis")]
        public async Task<ActionResult> PorPedido_Paso2_Analisis(string ids)
        {

            #region ComprobacionesPrevias

            // Pasa al VM la URL de origen del proceso.
            ViewBag.IDs = ids;

            // Si no se especificó ningún código de informe, muestra la pantalla de selección.
            if (String.IsNullOrWhiteSpace(ids))
            {
                return RedirectToRoute("SeguimientoPorPedido");
            }

            // La lista de pedidos asociados al listado se encuentra en una cadena separados por comas.
            // Pasa dicha lista a un vector.
            char[] delimiters = { ',' };
            var split = ids.Replace(" ", String.Empty).Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Convierte las cadenas a números enteros y genera un vector de ese tipo.
            var codigosDePedido = new List<int>();
            split.ForEach(item =>
            {
                short number;
                if (Int16.TryParse(item, out number)) codigosDePedido.Add(number);
            });

            // Si alguna cadena no pudo ser convertida a un número entero, muestra un error.
            if (split.Count != codigosDePedido.Count)
            {
                throw new Exception("Alguno de los códigos de pedido no pudo ser debidamente rescatado. No se puede continuar con el procesamiento del informe.");
            }

            #endregion

            #region VerificaPedidos

            var detallesDetectados = new Dictionary<int, Tuple<Seguimientos.DetallesDetectados, string>>();
            var puedeContinuar = await business.AnalizaPedidos(codigosDePedido, detallesDetectados);

            #endregion

            #region AnalizaResultado

            // Si se detectaron detalles o fallas en el análisis anterior,
            // muestra una pantalla con el detalle del análisis.
            if (detallesDetectados.Count > 0)
            {
                ViewBag.DetallesDetectados = detallesDetectados.Values.Select(tuple => tuple.Item2).ToList();
                return View("PorPedido_Paso2");
            }

            // Si no hubieron fallas o detalles de observación para mencionar,
            // pasa a la siguiente etapa.
            else
            {
                return RedirectToRoute("SeguimientoPorPedido_Paso3_Cantidades", new { @ids = ids });
            }

            #endregion

        }

        [Route("porpedido/cantidades", Name = "SeguimientoPorPedido_Paso3_Cantidades")]
        public async Task<ActionResult> PorPedido_Paso3_Cantidades(string ids)
        {
            return await Paso3_Cantidades(ids);
        }

        [Route("porcliente", Name = "SeguimientoPorCliente")]
        public async Task<ActionResult> PorCliente_Paso1_Seleccion(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return View("PorCliente");
            }
            else
            {
                if (await _UOW.ClienteRepository.ObtenerPorIdAsync(id) != null)
                {
                    return RedirectToRoute("SeguimientoPorCliente_Paso3_Cantidades", new { @id = id} );
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "El cliente no existe.");
                    return View("PorCliente");
                }
            }
        }

        [Route("porcliente/cantidades", Name = "SeguimientoPorCliente_Paso3_Cantidades")]
        public async Task<ActionResult> PorCliente_Paso3_Cantidades(string id)
        {
            // Comprueba que el Id pertenezca a un cliente existente.
            if (await _UOW.ClienteRepository.ObtenerPorIdAsync(id) == null)
            {
                ModelState.AddModelError(String.Empty, "El cliente es incorrecto.");
                return View("PorCliente");
            }

            // Extrae todos los pedidos activos del cliente.
            var pedidosDelCliente = await _UOW.PedidoRepository.ObtenerDesdeClienteAsync(id);

            // Comprueba que el Id pertenezca a un cliente existente.
            if (pedidosDelCliente == null)
            {
                ModelState.AddModelError(String.Empty, "El cliente no contiene pedidos activos.");
                return View("PorCliente");
            }

            return await Paso3_Cantidades(String.Join(",", pedidosDelCliente.Select(p => p.PedidoId).ToArray()));
        }

        [HttpPost]
        [Route("cantidades", Name = "Seguimiento_Paso3")]
        public async Task<ActionResult> Paso3_Cantidades(string ids)
        {

            #region Comprobaciones Previas

            // La lista de pedidos asociados al listado se encuentra en una cadena separados por comas.
            // Pasa dicha lista a un vector.
            char[] delimiters = { ',' };
            var split = ids.Replace(" ", String.Empty).Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (split == null || split.Length == 0) throw new Exception("Los datos ingresados no son válidos. Recuerde que debe ingresar una lista de números de pedido separados por comas.");

            // Convierte las cadenas a números enteros y genera un vector de ese tipo.
            var listaPedidos = new List<int>();
            split.ToList().ForEach(item =>
            {
                short number;
                if (Int16.TryParse(item, out number)) listaPedidos.Add(number);
            });

            // Si alguna cadena no pudo ser convertida a un número entero, muestra un error.
            if (split.Length != listaPedidos.Count) throw new Exception("Alguno de los pedidos no pudo ser procesado. Recuerde que debe ingresar una lista de números de pedido separados por comas.");

            #endregion

            // El parámetro se pasa vacío porque ya no tiene uso.
            var VM = new SeleccionaCantidadesVM(String.Empty);

            // Obtiene los pedidos.
            _UOW.PedidoRepository.IncluyeHistorial = true;
            _UOW.PedidoRepository.IncluyeDadosDeBaja = true;
            var pedidos = await _UOW.PedidoRepository.ObtenerDesdeVectorAsync(listaPedidos.ToArray());
            _UOW.PedidoRepository.Reset();

            if (pedidos == null) throw new Exception("No se pudo obtener ninguno de los pedidos solicitados desde la base de datos.");

            foreach (var p in pedidos)
            {
                p.SeguimientoGlobal.Detalle = (await _UOW.SeguimientoIndividualRepository.ObtenerDesdePedidoAsync(p.PedidoId)).ToList();
                VM.PedidosEtapa1.Add(new SeleccionaCantidadesVM_Pedido { ClienteId = p.Gestion.ClienteId, PedidoId = p.PedidoId, Leyenda = p.ToString(), CantidadSeleccionada = p.SeguimientoGlobal.CantidadPendiente });
            }

            return View("Paso3_SeleccionaCantidades", VM);

        }

        [HttpPost]
        [Route("nuevadistribucion", Name = "Seguimiento_Paso4_Redistribucion")]
        public async Task<ActionResult> Paso4_Redistribucion()
        {
            var VMin = new SeleccionaCantidadesVM();
            var VMout = new AplicaSeguimientosVM();
            AplicaSeguimientosVM_Pedido item;
            
            // Traspasa los datos del formulario al VM.
            if (!TryUpdateModel(VMin))
            {
                // Hubo un error al cargar el ViewModel. Se genera un error y se devuelve a la pantalla de edición.
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al actualizar los datos. Por favor revise lo ingresado y reintente nuevamente.");
                return View("Paso3_SeleccionaCantidades", VMin);
            }

            if (VMin.PedidosEtapa1.Where(i => i.CantidadSeleccionada == 0).Count() == VMin.PedidosEtapa1.Count)
            {
                // Hubo un error al cargar el ViewModel. Se genera un error y se devuelve a la pantalla de edición.
                ModelState.AddModelError(String.Empty, "Todos los ítems están en cero. Al menos uno debe poseer una cantidad positiva.");
                return View("Paso3_SeleccionaCantidades", VMin);
            }

            // Inicializa el VM de salida con los datos del de entrada.
            VMout.NuevaEtapa = VMin.NuevaEtapa;

            // Inicializa los Pedidos del nuevo VM.
            VMin.PedidosEtapa1.ForEach(i =>
            {
                VMout.PedidosEtapa2.Add(new AplicaSeguimientosVM_Pedido
                {
                    Cantidad = i.CantidadSeleccionada,
                    ClienteId = i.ClienteId,
                    Leyenda = i.Leyenda,
                    PedidoId = i.PedidoId
                });
            });

            // Borra del VM los ítems que vienen del VM de entrada
            // y quedaron con CantidadAAfectar en cero,
            // que no serán afectado por las modificaciones.
            for (var i = VMout.PedidosEtapa2.Count - 1; i >= 0; i--)
            {
                if (VMout.PedidosEtapa2[i].Cantidad < 1) VMout.PedidosEtapa2.RemoveAt(i);
            }

            // Pasa el listado de los pedidos a ser modificados a un vector.
            int[] PedidosVector = VMout.PedidosEtapa2
                .Select(i => i.PedidoId)
                .ToArray();

            bool Error = false;

            // Verifica que las cantidades rescatadas no superen la cantidad total del pedido.
            _UOW.PedidoRepository.IncluyeHistorial = true;
            _UOW.PedidoRepository.IncluyeDadosDeBaja = true;
            var PedidosDesdeBD = await _UOW.PedidoRepository.ObtenerDesdeVectorAsync(PedidosVector);
            _UOW.PedidoRepository.Reset();

            foreach (var p in PedidosDesdeBD)
            {
                item = VMout.PedidosEtapa2.Where(it => it.PedidoId == p.PedidoId).FirstOrDefault();
                if (item.Cantidad < 0 || item.Cantidad > p.Cantidad) Error = true;
            }
            
            if (Error)
            {
                ModelState.AddModelError(String.Empty, "Alguna de las cantidades supera la cantidad total pedida. El valor debe estar entre 1 y dicha cantidad");
                return View("Paso3_SeleccionaCantidades", VMin);
            }

            // Carga el VM con los datos de los seguimientos internos.
            var siDesdeBD = await _UOW.SeguimientoIndividualRepository.ObtenerDesdeVectorDePedidosAsync(PedidosVector);

            foreach (var si in siDesdeBD)
            {
                item = VMout.PedidosEtapa2.Where(i => i.PedidoId == si.SeguimientoGlobal.Pedido.PedidoId).FirstOrDefault();
                item.Seguimientos.Add(new AplicaSeguimientosVM_Seguimiento
                {
                    SeguimientoIndividualId = si.SeguimientoIndividualId,
                    CantidadAfectada = (si.SeguimientoGlobal.Detalle.Where(sg => sg.FechaBaja == null).Count() == 1 ? item.Cantidad : 0),
                    CantidadActual = si.Cantidad,
                    EtapaDelNegocioInternaId = si.EtapaDelNegocioInterna.EtapaDelNegocioInternaId
                });
            }

            // Setea las cantidades sólo en los ítems que tienen un sólo ítem de seguimiento interno.
            // Seteadas estas cantidades, servirán para saber que ese ítem aparecerá deshabilitado
            return View(VMout);
        }

        [HttpPost]
        [Route("aplicarcambios", Name = "Seguimiento_Paso5_AplicaCambios")]
        public async Task<ActionResult> Paso5_AplicaCambios()
        {

            var VM = new AplicaSeguimientosVM();

            // Traspasa los datos del formulario al ViewModel
            if (!TryUpdateModel(VM))
            {
                // Hubo un error al cargar el ViewModel. Se genera un error y se devuelve a la pantalla de edición.
                ModelState.AddModelError(String.Empty, "Ha ocurrido un error al actualizar los datos. Por favor revise lo ingresado y reintente nuevamente.");
                return View("Paso4_Redistribucion", VM);
            }
            
            try 
	        {
                await business.RedistribuyeSeguimientos(VM, null);
                UOW.SaveChanges();
	        }
	        catch (Exception e)
	        {
                ModelState.AddModelError(String.Empty, String.Format("Se ha producido un error al intentar guardar los cambios. [{0}]", e.Message));
                return View("Paso4_Redistribucion", VM);
	        }
            
            // Termina el proceso.
            return RedirectToAction("Index", "Home");

        }

    }

    public class Seguimientos : IDisposable
    {

        private bool disposed = false;
        private UOWController _controlador;
        
        public enum DetallesDetectados
        {
            PedidoNoEncontrado = 1,
            PedidoDadoDeBaja = 2,
            SeguimientoModificado = 4,
            PedidoModificado = 8
        }

        public Seguimientos(UOWController unitOfWork)
        {
            this._controlador = unitOfWork;
        }

        /// <summary>
        /// Plasma las redistribuciones de seguimientos generadas en procesos anteriores.
        /// </summary>
        /// <param name="controlador">Controlador vigente al momento de llamar a esta función.</param>
        /// <param name="VM">ViewModel que contiene las nuevas redistribuciones de seguimiento para uno o más pedidos.</param>
        /// <param name="fecha">Especifica la fecha y la hora a utilizar en la actualización de los datos.</param>
        /// <param name="cascada">Indica si la función debe intentar dar de baja los elementos asociados.</param>
        public async Task<bool> RedistribuyeSeguimientos(AplicaSeguimientosVM VM, DateTime? fecha, Control.Propagacion propagacion = Control.Propagacion.HaciaArriba)
        {

            #region Inicia el Proceso

            fecha = fecha ?? DateTime.Now;
            bool Errores = false;
            
            // Pasa a un vector todos los Ids de las etapas que no indican "baja" o "entregado"
            var eniEtapasActivas = (await _controlador.UOW.EtapaDelNegocioInternaRepository.ObtenerAsync(eni => eni.Nivel < 100))
                .Select(eni => eni.EtapaDelNegocioInternaId)
                .ToArray();

            // Obtiene el código de la etapa interna "dada de baja"
            var eniEtapaDadaDeBaja = (await _controlador.UOW.EtapaDelNegocioInternaRepository.ObtenerPorNivelAsync(101)).EtapaDelNegocioInternaId;

            // Pasa a un vector todos los pedidos que se solicita sean afectados por el nuevo seguimiento.
            int[] vectorDePedidos = VM.PedidosEtapa2.Where(i => i.Cantidad > 0).Select(i => i.PedidoId).ToArray();

            // Recupera las entidades desde la base de datos.
            _controlador.UOW.PedidoRepository.IncluyeHistorial = true;
            _controlador.UOW.PedidoRepository.IncluyeDadosDeBaja = true;
            var pedidos = await _controlador.UOW.PedidoRepository.ObtenerDesdeVectorAsync(vectorDePedidos);
            _controlador.UOW.PedidoRepository.Reset();

            #endregion

            #region Comprobaciones Previas

            // Si ningún pedido tenía cantidad alguna para afectar al nuevo seguimiento
            // o no se encontró ningún pedido en el VM,
            //
            // The important thing to note about the code below is that in csharp, the or operator (||) is short circuiting.
            // It sees that the array is null (statement is true) and does NOT attempt to evaluate the second statement.
            // If it did, you'd still get a NullReferenceException and would have to nest your if statements.
            // Not all languages are like this, VB.Net being one example.
            // The default Or operator is NOT short-circuiting; they have OrElse for that.
            //
            if (vectorDePedidos == null || vectorDePedidos.Length == 0) return true;

            // Verifica que la cantidad indicada para ser afectada al nuevo seguimiento
            // no supere la cantidad total del pedido en la base de datos.
            pedidos.ToList().ForEach(p =>
            {
                var item = VM.PedidosEtapa2.Where(it => it.PedidoId == p.PedidoId).FirstOrDefault();
                if (item.Cantidad > p.Cantidad) Errores = true;
            });

            // Si se detectaron errores en el paso anterior,
            if (Errores) throw new Exception("Al menos uno de los pedidos posee indicada una cantidad para ser afectada al nuevo seguimiento que resulta ser mayor a la cantidad total del mismo.");

            // Verifica que la cantidad global a afectar al nuevo seguimiento sea coherente con la distribución de la misma,
            // comprobando que la suma de los distintos seguimientos indicados para el pedido
            // sea igual a la cantidad global indicada en el paso previo.
            foreach (var pedidoEnVM in VM.PedidosEtapa2)
            {
                if (pedidoEnVM.Cantidad != pedidoEnVM.Seguimientos.Select(seg => seg.CantidadAfectada).Sum())
                {
                    var e = new Exception("La distribución de las cantidades elegidas para afectar al nuevo seguimiento supera la cantidad total escogida previamente.");
                    throw e;
                }
            }

            #endregion

            #region Procesa Seguimientos

            // Se crea un contenedor para todas las distribuciones finales generadas,
            // para todos los pedidos.
            var nuevosSeguimientos = new List<SeguimientoIndividual>();

            // Recorre los pedidos que se encuentran en el ViewModel.
            foreach (var PedidoEnProceso in VM.PedidosEtapa2)
            {
                
                // Recorre el actual detalle de seguimientos, restando las cantidades
                // que se indicaron pasarán a la nueva etapa.
                foreach (var SeguimientoIndividualEnProceso in PedidoEnProceso.Seguimientos)
                {
                    // Si se indicó que el conjunto entero pase a la nueva etapa, entonces
                    // el ítem en cuestión quedará con cantidad cero, lo que deriva en que
                    // no se agregue a la nueva configuración de seguimiento.
                    // Si se afectó sólo una cantidad parcial a la nueva etapa, entonces
                    // se agrega el resto a la nueva configuración de seguimiento.
                    if (SeguimientoIndividualEnProceso.CantidadResultante != 0)
                    {
                        var NuevoSeguimiento = new SeguimientoIndividual(SeguimientoIndividualEnProceso.CantidadResultante, _controlador.User);
                        NuevoSeguimiento.EtapaDelNegocioInternaId = SeguimientoIndividualEnProceso.EtapaDelNegocioInternaId;
                        NuevoSeguimiento.Fecha = fecha.Value;
                        NuevoSeguimiento.SeguimientoGlobalId = PedidoEnProceso.PedidoId;
                        nuevosSeguimientos.Add(NuevoSeguimiento);
                    }
                }

                // Analiza la nueva etapa. La misma puede haber sido tratada en el proceso anterior,
                // por lo que quizás ya esté dada de alta como SeguimientoIndividual.
                // Si la etapa a agregar no existe aún en los seguimientos previamente cargados,
                if (nuevosSeguimientos.Where(ns => ns.SeguimientoGlobalId == PedidoEnProceso.PedidoId && ns.EtapaDelNegocioInternaId == VM.NuevaEtapa).Count() == 0)
                {
                    // Se agrega a la nueva configuración de seguimiento.
                    var nuevoSeguimiento = new SeguimientoIndividual(PedidoEnProceso.Cantidad, _controlador.User);
                    nuevoSeguimiento.EtapaDelNegocioInternaId = VM.NuevaEtapa ?? 0;
                    nuevoSeguimiento.Fecha = fecha.Value;
                    nuevoSeguimiento.SeguimientoGlobalId = PedidoEnProceso.PedidoId;
                    nuevosSeguimientos.Add(nuevoSeguimiento);
                    
                }

                // Si la etapa a agregar ya fue adicionada en el paso anterior,
                else
                {
                    // Se suma la cantidad que quedó determinada en el proceso anterior con
                    // la cantidad indicada a afectar a la nueva etapa.
                    var SeguimientoAModificar = nuevosSeguimientos.Where(ns => ns.SeguimientoGlobalId == PedidoEnProceso.PedidoId && ns.EtapaDelNegocioInternaId == VM.NuevaEtapa).FirstOrDefault();
                    SeguimientoAModificar.Cantidad = SeguimientoAModificar.Cantidad + PedidoEnProceso.Cantidad;
                }

                // Si el usuario actual es un cliente,
                if (_controlador.User.IsInRole("Cliente"))
                {
                    // Y si la acción a realizar es marcar una parte del pedido (o el pedido entero) como dado de baja,
                    if (VM.NuevaEtapa == eniEtapaDadaDeBaja)
                    {
                        // Alerta a Administración para que verifique el mismo.
                        var entityToUpdate = await _controlador.UOW.PedidoRepository.ObtenerPorIdAsync(PedidoEnProceso.PedidoId);
                        entityToUpdate.RequiereAprobacion = true;
                        _controlador.UOW.PedidoRepository.Update(entityToUpdate);
                    }
                }

                // Se dan de baja los seguimientos actuales.
                var siAMarcar = (await _controlador.UOW.SeguimientoIndividualRepository.ObtenerDesdeVectorDePedidosAsync(vectorDePedidos)).ToList();
                siAMarcar.ForEach(si =>
                {
                    si.FechaBaja = fecha.Value;
                    _controlador.UOW.SeguimientoIndividualRepository.Update(si);
                });

                // Se agregan los nuevos seguimientos.
                _controlador.UOW.SeguimientoIndividualRepository.Insert(nuevosSeguimientos);

            }

            #endregion

            #region Verifica Pedidos (Hacia Arriba)

            // Si viene propagando desde arriba (porque tiene la bandera hacia abajo), el proceso termina aquí.
            // Si no viene propagando desde arriba (no viene con bandera hacia abajo),
            // entonces debe verificarse si se requirió propagar hacia alli (hacia arriba)
            if (!propagacion.HasFlag(Control.Propagacion.HaciaAbajo) && propagacion.HasFlag(Control.Propagacion.HaciaArriba))
            {

                using (PedidosBusiness pedidoBusiness = new PedidosBusiness())
                {

                    // Recorre los pedidos que se encuentran en el ViewModel.
                    foreach (var pedidoEnVM in VM.PedidosEtapa2)
                    {

                        // Si todos los seguimientos del pedido están en una etapa "inactiva" (entregados o dados de baja),
                        if (nuevosSeguimientos.Where(ns => ns.SeguimientoGlobalId == pedidoEnVM.PedidoId && eniEtapasActivas.Contains(ns.EtapaDelNegocioInternaId)).Count() == 0)
                        {

                            // Da de baja al pedido en general.
                            var pedido = new int[1] { pedidoEnVM.PedidoId };

                            // Como es el último eslabón, si propaga hacia abajo significa que viene de arriba con ese seteo y debe esa bandera debe desaparecer.
                            await pedidoBusiness.Baja(_controlador, pedido, fecha, Control.Propagacion.HaciaArriba);

                        }

                    }

                }

            }

            #endregion

            // Termina.
            return true;

        }

        /// <summary>
        /// Afecta la totalidad de los ítems activos de los pedidos indicados (los items activos son los que no están ni entregados ni dados de baja) a una nueva etapa de seguimiento.
        /// </summary>
        /// <param name="controlador">Controlador vigente al momento de llamar a esta función.</param>
        /// <param name="pedidosId">Lista de Códigos de Pedido a los que se les impondrá la nueva Etapa del Negocio Interna.</param>
        /// <param name="nuevaEtapaId">Código de la Etapa del Negocio Interna que tomará la totalidad de los items activos de los pedidos indicados</param>
        /// <param name="fecha">Especifica la fecha y la hora a utilizar en la actualización de los datos.</param>
        /// <param name="cascada">Indica si la función debe intentar dar de baja los elementos asociados.</param>
        public async Task<bool> AfectaPedidosEnteros(int[] pedidosId, int nuevaEtapaId, DateTime? fecha, Control.Propagacion propagacion = Control.Propagacion.HaciaArriba)
        {
            // Obtiene los pedidos indicados desde la base de datos.
            var pedidos = await _controlador.UOW.PedidoRepository.ObtenerDesdeVectorAsync(pedidosId);

            // Inicializa el VM en donde se plasmará la nueva distribución de seguimientos
            // para todos los pedidos.
            var VM = new AplicaSeguimientosVM();
            VM.NuevaEtapa = nuevaEtapaId;

            // Procesa los pedidos indicados.
            foreach (var pedido in pedidos)
            {
                // Dentro del VM, inicializa una nueva distribución para el pedido en proceso.
                var nuevoItem = new AplicaSeguimientosVM_Pedido();
                nuevoItem.ClienteId = pedido.Gestion.ClienteId;
                nuevoItem.PedidoId = pedido.PedidoId;

                // Indica que deben pasar a la nueva etapa todos los ítems del pedido aún pendientes.
                // Los ítems del pedido que están actualmente dados de baja o entregados no sufrirán cambios.
                nuevoItem.Cantidad = pedido.SeguimientoGlobal.CantidadPendiente;

                // Recorre la distribución de seguimiento actual.
                foreach (var si in pedido.SeguimientoGlobal.Detalle.Where(si => si.FechaBaja == null))
                {
                    
                    // Crea un ítem en el VM que contendrá los cambios que deben
                    // aplicarse a la entidad SeguimientoIndividual en proceso.
                    var nuevoDetalle = new AplicaSeguimientosVM_Seguimiento();
                    nuevoDetalle.EtapaDelNegocioInternaId = si.EtapaDelNegocioInternaId;
                    nuevoDetalle.CantidadActual = si.Cantidad;
                    
                    // Si los items en análisis están entregados (100) o dados de baja (101),
                    if (si.EtapaDelNegocioInterna.Nivel >= 100)
                    {
                        // Dicho item no resulta afectado.
                        nuevoDetalle.CantidadAfectada = 0;
                    }
                    else
                    {
                        // El item queda afectado en su totalidad.
                        nuevoDetalle.CantidadAfectada = si.Cantidad;
                    }
                    
                    // Agrega la nueva configuración para la entidad SeguimientoIndividual en proceso.
                    nuevoItem.Seguimientos.Add(nuevoDetalle);
                }

                // Agrega la nueva configuración para el pedido en proceso.
                VM.PedidosEtapa2.Add(nuevoItem);
            }

            // Redistribuye los seguimientos generados.
            return await RedistribuyeSeguimientos(VM, fecha, propagacion);

        }

        /// <summary>
        /// Analiza preventivamente los pedidos solicitados para hacer cambios en su seguimiento, en busca de inconsistencias que afecten el normal desarrollo del proceso.
        /// </summary>
        /// <param name="controlador">Controlador vigente al momento de llamar a esta función.</param>
        /// <param name="pedidosId">Lista de códigos de pedidos a verificar integridad.</param>
        /// <param name="detalles">Diccionario con una lista de detalles encontrados en el análisis.</param>
        /// <param name="fechaReferencia">Permite buscar modificaciones posteriores a la fecha indicada.</param>
        /// <returns>Indica si es posible continuar con el proceso de seguimiento, o si por algún motivo el proceso no puede continuar.</returns>
        public async Task<bool> AnalizaPedidos(List<int> pedidosId, Dictionary<int,Tuple<DetallesDetectados, string>> detalles, DateTime? fechaReferencia = null)
        {

            #region VerificaPedidos

            // Obtiene los pedidos del informe.
            _controlador.UOW.PedidoRepository.IncluyeHistorial = true;
            _controlador.UOW.PedidoRepository.IncluyeDadosDeBaja = true;
            var pedidos = await _controlador.UOW.PedidoRepository.ObtenerDesdeVectorAsync(pedidosId.ToArray());
            _controlador.UOW.PedidoRepository.Reset();

            // Recorre cada uno de los pedidos que especifica el listado guardado,
            // para su ordenamiento y verificación
            Pedido pedido;
            int erroresGraves = 0;
            if (detalles == null) detalles = new Dictionary<int, Tuple<DetallesDetectados, string>>();
            foreach (var numeroDePedido in pedidosId)
            {

                // Recupera el pedido en cuestión.
                // Los pedidos se recuperan desde la base con un orden aleatorio y se van
                // seleccionando de acuerdo al ordenamiento guardado en la base de datos.
                // Los pedidos, entonces, se van acomodando en orden al tiempo que se omiten
                // los que tienen detalles para observación.
                pedido = pedidos.Where(p => p.PedidoId == numeroDePedido).FirstOrDefault();
                DateTime ultimoCambioDeSeguimiento = pedido.SeguimientoGlobal.Detalle.Where(si => si.FechaBaja == null).Select(si => si.Fecha).Max();

                // Si luego de haberse generado el informe el pedido en cuestión ya no existe en la base de datos,
                if (pedido == null)
                {
                    detalles.Add(numeroDePedido, new Tuple<DetallesDetectados, string>(DetallesDetectados.PedidoNoEncontrado, String.Format("{0} | El pedido no existe en la base de datos.", numeroDePedido)));
                    erroresGraves++;
                }

                // Si luego de haberse generado el informe el pedido en cuestión fue dado de baja,
                else if (pedido.FechaBaja != null)
                {
                    detalles.Add(numeroDePedido, new Tuple<DetallesDetectados, string>(DetallesDetectados.PedidoDadoDeBaja, String.Format("{0} | El pedido fue dado de baja el {1}.", numeroDePedido, pedido.FechaBaja.Value.ToString("dd/MM/yyyy"))));
                    erroresGraves++;
                }

                // Si luego de haberse generado el informe el pedido en cuestión fue modificado,
                else if (fechaReferencia.HasValue && pedido.Historial.Count > 0 && pedido.Historial.OrderByDescending(h => h.FechaBaja).FirstOrDefault().FechaBaja > fechaReferencia.Value) detalles.Add(numeroDePedido, new Tuple<DetallesDetectados, string>(DetallesDetectados.PedidoModificado, String.Format("{0} | Se ha modificado una o más características del pedido con posterioridad al {1}, fecha en que se generó el listado solicitado.", numeroDePedido, fechaReferencia.Value.ToString("dd/MM/yyyy"))));

                // Si luego de haberse generado el informe el seguimiento del pedido en cuestión fue modificado,
                else if (fechaReferencia.HasValue && ultimoCambioDeSeguimiento > fechaReferencia.Value) detalles.Add(numeroDePedido, new Tuple<DetallesDetectados, string>(DetallesDetectados.SeguimientoModificado, String.Format("{0} | Se ha actualizado el seguimiento del pedido con posterioridad al {1}, fecha en que se generó el listado solicitado.", numeroDePedido, fechaReferencia.Value.ToString("dd/MM/yyyy"))));

            }

            #endregion

            #region AnálisisGlobal

            // Si luego de verificar cada pedido aún quedan items para procesar en la etapa siguiente,
            if (erroresGraves == pedidosId.Count) return false;

            #endregion

            // Pueden haberse detectado detalles pero es posible continuar con el proceso.
            return true;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Espacio para desechar objetos.
                    // En este caso se usa nada para cumplir sólamente con la interfaz IDisposable.
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

    public class SeguimientoPorListado : Seguimientos
    {

         public SeguimientoPorListado(UOWController unitOfWork) : base(unitOfWork) { }

    }
}
