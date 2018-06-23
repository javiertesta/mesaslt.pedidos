using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pedidos.ViewModels;
using Pedidos.Models;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Pedidos.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Pedidos.Controllers
{
    
    public class AdjuntosController : UOWController
    {

        private AdjuntosBusiness business = new AdjuntosBusiness();
        private int maximoAdjuntosPorPedido = 3;
        
        [HttpGet]
        public ActionResult Subir(int pedidoId)
        {
            
            // Rescata el pedido indicado.
            UOW.PedidoRepository.IncluyeAdjuntos = true;
            var pedido = UOW.PedidoRepository.ObtenerPorId(pedidoId);
            if (pedido == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            // Comprueba que el usuario autenticado tenga acceso al pedido.
            if (Request.IsAuthenticated)
            {
                var Usuario = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
                if (Usuario.ClienteId != null && Usuario.ClienteId != pedido.Gestion.ClienteId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // Completa el VM y procede a mostrar la vista.
            var VM = new SubirVM();
            VM.PedidoId = pedidoId;
            VM.Permitido = !pedido.FechaBaja.HasValue && (pedido.ArchivosAdjuntos.Count < maximoAdjuntosPorPedido);

            return View(VM);

        }

        [HttpPost]
        public ActionResult Subir(SubirVM VM)
        {

            if (ModelState.IsValid)
            {

                // Rescata el pedido indicado.
                UOW.PedidoRepository.IncluyeAdjuntos = true;
                var pedido = UOW.PedidoRepository.ObtenerPorId(VM.PedidoId);
                if (pedido == null) return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

                // Comprueba que el usuario autenticado tenga acceso al pedido.
                if (Request.IsAuthenticated)
                {
                    var Usuario = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
                    if (Usuario.ClienteId != null && Usuario.ClienteId != pedido.Gestion.ClienteId) return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }

                // Comprueba que no se haya superado la cantidad máxima de archivos adjuntos permitidos para un mismo pedido.
                if (pedido.FechaBaja.HasValue || (pedido.ArchivosAdjuntos.Count >= maximoAdjuntosPorPedido)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

                // Corrobora que el archivo tenga coherencia
                // y genera la entidad para registrarlo en la base de datos.
                if (VM.Archivo != null && VM.Archivo.ContentLength > 0)
                {
                    var archivoAdjunto = new ArchivoAdjuntoEnPedido
                    {
                        Tipo = VM.Archivo.ContentType,
                        PedidoId = VM.PedidoId,
                        NombreOriginal = System.IO.Path.GetFileName(VM.Archivo.FileName),
                        FechaCreacion = DateTime.Now
                    };
                    archivoAdjunto.Nombre = archivoAdjunto.GenerarNuevoNombre(archivoAdjunto.NombreOriginal);
                    UOW.ArchivoEnDiscoRepository.Insert(archivoAdjunto);
                
                    // Graba el archivo en el sistema.
                    VM.Archivo.SaveAs(Path.Combine(archivoAdjunto.Ubicacion, archivoAdjunto.Nombre));

                    // Registra el archivo en la base de datos.
                    try
                    {
                        UOW.SaveChanges();
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError(String.Empty, "No se pudo dar de alta el archivo en la base de datos. Intente nuevamente.");
                        VM.Archivo = null;
                        return View(VM);
                    }
                
                    return RedirectToAction("Index");
                }

            }
            
            // No pasó la validación.
            ModelState.AddModelError(String.Empty, "Alguno de los datos confirmados no es válido. Intente nuevamente.");
            return View();

        }

        public async Task<ActionResult> Baja(int? id, string volverA)
        {

            // Si no se especificó un Id, presenta un error.
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Intenta obtener la Gestión mediante su Id.
            var fromDb = await UOW.ArchivoEnDiscoRepository.ObtenerPorIdAsync(id ?? 0);

            // Si el Id resultó ser incorrecto, presenta un error.
            if (fromDb == null) return HttpNotFound();

            // Si el Id resultó ser incorrecto por ser un archivo pero no un adjunto de un pedido.
            if (fromDb as ArchivoAdjuntoEnPedido == null) return HttpNotFound();

            // Mapeo
            ArchivoAdjuntoEnPedido modelo = new ArchivoAdjuntoEnPedido();
            modelo.ArchivoEnDiscoId = fromDb.ArchivoEnDiscoId;
            modelo.PedidoId = (fromDb as ArchivoAdjuntoEnPedido).PedidoId;
            modelo.FechaCreacion = fromDb.FechaCreacion;
            modelo.NombreOriginal = fromDb.NombreOriginal;
            modelo.Tipo = fromDb.Tipo;
            ViewBag.VolverA = volverA;

            // Presenta la pantalla de confirmación.
            return View(modelo);

        }

        [HttpPost, ActionName("Baja")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BajaConfirmada(ArchivoAdjuntoEnPedido model, string volverA)
        {

            // Si se pudo construir correctamente el ViewModel,
            if (ModelState.IsValid)
            {

                // Procesa los datos.
                try
                {
                    await business.Baja(this, model.ArchivoEnDiscoId);
                    UOW.SaveChanges();
                }

                // Capta cualquier error producido en el bloque "try".
                catch (Exception)
                {
                    ModelState.AddModelError(String.Empty, "No se pudo dar de baja el registro. Intenta nuevamente en unos instantes o compruebe que el mismo no haya sido dado de baja en el medio del proceso.");
                    return View("Baja", model);
                }

                // El proceso se realizó correctamente.
                // Regresa.
                return Redirect(volverA);

            }

            // No pudo armarse el VM con los datos rescatados del formulario.
            // Vuelve a presentar la pantalla de confirmación.
            ModelState.AddModelError(String.Empty, "Ocurrió un error en el pasaje entre pantallas. Intentá nuevamente en unos segundos o reintentá la operación desde cero.");
            return View("Baja", model);

        }

    }

    public class AdjuntosBusiness : IDisposable
    {

        private bool disposed = false;

        public async Task<bool> Baja(UOWController controlador, int id)
        {
            
            // Intenta obtener el archivo desde la base de datos.
            var fromDb = await controlador.UOW.ArchivoEnDiscoRepository.ObtenerPorIdAsync(id);
            if (fromDb == null) throw new Exception("El archivo especificado no existe registrado en la base de datos.");
            if ((fromDb as ArchivoAdjuntoEnPedido) == null) throw new Exception("El archivo especificado es un archivo pero no es un archivo adjunto de un pedido.");

            // El borrado físico del archivo puede hacerse en cualquier momento,
            // dado que no es posible conocer en este punto el momento en donde se plasmará la transacción actual
            // y se hará efectivo el borrado del registro en la base de datos.
            // Por lo tanto, por el momento no queda otra opción que borrar físicamente el registro y después ver.

            // Si el archivo existe, lo borra.
            // Si no existe, no hace nada (no devuelve error alguno)
            if (System.IO.File.Exists(fromDb.Ruta)) System.IO.File.Delete(fromDb.Ruta);

            // Borra el registro de la base de datos.
            controlador.UOW.ArchivoEnDiscoRepository.Delete(fromDb);

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