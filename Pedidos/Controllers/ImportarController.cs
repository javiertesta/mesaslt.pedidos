using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Pedidos.DAL;
using Pedidos.Models;
using Pedidos.Models.Enums;
using CustomExtensions;
using PagedList;
using System.Web.Routing;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;
using System.Web;
using System.Text;
using System.Diagnostics;

namespace Pedidos.Controllers
{
    [Authorize(Roles = "Webmaster")]
    public class ImportarController : Controller
    {
        
        private PedidosDbContext db = new PedidosDbContext();

        // POST: Import
        [HttpPost]
        public async Task<ActionResult> Index(string type, HttpPostedFileBase upload)
        {
            type = type ?? "";
            
            // Comprobaciones previas.
            if (upload == null || upload.ContentLength < 1)
            {
                ModelState.AddModelError("", "No se ha seleccionado un archivo o hubo un error al intentar subirlo.");
                return View();
            }
            if (!(upload.ContentType == "text/xml"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable);
            }


            // Creación de variables de trabajo.
            XmlSerializer serializer = null;

            switch (type)
            {
                case "Clientes":
                    
                    // Variables de Trabajo.
                    Cliente[] ClientesSubidos = null;
                    Cliente[] ClientesActuales = null;
                    Cliente ClienteAProcesar = null;
            
                    // Deserializa el archivo XML subido y obtiene la lista de objetos Cliente.
                    serializer = new XmlSerializer(typeof(Cliente[]));
                    ClientesSubidos = (Cliente[])serializer.Deserialize(upload.InputStream);

                    // Obtiene el total de los clientes en el sistema.
                    ClientesActuales = db.Clientes.ToArray();

                    // Recorre la lista de clientes subidos.
                    foreach (var UploadedEntity in ClientesSubidos)
                    {
                        // Si el cliente subido no existe en la base de datos.
                        if (await db.Clientes.FindAsync(UploadedEntity.ClienteId) == null)
                        {
                            // Agrega el cliente.
                            db.Clientes.Add(UploadedEntity);
                        }
                        // Si el cliente subido ya existe en la base de datos.
                        else
                        {
                            // Obtiene el cliente a actualizar desde el array que contiene al total de los mismos.
                            ClienteAProcesar = ClientesActuales.Where(c => c.ClienteId == UploadedEntity.ClienteId).FirstOrDefault();
                            // Actualiza sus datos.
                            ClienteAProcesar.Zona = UploadedEntity.Zona;
                            ClienteAProcesar.RazonSocial = UploadedEntity.RazonSocial;
                        }
                    }
                    
                    await db.SaveChangesAsync();
                    break;

                case "Gestiones":

                    // Variables de Trabajo.
                    Gestion[] GestionesSubidas = null;
                    Gestion[] GestionesActuales = null;
                    Gestion GestionAProcesar = null;

                    // Deserializa el archivo XML subido y obtiene la lista de objetos Gestion.
                    serializer = new XmlSerializer(typeof(Gestion[]));
                    GestionesSubidas = (Gestion[])serializer.Deserialize(upload.InputStream);

                    // Obtiene el total de gestiones en el sistema.
                    GestionesActuales = db.Gestiones.ToArray();

                    // Recorre la lista de gestiones subidas.
                    foreach (var UploadedEntity in GestionesSubidas)
                    {
                        // Si la gestión subida no existe en la base de datos.
                        if (await db.Gestiones.FindAsync(UploadedEntity.GestionId) == null)
                        {
                            // Agrega el contacto.
                            db.Gestiones.Add(UploadedEntity);
                        }
                        // Si la gestión subida ya existe en la base de datos.
                        else
                        {
                            // Obtiene la gestión a actualizar desde el array que contiene al total de las mismas.
                            GestionAProcesar = GestionesActuales.Where(c => c.GestionId == UploadedEntity.GestionId).FirstOrDefault();
                            // Actualiza sus datos.
                            //ContactoAProcesar.Zona = UploadedEntity.Zona;
                            //ContactoAProcesar.RazonSocial = UploadedEntity.RazonSocial;
                        }
                    }

                    await db.SaveChangesAsync();
                    break;

                case "Pedidos":

                    // Variables de Trabajo.
                    Pedido[] PedidosSubidos = null;
                    Pedido[] PedidosActuales = null;
                    Pedido PedidoAProcesar = null;

                    // Deserializa el archivo XML subido y obtiene la lista de objetos Contacto.
                    serializer = new XmlSerializer(typeof(Pedido[]));
                    PedidosSubidos = (Pedido[])serializer.Deserialize(upload.InputStream);

                    // Obtiene el total de gestiones en el sistema.
                    PedidosActuales = db.Pedidos.ToArray();

                    // Recorre la lista de contactos subidos.
                    foreach (var UploadedEntity in PedidosSubidos)
                    {
                        // Si el contacto subido no existe en la base de datos.
                        if (await db.Gestiones.FindAsync(UploadedEntity.GestionId) == null)
                        {
                            // Agrega el contacto.
                            db.Pedidos.Add(UploadedEntity);
                        }
                        // Si el contacto subido ya existe en la base de datos.
                        else
                        {
                            // Obtiene el contacto a actualizar desde el array que contiene al total de los mismos.
                            PedidoAProcesar = PedidosActuales.Where(c => c.GestionId == UploadedEntity.GestionId).FirstOrDefault();
                            // Actualiza sus datos.
                            //ContactoAProcesar.Zona = UploadedEntity.Zona;
                            //ContactoAProcesar.RazonSocial = UploadedEntity.RazonSocial;
                        }
                    }

                    await db.SaveChangesAsync();
                    break;

                default:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Import
        public ActionResult Index()
        {
            return View();
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
