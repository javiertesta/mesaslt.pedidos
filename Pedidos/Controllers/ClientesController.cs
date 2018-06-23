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
using Pedidos.ViewModels;
using System.Collections;

namespace Pedidos.Controllers
{
    [Authorize(Roles = "Webmaster,Gerencia")]
    public class ClientesController : Controller
    {
        
        private PedidosDbContext db = new PedidosDbContext();

        // POST: Clientes
        [HttpPost]
        public ActionResult Busqueda(string newSearch)
        {
            var qs = Request.QueryString.toRouteValueDictionary();
            if (!String.IsNullOrEmpty(newSearch))
            {
                RouteValueDictionary newParams = new RouteValueDictionary();
                newParams.Add("search", newSearch.ToLower());
                newParams.Add("page", "1");
                return RedirectToAction("Busqueda", "Clientes", qs.Merge(newParams));
            }
            else
            {
                RouteValueDictionary newParams = new RouteValueDictionary();
                newParams.Add("page", "1");
                if (qs.ContainsKey("search")) qs.Remove("search");
                return RedirectToAction("Busqueda", "Clientes", qs.Merge(newParams));
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        // GET: Clientes
        public ActionResult Busqueda(string sort, string search, int? page, string output)
        {
            using (var specialContext = new PedidosDbContext())
            {
                specialContext.Configuration.LazyLoadingEnabled = false;
                specialContext.Configuration.ProxyCreationEnabled = false;

                var clientes = from c in specialContext.Clientes
                               select c;

                if (!String.IsNullOrEmpty(search))
                {
                    var filteredEnum = EnumExtensionMethods.EnumDisplayNamesToDictionary<Zonas>().FilterKeys<Zonas, string>(search);

                    clientes = clientes.Where(c => c.ClienteId.ToLower().Contains(search)
                                           || filteredEnum.Contains(c.Zona)
                                           || c.RazonSocial.ToLower().Contains(search));
                }

                ViewBag.linkCodigo = "codigo";
                ViewBag.linkZona = "zona";
                ViewBag.linkRazonSocial = "razonsocial";

                switch (sort)
                {
                    case "codigo_desc":
                        clientes = clientes.OrderByDescending(c => c.ClienteId);
                        break;
                    case "zona_desc":
                        clientes = clientes.OrderByDescending(c => c.Zona);
                        break;
                    case "razonsocial_desc":
                        clientes = clientes.OrderByDescending(c => c.RazonSocial);
                        break;
                    case "zona":
                        clientes = clientes.OrderBy(c => c.Zona);
                        ViewBag.linkZona = "zona_desc";
                        break;
                    case "razonsocial":
                        clientes = clientes.OrderBy(c => c.RazonSocial);
                        ViewBag.linkRazonSocial = "razonsocial_desc";
                        break;
                    default:
                        clientes = clientes.OrderBy(c => c.ClienteId);
                        ViewBag.linkCodigo = "codigo_desc";
                        break;
                }

                if (!String.IsNullOrEmpty(output))
                {
                    if (output.ToLower() == "xml") return new XmlResult<List<Cliente>>(clientes.ToList());
                }

                int pageSize = 15;
                int pageNumber = (page ?? 1);
                IEnumerable<ClientesIndexVM> clientesVM = from cliente in clientes.AsEnumerable()
                                                          select new ClientesIndexVM
                                                          {
                                                              ClienteId = cliente.ClienteId,
                                                              RazonSocial = cliente.RazonSocial,
                                                              ZonaNombre = cliente.ZonaNombre
                                                          };
                return View(clientesVM.ToPagedList(pageNumber, pageSize));
            }

        }

        // GET: Clientes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClientesCreateVM clienteVM)
        {
            if (ModelState.IsValid)
            {
                var cliente = new Cliente();
                cliente.ClienteId = clienteVM.ClienteId;
                cliente.RazonSocial = clienteVM.RazonSocial;
                cliente.Zona = clienteVM.Zona;
                try
                {
                    db.Clientes.Add(cliente);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(String.Empty, "Error al dar de alta el artículo. Intente nuevamente en unos instantes.");
                }
            }
            return View(clienteVM);
        }

        // GET: Clientes/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Cliente cliente = await db.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            
            ClientesEditVM clienteVM = new ClientesEditVM();
            clienteVM.ClienteId = cliente.ClienteId;
            clienteVM.RazonSocial = cliente.RazonSocial;
            clienteVM.Zona = cliente.Zona;
            clienteVM.RowVersion = cliente.RowVersion;
            
            return View(clienteVM);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ClientesEditVM clienteVM)
        {
            if (ModelState.IsValid)
            {
                Cliente cliente = new Cliente();
                cliente.ClienteId = clienteVM.ClienteId;
                cliente.RazonSocial = clienteVM.RazonSocial;
                cliente.Zona = clienteVM.Zona;
                cliente.RowVersion = clienteVM.RowVersion;
                db.Entry(cliente).State = EntityState.Modified;
                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(String.Empty, "Error al grabar el cliente. Intente nuevamente en unos instantes.");
                }
            }
            return View(clienteVM);
        }

        // GET: Clientes/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Cliente cliente = await db.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }
            
            ClientesDeleteVM clienteVM = new ClientesDeleteVM();
            clienteVM.ClienteId = cliente.ClienteId;
            clienteVM.RazonSocial = cliente.RazonSocial;
            clienteVM.ZonaNombre = cliente.ZonaNombre;
            
            return View(clienteVM);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            try
            {
                Cliente cliente = await db.Clientes.FindAsync(id);
                db.Clientes.Remove(cliente);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                ModelState.AddModelError(String.Empty, "Error al borrar el comprobante. Intente nuevamente en unos instantes.");
            }
            return RedirectToAction("Delete", new { @id = id });
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
