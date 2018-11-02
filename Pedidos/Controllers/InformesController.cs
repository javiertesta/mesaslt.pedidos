using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Pedidos.DAL;
using Pedidos.Models;
using System.Data.Entity;
using CustomExtensions;

namespace Pedidos.Controllers
{

    [Authorize(Roles = "Webmaster,Gerencia,Jefe")]
    public class InformesController : UOWController
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Clientes()
        {

            Informes.InformeEnTabla informe = new Informes.InformeEnTabla(PageSize.LEGAL, 36, 36, 18, 36);
            List<string> contenidos;

            informe.Titulo = "Listado de Clientes";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado clientes mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            // Abre el documento para permitir la escritura.
            informe.Open();

            // Obtiene los datos desde la base.
            List<Cliente> clientes = UOW.ClienteRepository.Obtener(null, listado => listado.OrderBy(c => c.ClienteId));

            // Crea la tabla que contendrá los datos presentados.
            List<Informes.Columna> columnas = new List<Informes.Columna>();
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 15, Contenido = "Código" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 45, Contenido = "Razón Social" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_RIGHT, AnchoRelativo = 40, Contenido = "Zona" });
            informe.CrearTabla(columnas);

            // Comprueba si hay gestiones para mostrar.
            PdfPCell listadoVacio = informe.CeldaDatos("Listado Vacío");
            listadoVacio.Colspan = informe.Tabla.NumberOfColumns;
            if (clientes.FirstOrDefault() == null) informe.Tabla.AddCell(listadoVacio);

            // Recorre el listado de clientes.
            foreach (var cliente in clientes)
            {
                contenidos = new List<string>();
                contenidos.Add(cliente.ClienteId);
                contenidos.Add(cliente.RazonSocial);
                contenidos.Add(cliente.ZonaNombre);
                informe.AgregarFila(contenidos);
            }

            // Agrega la tabla al documento y finaliza, cerrando los objetos requeridos.
            informe.Close();

            // Devuelve el archivo generado al explorador.
            return informe.SendFileResult();

        }

        public ActionResult Gestiones()
        {

            Informes.InformeEnTabla informe = new Informes.InformeEnTabla(PageSize.LEGAL.Rotate(), 36, 36, 18, 36);
            List<string> contenidos;

            informe.Titulo = "Listado de Gestiones";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado gestiones mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            // Abre el documento para permitir la escritura.
            informe.Open();

            // Obtiene los datos desde la base.
            List<Gestion> gestiones = UOW.GestionRepository.ListarTodo("InformeDeGestionesActivas", "", "", 1, Int32.MaxValue, Request.UserLanguages[0]).ToList();

            // Crea la tabla que contendrá los datos presentados.
            List<Informes.Columna> columnas = new List<Informes.Columna>();
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 2, Contenido = "Código" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_CENTER, AnchoRelativo = 2, Contenido = "Fecha" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_CENTER, AnchoRelativo = 2, Contenido = "Cliente" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 6, Contenido = "Observaciones" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_RIGHT, AnchoRelativo = 5, Contenido = "Usuario" });
            informe.CrearTabla(columnas);

            // Comprueba si hay gestiones para mostrar.
            PdfPCell listadoVacio = informe.CeldaDatos("Listado Vacío");
            listadoVacio.Colspan = informe.Tabla.NumberOfColumns;
            if (gestiones.FirstOrDefault() == null) informe.Tabla.AddCell(listadoVacio);

            // Recorre la gestiones activas.
            foreach (var gestion in gestiones)
            {
                contenidos = new List<string>();
                contenidos.Add(gestion.GestionId.ToString());
                contenidos.Add(gestion.FechaGestion.ToString("dd/MM/yyyy HH:mm"));
                contenidos.Add(gestion.ClienteId);
                contenidos.Add(gestion.Observaciones);
                contenidos.Add(gestion.UserName);
                informe.AgregarFila(contenidos);
            }

            // Agrega la tabla al documento y finaliza, cerrando los objetos requeridos.
            informe.Close();

            // Devuelve el archivo generado al explorador.
            return informe.SendFileResult();

        }

        public ActionResult PedidosActivos()
        {

            Informes.InformeDePedidosActivos informe = new Informes.InformeDePedidosActivos(PageSize.LEGAL, 36, 36, 18, 36);
            informe.Titulo = "Listado General de Pedidos";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado pedidos mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            informe.Open();

            // Obtiene los datos desde la base.
            List<Pedido> pedidos = UOW.PedidoRepository.ListarTodo("InformeDePedidosActivos", "", "", 1, Int32.MaxValue, Request.UserLanguages[0])
                .OrderBy(p => p.Gestion.Cliente.ZonaNombre)
                .ThenBy(p => p.Gestion.ClienteId)
                .ToList();

            // Comprueba si hay pedidos para mostrar.
            if (pedidos.FirstOrDefault() == null) informe.ListadoVacio();

            Pedidos.Models.Enums.Zonas? _zonaActiva = null;
            string _clienteActivo = "";

            foreach (var pedido in pedidos)
            {

                // Si cambió la zona, genera una sección nueva.
                if (!_zonaActiva.HasValue || _zonaActiva.Value != pedido.Gestion.Cliente.Zona)
                {
                    _zonaActiva = pedido.Gestion.Cliente.Zona;
                    informe.DisponerTitulo(_zonaActiva.GetEnumMemberDisplayName<Pedidos.Models.Enums.Zonas>());
                }

                // Si cambió el cliente, genera una subsección nueva.
                if (_clienteActivo != pedido.Gestion.ClienteId)
                {
                    _clienteActivo = pedido.Gestion.ClienteId;
                    informe.DisponerSubtitulo(String.Format("{0} {1}", pedido.Gestion.ClienteId, pedido.Gestion.Cliente.RazonSocial));
                }

                informe.AgregarLinea(informe.GenerarLinea(pedido));

            }

            // Agrega Índices
            informe.AgregarIndiceDeTitulos();
            informe.AgregarIndiceDeSubtitulos();

            // Cierra los objetos requeridos.
            informe.Close();

            // Vuelca el contenido del stream en el explorador.
            return informe.SendFileResult();

        }

        public ActionResult DadosDeBaja()
        {

            Informes.InformeDePedidosActivos informe = new Informes.InformeDePedidosActivos(PageSize.LEGAL, 36, 36, 18, 36);
            informe.Titulo = "Listado de Pedidos Dados de BAJA";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado pedidos baja mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            informe.Open();

            // Obtiene los datos desde la base.
            List<Pedido> pedidos = UOW.PedidoRepository.ListarDadosDeBaja("0001a", DateTime.MinValue, DateTime.MaxValue, 1, Int32.MaxValue, Request.UserLanguages[0])
                .OrderBy(p => p.Gestion.ClienteId)
                .ToList();

            // Comprueba si hay pedidos para mostrar.
            if (pedidos.FirstOrDefault() == null) informe.ListadoVacio();

            Pedidos.Models.Enums.Zonas? _zonaActiva = null;
            string _clienteActivo = "";

            foreach (var pedido in pedidos)
            {

                // Si cambió la zona, genera una sección nueva.
                if (!_zonaActiva.HasValue || _zonaActiva.Value != pedido.Gestion.Cliente.Zona)
                {
                    _zonaActiva = pedido.Gestion.Cliente.Zona;
                    informe.DisponerTitulo(_zonaActiva.GetEnumMemberDisplayName<Pedidos.Models.Enums.Zonas>());
                }

                // Si cambió el cliente, genera una subsección nueva.
                if (_clienteActivo != pedido.Gestion.ClienteId)
                {
                    _clienteActivo = pedido.Gestion.ClienteId;
                    informe.DisponerSubtitulo(String.Format("{0} {1}", pedido.Gestion.ClienteId, pedido.Gestion.Cliente.RazonSocial));
                }

                informe.AgregarLinea(informe.GenerarLinea(pedido));

            }

            // Agrega Índices
            informe.AgregarIndiceDeTitulos();
            informe.AgregarIndiceDeSubtitulos();

            // Cierra los objetos requeridos.
            informe.Close();

            // Vuelca el contenido del stream en el explorador.
            return informe.SendFileResult();

        }

        public ActionResult PedidosDemorados()
        {

            Informes.InformeDePedidosDemorados informe = new Informes.InformeDePedidosDemorados(PageSize.LEGAL, 36, 36, 18, 36);
            informe.Titulo = "Listado de Pedidos Demorados";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado pedidos demorados mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            informe.Open();

            // Obtiene los datos desde la base.
            List<Pedido> pedidos = UOW.PedidoRepository.ListarDemorados("InformeDePedidosActivos", "", "", 1, Int32.MaxValue, Request.UserLanguages[0])
                .OrderBy(p => p.Gestion.Cliente.ZonaNombre)
                .ThenBy(p => p.Gestion.ClienteId)
                .ToList();

            // Comprueba si hay pedidos para mostrar.
            if (pedidos.FirstOrDefault() == null) informe.ListadoVacio();

            // Registra el listado para que posteriormente se pueda recuperar y analizar.
            UOW.PedidosListadosRepository.Insert(new PedidoListado()
            {
                Fecha = informe.Fecha,
                Creador = informe.Creador,
                Titulo = informe.Titulo,
                Pedidos = String.Join(",", pedidos.Select(p => p.PedidoId).ToArray())
            });
            UOW.SaveChanges();

            Pedidos.Models.Enums.Zonas? _zonaActiva = null;
            string _clienteActivo = "";

            foreach (var pedido in pedidos)
            {

                // Si cambió la zona, genera una sección nueva.
                if (!_zonaActiva.HasValue || _zonaActiva.Value != pedido.Gestion.Cliente.Zona)
                {
                    _zonaActiva = pedido.Gestion.Cliente.Zona;
                    informe.DisponerTitulo(_zonaActiva.GetEnumMemberDisplayName<Pedidos.Models.Enums.Zonas>());
                }

                // Si cambió el cliente, genera una subsección nueva.
                if (_clienteActivo != pedido.Gestion.ClienteId)
                {
                    _clienteActivo = pedido.Gestion.ClienteId;
                    informe.DisponerSubtitulo(String.Format("{0} {1}", pedido.Gestion.ClienteId, pedido.Gestion.Cliente.RazonSocial));
                }

                informe.AgregarLinea(informe.GenerarLinea(pedido));

            }

            // Agrega Índices
            informe.AgregarIndiceDeTitulos();
            informe.AgregarIndiceDeSubtitulos();

            // Cierra los objetos requeridos.
            informe.Close();

            // Vuelca el contenido del stream en el explorador.
            return informe.SendFileResult();

        }

        public ActionResult CorteDeLaminado()
        {

            Informes.InformeEnTabla informe = new Informes.InformeEnTabla(PageSize.LEGAL.Rotate(), 36, 36, 18, 36);
            List<string> contenido;
            informe.Titulo = "Listado para Corte de Laminado";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado pedidos corte laminado mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            // Abre el documento para permitir la escritura.
            informe.Open();

            // Crea la tabla que contendrá los datos presentados.
            List<Informes.Columna> columnas = new List<Informes.Columna>();
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 10, Contenido = "Cantidad" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 50, Contenido = "Modelo" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_CENTER, AnchoRelativo = 10, Contenido = "Melamina" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_CENTER, AnchoRelativo = 10, Contenido = "Color" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_CENTER, AnchoRelativo = 10, Contenido = "Cliente" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_RIGHT, AnchoRelativo = 10, Contenido = "Pedido" });
            informe.CrearTabla(columnas);

            // Obtiene los datos desde la base.
            List<Pedido> pedidos = UOW.PedidoRepository.InformePedidosParaCorteDeLaminado();

            // Crea una celda que contiene un mensaje de "Listado Vacío".
            PdfPCell listadoVacio = informe.CeldaDatos("Listado Vacío");
            listadoVacio.Colspan = informe.Tabla.NumberOfColumns;

            // Si no hay ninguna tapa para mostrar,
            if (pedidos.FirstOrDefault() == null)
            {
                informe.Tabla.AddCell(listadoVacio);
            }

            // Si hay tapas para mostrar,
            else
            {

                // Registra el listado para que posteriormente se pueda recuperar y analizar.
                UOW.PedidosListadosRepository.Insert(new PedidoListado()
                {
                    Fecha = informe.Fecha,
                    Creador = informe.Creador,
                    Titulo = informe.Titulo,
                    Pedidos = String.Join(",", pedidos.Select(p => p.PedidoId).ToArray())
                });
                UOW.SaveChanges();

                // Procede a listar las tapas para corte.
                Tapa tapa;
                PdfPCell celda;
                foreach (var pedido in pedidos)
                {

                    tapa = pedido.Articulo as Tapa;
                    contenido = new List<string>();
                    contenido.Add(pedido.SeguimientoGlobal.ConjuntoAtrasado.Cantidad.ToString());
                    contenido.Add(tapa.ToString("Controllers.InformesController.CorteDeLaminado"));
                    contenido.Add(tapa.Melamina.HasValue && tapa.Melamina.Value ? "MEL" : " ");
                    contenido.Add(tapa.Laminado_CodigoId);
                    contenido.Add(pedido.Gestion.ClienteId);
                    contenido.Add(pedido.PedidoId.ToString());
                    informe.AgregarFila(contenido);

                    if (!String.IsNullOrWhiteSpace(pedido.Articulo.Particularidades))
                    {
                        celda = informe.CeldaDatos(String.Format("Pedido {0} -> {1}", pedido.PedidoId.ToString(), tapa.Particularidades));
                        celda.Colspan = columnas.Count;
                        informe.Tabla.AddCell(celda);
                    }

                }

            }

            // Agrega la tabla al documento y finaliza, cerrando los objetos requeridos.
            informe.Close();

            // Vuelca el contenido del stream en el explorador.
            return informe.SendFileResult();

        }

        [ActionName("mdf")]
        public ActionResult MDF()
        {

            Informes.InformeEnTabla informe = new Informes.InformeEnTabla(PageSize.LEGAL.Rotate(), 36, 36, 18, 36);
            List<string> contenido;
            informe.Titulo = "Listado para Cabina";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado pedidos cabina MDF lustre mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            // Abre el documento para permitir la escritura.
            informe.Open();

            // Crea la tabla que contendrá los datos presentados.
            List<Informes.Columna> columnas = new List<Informes.Columna>();
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 10, Contenido = "Cantidad" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 30, Contenido = "Descripción" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 10, Contenido = "Color" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 10, Contenido = "BDE Tipo" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 10, Contenido = "BDE Espesor" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_LEFT, AnchoRelativo = 10, Contenido = "BDE Color" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_RIGHT, AnchoRelativo = 10, Contenido = "Cliente" });
            columnas.Add(new Informes.Columna() { AlineacionHorizontal = PdfPCell.ALIGN_RIGHT, AnchoRelativo = 10, Contenido = "Pedido" });
            informe.CrearTabla(columnas);

            // Obtiene los datos desde la base.
            List<Pedido> pedidos = UOW.PedidoRepository.InformePedidosCabina();

            // Crea una celda que contiene un mensaje de "Listado Vacío".
            PdfPCell listadoVacio = informe.CeldaDatos("Listado Vacío");
            listadoVacio.Colspan = informe.Tabla.NumberOfColumns;

            // Si no hay ninguna tapa para mostrar,
            if (pedidos.FirstOrDefault() == null)
            {
                informe.Tabla.AddCell(listadoVacio);
            }

            // Si hay tapas para mostrar,
            else
            {

                // Registra el listado para que posteriormente se pueda recuperar y analizar.
                UOW.PedidosListadosRepository.Insert(new PedidoListado()
                {
                    Fecha = informe.Fecha,
                    Creador = informe.Creador,
                    Titulo = informe.Titulo,
                    Pedidos = String.Join(",", pedidos.Select(p => p.PedidoId).ToArray())
                });
                UOW.SaveChanges();

                // Procede a listar las tapas para corte.
                Tapa tapa;
                PdfPCell celda;
                foreach (var pedido in pedidos)
                {

                    tapa = pedido.Articulo as Tapa;
                    contenido = new List<string>();
                    contenido.Add(pedido.SeguimientoGlobal.ConjuntoAtrasado.Cantidad.ToString());
                    contenido.Add(tapa.ToString("Controllers.InformesController.MDF"));
                    contenido.Add(tapa.Laminado_CodigoId);
                    contenido.Add(tapa.Borde.TipoNombre);
                    contenido.Add(tapa.Borde.EspesorNombre);
                    contenido.Add(tapa.Borde.ColorNombre);
                    contenido.Add(pedido.Gestion.ClienteId);
                    contenido.Add(pedido.PedidoId.ToString());
                    informe.AgregarFila(contenido);

                    celda = informe.CeldaDatos(pedido.FechaEntrega.HasValue ? String.Format("PARA EL {0}", pedido.FechaEntrega.Value.ToString("dd/MM")) : String.Format("Pedido el {0}", pedido.Gestion.FechaGestion.ToString("dd/MM")));
                    celda.HorizontalAlignment = Element.ALIGN_LEFT;
                    celda.BackgroundColor = BaseColor.LIGHT_GRAY;
                    informe.Tabla.AddCell(celda);

                    if (!String.IsNullOrWhiteSpace(pedido.Articulo.Particularidades) || !String.IsNullOrWhiteSpace(pedido.Observaciones))
                    {
                        List<string> textos = new List<string>();
                        if (!String.IsNullOrWhiteSpace(tapa.Particularidades)) textos.Add(tapa.Particularidades);
                        if (!String.IsNullOrWhiteSpace(pedido.Observaciones)) textos.Add(pedido.Observaciones);
                        celda = informe.CeldaDatos(String.Format((textos.Count == 0) ? " " : "[Pedido {0} --> {1}]", pedido.PedidoId.ToString(), String.Join(" / ", textos.ToArray())));
                        celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                    }
                    else celda = informe.CeldaDatos(" ");

                    celda.Colspan = columnas.Count - 1;
                    informe.Tabla.AddCell(celda);

                    celda = informe.CeldaDatos(" ");
                    celda.Colspan = columnas.Count;
                    informe.Tabla.AddCell(celda);

                }

            }

            // Agrega la tabla al documento y finaliza, cerrando los objetos requeridos.
            informe.Close();

            // Vuelca el contenido del stream en el explorador.
            return informe.SendFileResult();

        }

        public ActionResult Zona(Pedidos.Models.Enums.Zonas? id)
        {

            // Corrobora que la zona pasada como argumento exista.
            if (id == null || !Enum.IsDefined(typeof(Pedidos.Models.Enums.Zonas), id.Value)) return View("SeleccionaZona");

            Informes.InformeDePedidosActivos informe = new Informes.InformeDePedidosActivos(PageSize.LEGAL, 36, 36, 18, 36);
            informe.Titulo = "Listado de Zona";
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = "PDF informe listado pedidos zona zonas mesas lt";
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            informe.Open();

            // Obtiene los datos desde la base.
            List<Pedido> pedidos = UOW.PedidoRepository.InformePedidosActivosPorZona(id.Value);

            // Comprueba si hay pedidos para mostrar.
            if (pedidos.FirstOrDefault() == null) informe.ListadoVacio();

            string _clienteActivo = "";

            foreach (var pedido in pedidos)
            {

                // Si cambió el cliente, genera una subsección nueva.
                if (_clienteActivo != pedido.Gestion.ClienteId)
                {
                    _clienteActivo = pedido.Gestion.ClienteId;
                    informe.DisponerSubtitulo(String.Format("{0} {1}", pedido.Gestion.ClienteId, pedido.Gestion.Cliente.RazonSocial));
                }

                informe.AgregarLinea(informe.GenerarLinea(pedido));

            }

            // Cierra los objetos requeridos.
            informe.Close();

            // Vuelca el contenido del stream en el explorador.
            return informe.SendFileResult();

        }

        public ActionResult Cliente(string id)
        {

            // Si no se pasó ningún argumento, presenta la pantalla de elección de cliente.
            if (String.IsNullOrEmpty(id)) return View("SeleccionaCliente");

            // Corrobora que la zona pasada como argumento exista.
            if (UOW.ClienteRepository.ObtenerPorId(id) == null)
            {
                ModelState.AddModelError(String.Empty, "El cliente es inválido.");
                return View("SeleccionaCliente");
            }

            Informes.InformeDePedidosActivos informe = new Informes.InformeDePedidosActivos(PageSize.LEGAL, 36, 36, 18, 36);
            informe.Titulo = String.Format("Listado de Pedidos del Cliente {0}", id);
            informe.Creador = "Sistema de Pedidos";
            informe.PalabrasClaves = String.Format("PDF informe listado pedidos cliente {0} mesas lt", id);
            informe.Asunto = "Informe";
            informe.Fecha = DateTime.Now;

            informe.Open();

            // Obtiene los datos desde la base.
            List<Pedido> pedidos = UOW.PedidoRepository.InformePedidosActivosPorCliente(id);

            // Comprueba si hay pedidos para mostrar.
            if (pedidos.FirstOrDefault() == null) informe.ListadoVacio();

            foreach (var pedido in pedidos)
            {
                informe.AgregarLinea(informe.GenerarLinea(pedido));
            }

            // Cierra los objetos requeridos.
            informe.Close();

            // Vuelca el contenido del stream en el explorador.
            return informe.SendFileResult();

        }

    }

}

namespace Pedidos.Controllers.Informes
{

    public interface ILectorDePedido
    {
        Tuple<string[], Dictionary<string, object>> LeePedido(Pedido pedido);
    }

    public interface IImpresorDePedido
    {
        PdfPTable ImprimePedido(string[] contenido, Dictionary<string, object> configuracion);
    }

    public class LectorDePedidosCompletos : ILectorDePedido
    {

        public virtual Tuple<string[], Dictionary<string, object>> LeePedido(Pedido pedido)
        {

            string[] contenido = new string[16];
            Dictionary<string, object> configuracion = new Dictionary<string, object>();

            Pedido pedidoOriginal;
            PedidosDbContext db = new PedidosDbContext();

            #region Rescate del Artículo

            Tuple<string[], Dictionary<string, object>> parArticulo;

            switch (pedido.Articulo.TipoDeArticulo)
            {
                case "Tapa":
                    parArticulo = LeeTapa((Tapa)pedido.Articulo);
                    break;

                case "Base":
                    parArticulo = LeeBase((Base)pedido.Articulo);
                    break;

                case "Vitrea":
                    parArticulo = LeeVitrea((Vitrea)pedido.Articulo);
                    break;

                case "FueraDeLista":
                    parArticulo = LeeFueraDeLista((FueraDeLista)pedido.Articulo);
                    break;

                default:
                    parArticulo = null;
                    break;
            }

            #endregion

            #region Rescate del Pedido

            // Primera Línea
            contenido[0] = parArticulo.Item1[0];
            contenido[1] = "CLI " + pedido.Gestion.ClienteId;
            contenido[2] = pedido.Cantidad.ToString("00") + " x " + parArticulo.Item1[2];
            contenido[3] = parArticulo.Item1[3];

            // Segunda Línea
            contenido[4] = parArticulo.Item1[4];
            contenido[5] = parArticulo.Item1[5];

            // Tercera Línea.
            contenido[6] = "PED " + pedido.PedidoId.ToString();
            contenido[7] = "";
            if (pedido.RegistroOriginalId != null)
            {
                pedidoOriginal = (from p in db.Pedidos.Include(p => p.Gestion).Include(p => p.Articulo)
                                  where p.PedidoId == pedido.RegistroOriginalId.Value
                                  select p).FirstOrDefault();
                contenido[7] = pedido.FechaBaja.Value.Day + " de " + pedido.FechaBaja.Value.ToString("MMMM") + " de " + pedido.FechaBaja.Value.ToString("yyyy") + " a las " + pedido.FechaBaja.Value.ToString("H:mm");
                contenido[7] = "--- [Historial del pedido " + pedido.RegistroOriginalId.Value + ", huella generada el " + contenido[7] + "] ---";
            }

            // Cuarta Línea.
            contenido[8] = pedido.Gestion.Cliente.ZonaNombre;
            contenido[9] = "Gestión " + pedido.GestionId.ToString() + " (" + pedido.Gestion.FechaGestion.ToString("dd/MM") + ")";
            contenido[10] = pedido.FechaEntrega.HasValue ? ("A Entregar el " + pedido.FechaEntrega.Value.ToString("dd/MM")) : " ";
            contenido[11] = String.IsNullOrEmpty(pedido.EstructuraSolicitada) ? "FUERA de LISTA" : ("Tango " + pedido.EstructuraSolicitada);
            contenido[12] = pedido.FechaBaja.HasValue ? ("Pedido DADO de BAJA el " + pedido.FechaBaja.Value.ToString("dd/MM")) : "Pedido ACTIVO";

            // Quinta Línea.
            contenido[13] = "Editado por " + pedido.UserName;
            contenido[14] = String.IsNullOrWhiteSpace(pedido.Referencia) ? "" : ("Referencia " + pedido.Referencia);
            contenido[15] = pedido.RequiereAprobacion ? "Resta Aprobación" : "";

            configuracion.Add("DadoDeBaja", pedido.FechaBaja.HasValue);

            #endregion

            #region Rescate de Adjuntos
                
            #endregion

            foreach (var item in parArticulo.Item2)
            {
                configuracion.Add(item.Key, item.Value);
            }

            return new Tuple<string[], Dictionary<string, object>>(contenido, configuracion);

        }

        private Tuple<string[], Dictionary<string, object>> LeeArticulo(Articulo articulo)
        {

            switch (articulo.TipoDeArticulo)
            {
                case "Tapa":
                    return LeeTapa((Tapa)articulo);

                case "Base":
                    return LeeBase((Base)articulo);

                case "Vitrea":
                    return LeeVitrea((Vitrea)articulo);

                case "FueraDeLista":
                    return LeeFueraDeLista((FueraDeLista)articulo);

                default:
                    return null;
            }

        }

        private Tuple<string[], Dictionary<string, object>> LeeTapa(Tapa tapa)
        {

            string[] contenido = new string[16];
            Dictionary<string, object> configuracion = new Dictionary<string, object>();
            configuracion.Add("ColorDelTipo", new BaseColor(255, 217, 102));

            // Primera Línea
            contenido[0] = tapa.TipoDeArticulo[0].ToString();
            contenido[1] = String.Empty;
            contenido[2] = tapa.ToString("Controllers.Informes.PedidosEnteros");
            contenido[3] = tapa.Laminado_MuestrarioId != null ? ("LAM " + tapa.Laminado_CodigoId + " (MUE " + tapa.Laminado_MuestrarioId.Value.ToString() + ")") : "";

            // Segunda Línea
            contenido[4] = tapa.Borde.ToString();
            contenido[5] = tapa.Particularidades ?? "";

            // Tercera Línea.
            contenido[6] = String.Empty;
            contenido[7] = String.Empty;

            // Cuarta Línea.
            contenido[8] = String.Empty;
            contenido[9] = String.Empty;
            contenido[10] = String.Empty;
            contenido[11] = String.Format("Tango {0}", tapa.CodigoTango);
            contenido[12] = String.Empty;

            // Quinta Línea.
            contenido[13] = String.Empty;
            contenido[14] = String.Empty;
            contenido[15] = String.Empty;

            return new Tuple<string[], Dictionary<string, object>>(contenido, configuracion);

        }

        private Tuple<string[], Dictionary<string, object>> LeeBase(Base _base)
        {

            string[] contenido = new string[16];
            Dictionary<string, object> configuracion = new Dictionary<string, object>();
            configuracion.Add("ColorDelTipo", new BaseColor(172, 185, 202));

            // Primera Línea
            contenido[0] = _base.TipoDeArticulo[0].ToString();
            contenido[1] = String.Empty;
            contenido[2] = _base.ToString("Optimizado");
            contenido[3] = _base.Particularidades ?? "";

            // Segunda Línea
            contenido[4] = _base.ProveedorNombre;
            contenido[5] = String.Empty;

            // Tercera Línea.
            contenido[6] = String.Empty;
            contenido[7] = String.Empty;

            // Cuarta Línea.
            contenido[8] = String.Empty;
            contenido[9] = String.Empty;
            contenido[10] = String.Empty;
            contenido[11] = String.Format("Tango {0}", _base.CodigoTango);
            contenido[12] = String.Empty;

            // Quinta Línea.
            contenido[13] = String.Empty;
            contenido[14] = String.Empty;
            contenido[15] = String.Empty;

            return new Tuple<string[], Dictionary<string, object>>(contenido, configuracion);

        }

        private Tuple<string[], Dictionary<string, object>> LeeVitrea(Vitrea vitrea)
        {

            string[] contenido = new string[16];
            Dictionary<string, object> configuracion = new Dictionary<string, object>();
            configuracion.Add("ColorDelTipo", new BaseColor(169, 208, 142));

            // Primera Línea
            contenido[0] = vitrea.TipoDeArticulo[0].ToString();
            contenido[1] = String.Empty;
            contenido[2] = vitrea.ToString("Optimizado");
            contenido[3] = vitrea.Particularidades ?? "";

            // Segunda Línea
            contenido[4] = String.Empty;
            contenido[5] = String.Empty;

            // Tercera Línea.
            contenido[6] = String.Empty;
            contenido[7] = String.Empty;

            // Cuarta Línea.
            contenido[8] = String.Empty;
            contenido[9] = String.Empty;
            contenido[10] = String.Empty;
            contenido[11] = String.Format("Tango {0}", vitrea.CodigoTango);
            contenido[12] = String.Empty;

            // Quinta Línea.
            contenido[13] = String.Empty;
            contenido[14] = String.Empty;
            contenido[15] = String.Empty;

            return new Tuple<string[], Dictionary<string, object>>(contenido, configuracion);

        }

        private Tuple<string[], Dictionary<string, object>> LeeFueraDeLista(FueraDeLista fueraDeLista)
        {

            string[] contenido = new string[16];
            Dictionary<string, object> configuracion = new Dictionary<string, object>();
            configuracion.Add("ColorDelTipo", new BaseColor(210, 147, 140));

            // Primera Línea
            contenido[0] = fueraDeLista.TipoDeArticulo[0].ToString();
            contenido[1] = String.Empty;
            contenido[2] = fueraDeLista.ToString("Controllers.Informes.PedidosEnteros");
            contenido[3] = String.Empty;

            // Segunda Línea
            contenido[4] = String.Empty;
            contenido[5] = fueraDeLista.Particularidades ?? "";

            // Tercera Línea.
            contenido[6] = String.Empty;
            contenido[7] = String.Empty;

            // Cuarta Línea.
            contenido[8] = String.Empty;
            contenido[9] = String.Empty;
            contenido[10] = String.Empty;
            contenido[11] = String.Format("Tango {0}", fueraDeLista.CodigoTango);
            contenido[12] = String.Empty;

            // Quinta Línea.
            contenido[13] = String.Empty;
            contenido[14] = String.Empty;
            contenido[15] = String.Empty;

            return new Tuple<string[], Dictionary<string, object>>(contenido, configuracion);

        }

    }

    public class ImpresorDePedidosCompletos : IImpresorDePedido
    {

        /// <summary>
        /// Arma la tabla y la completa con los datos recolectados previamente.
        /// </summary>
        /// <param name="contenido">Los datos de las distintas celdas de la tabla.</param>
        /// <param name="configuracion">Datos varios de configuración. Consultar.</param>
        /// <returns>Tabla con el pedido correctamente formateado y colocado.</returns>
        public virtual PdfPTable ImprimePedido(string[] contenido, Dictionary<string, object> configuracion)
        {

            PdfPTable _tabla = new PdfPTable(new float[] { 1, 1, 8, 10, 10, 10, 10, 10, 10 });
            _tabla.WidthPercentage = 100;
            PdfPCell _celda;
            Phrase _frase;

            BaseColor _colorDelTipoDeArticulo = (BaseColor)configuracion["ColorDelTipo"];
            BaseColor _colorVerdeOscuro = new BaseColor(84, 130, 53);
            Font _fuenteNegra = FontFactory.GetFont(BaseFont.HELVETICA, 8, BaseColor.BLACK);
            Font _fuenteDestacadaNegra = FontFactory.GetFont(BaseFont.TIMES_BOLD, 12, BaseColor.BLACK);
            Font _fuenteRoja = FontFactory.GetFont(BaseFont.HELVETICA, 8, BaseColor.RED);
            Font _fuenteVerde = FontFactory.GetFont(BaseFont.HELVETICA, 8, _colorVerdeOscuro);
            Font _fuenteLetra = FontFactory.GetFont(BaseFont.TIMES_ROMAN, 20, BaseColor.BLACK);

            #region Primera Línea

            _celda = new PdfPCell(new Phrase(contenido[0], _fuenteLetra));
            _celda.Rowspan = 2;
            _celda.Colspan = 2;
            _celda.BorrarFormato();
            _celda.BorderWidthLeft = 2f;
            _celda.BorderWidthBottom = 2f;
            _celda.BorderWidthTop = 2f;
            _celda.BackgroundColor = _colorDelTipoDeArticulo;
            _celda.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[1], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.Rowspan = 2;
            _celda.BorderWidthRight = 2f;
            _celda.BorderWidthTop = 2f;
            _celda.BackgroundColor = _colorDelTipoDeArticulo;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[2], _fuenteDestacadaNegra));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            _celda.Colspan = 4;
            _celda.BorderWidthTop = 1f;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[3], _fuenteDestacadaNegra));
            _celda.BorrarFormato();
            _celda.Colspan = 2;
            _celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            _celda.BorderWidthTop = 1f;
            _celda.BorderWidthRight = 1f;
            _tabla.AddCell(_celda);

            #endregion

            #region Segunda Línea

            _celda = new PdfPCell(new Phrase(contenido[4], _fuenteDestacadaNegra));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            _celda.Colspan = 3;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[5], _fuenteDestacadaNegra));
            _celda.BorrarFormato();
            _celda.Colspan = 3;
            _celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            _celda.BorderWidthRight = 1f;
            _tabla.AddCell(_celda);

            #endregion

            #region Tercera Línea

            _celda = new PdfPCell();
            _celda.BorrarFormato();
            _celda.Colspan = 2;
            _celda.Rowspan = 3;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[6], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.BorderWidthRight = 2f;
            _celda.BorderWidthLeft = 2f;
            _celda.BackgroundColor = _colorDelTipoDeArticulo;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[7], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.Colspan = 6;
            _celda.BorderWidthRight = 1f;
            _celda.BackgroundColor = new BaseColor(189, 215, 238);
            _celda.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            _tabla.AddCell(_celda);

            #endregion

            #region Cuarta Línea

            _celda = new PdfPCell(new Phrase(contenido[8], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.Rowspan = 2;
            _celda.BorderWidthRight = 2f;
            _celda.BorderWidthLeft = 2f;
            _celda.BorderWidthBottom = 2f;
            _celda.BackgroundColor = _colorDelTipoDeArticulo;
            _tabla.AddCell(_celda);

            _frase = new Phrase(contenido[9], _fuenteRoja);
            _celda = new PdfPCell(_frase);
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            _celda.Colspan = 2;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[10], _fuenteNegra));
            _celda.BorrarFormato();
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[11], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            _tabla.AddCell(_celda);

            _frase = new Phrase(contenido[12]);
            _frase.Chunks[0].Font = (bool)configuracion["DadoDeBaja"] ? _fuenteRoja : _fuenteVerde;
            _celda = new PdfPCell(_frase);
            _celda.BorrarFormato();
            _celda.Colspan = 2;
            _celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            _celda.BorderWidthRight = 1f;
            _tabla.AddCell(_celda);

            #endregion

            #region Quinta Línea

            _celda = new PdfPCell(new Phrase(contenido[13], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            _celda.Colspan = 3;
            _celda.BorderWidthBottom = 1f;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[14], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            _celda.Colspan = 2;
            _celda.BorderWidthBottom = 1f;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell(new Phrase(contenido[15], _fuenteNegra));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            _celda.BorderWidthBottom = 1f;
            _celda.BorderWidthRight = 1f;
            _tabla.AddCell(_celda);

            #endregion

            return _tabla;

        }

    }

    public class LectorDePedidosActivos : LectorDePedidosCompletos
    {

        public override Tuple<string[], Dictionary<string, object>> LeePedido(Pedido pedido)
        {

            var pedidoPreprocesado = base.LeePedido(pedido);

            var cantidadPendiente = pedido.Cantidad;
            cantidadPendiente = (pedido.SeguimientoGlobal != null ? pedido.SeguimientoGlobal.Detalle.Where(si => si.EtapaDelNegocioInternaId < 100 && si.FechaBaja == null).Sum(si => si.Cantidad) : cantidadPendiente);
            var campoSeparado = pedidoPreprocesado.Item1[2].Split(("x").ToCharArray());
            campoSeparado[0] = String.Format("{0} ", cantidadPendiente);
            pedidoPreprocesado.Item1[2] = String.Join("x", campoSeparado);

            return pedidoPreprocesado;

        }

    }

    public class ImpresorDePedidosActivos : ImpresorDePedidosCompletos { }

    public class LectorDePedidosDemorados : LectorDePedidosCompletos
    {

        public override Tuple<string[], Dictionary<string, object>> LeePedido(Pedido pedido)
        {

            var pedidoPreprocesado = base.LeePedido(pedido);

            var cantidadDemorada = pedido.SeguimientoGlobal.Detalle.Where(si => si.EtapaDelNegocioInternaId == 2 && si.FechaBaja == null).FirstOrDefault();
            var campoSeparado = pedidoPreprocesado.Item1[2].Split(("x").ToCharArray());
            campoSeparado[0] = String.Format("{0} ", (cantidadDemorada != null ? cantidadDemorada.Cantidad : 0));
            pedidoPreprocesado.Item1[2] = String.Join("x", campoSeparado);

            return pedidoPreprocesado;

        }

    }

    public class ImpresorDePedidosDemorados : ImpresorDePedidosCompletos { }

    public abstract class Informe
    {

        MemoryStream _memoryStream;
        Document _document;
        PdfWriter _writer;
        IEncabezado _encabezado;

        // Almacena el margen superior original.
        // El margen final, efectivo, será el que resulte de adicionar el alto del encabezado.
        // Al setear un nuevo encabezado, el sistema deberá volver a tener en cuenta este valor.
        float _topMargin = 0;

        public Informe(Rectangle pageSize, float leftMargin = 0, float rightMargin = 0, float topMargin = 0, float bottomMargin = 0)
        {

            // El margen superior queda almacenado aparte.
            _topMargin = topMargin;

            // Se setea un nuevo documento con los datos requeridos.
            NuevoDocumento(pageSize, leftMargin, rightMargin, topMargin, bottomMargin);

        }

        private void NuevoDocumento(Rectangle pageSize, float leftMargin = 0, float rightMargin = 0, float topMargin = 0, float bottomMargin = 0)
        {

            // Se crea un nuevo stream en memoria.
            _memoryStream = new MemoryStream();

            // Si existe un encabezado seteado, se calcula su alto.
            float _altoEncabezado = 0;
            if (_encabezado != null) _altoEncabezado = _encabezado.Alto(pageSize, leftMargin, rightMargin);

            // Se crea un nuevo documento teniendo en cuenta el alto del encabezado.
            _document = new Document(pageSize, leftMargin, rightMargin, topMargin + _altoEncabezado, bottomMargin);
            _writer = PdfWriter.GetInstance(_document, _memoryStream);
            _writer.CloseStream = false;

            // Se setea este dato en base a una constante.
            this.Empresa = Informes.Constantes.EMPRESA;

        }

        public Document Document
        {
            get
            {
                return _document;
            }
        }

        public PdfWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public string Empresa { get; set; }

        public string Titulo { get; set; }

        public string Creador { get; set; }

        public DateTime Fecha { get; set; }

        public string PalabrasClaves { get; set; }

        public string Asunto { get; set; }

        public IEncabezado Encabezado
        {

            get
            {
                return _encabezado;
            }

            set
            {

                // Setea el nuevo encabezado.
                _encabezado = value;

                // Regenera el documento tomando como base los valores del documento actual
                // pero reiniciando el margen superior al valor original,
                // valor que no contiene incluido alto de encabezado alguno.
                this.NuevoDocumento(this.Document.PageSize, this.Document.LeftMargin, this.Document.RightMargin, _topMargin, this.Document.BottomMargin);

                // Setea el Writer con el nuevo encabezado.
                Writer.PageEvent = Encabezado;

            }

        }

        public virtual void Open()
        {

            // Agrega metadatos.
            if (this.Titulo != null) this.Document.AddTitle(this.Titulo);
            if (this.Asunto != null) this.Document.AddSubject(this.Asunto);
            if (this.Empresa != null) this.Document.AddAuthor(this.Empresa);
            if (this.Creador != null) this.Document.AddCreator(this.Creador);
            if (this.PalabrasClaves != null) this.Document.AddKeywords(this.PalabrasClaves);

            this.Document.Open();

        }

        public virtual void Close()
        {

            // Cierra objetos y posiciona el cursor al inicio.
            this.Document.Close();
            this.Writer.Close();
            _memoryStream.Position = 0;

        }

        public FileStreamResult SendFileResult()
        {

            // Prepara el documento actual para ser devuelto como un resultado MVC
            // y ser enviado al explorador como un archivo PDF.
            return new FileStreamResult(_memoryStream, "application/pdf");

        }

    }

    public abstract class InformeDePedidos : Informe
    {

        protected ILectorDePedido lectorDePedido;
        protected IImpresorDePedido impresorDePedido;

        // Verdadero cuando la próxima linea a escribir es la primera de la página.
        bool _primeraLineaEnPagina = true;

        // Los títulos y los subtítulos son manejados a través clases "Seccion"
        Seccion _titulo = new Seccion();
        Seccion _subTitulo = new Seccion();

        // Objetos que almacenarán las referencias a las páginas que se usarán para generar los índices.
        Dictionary<string, int> _indiceTitulos = new Dictionary<string, int>();
        Dictionary<string, int> _indiceSubtitulos = new Dictionary<string, int>();

        public InformeDePedidos(Rectangle pageSize, float leftMargin = 0, float rightMargin = 0, float topMargin = 0, float bottomMargin = 0)
            : base(pageSize, leftMargin, rightMargin, topMargin, bottomMargin)
        {

            // Fuentes por defecto.
            Font _fuenteSeccion = FontFactory.GetFont(BaseFont.HELVETICA, 14, BaseColor.BLACK); ;
            Font _fuenteSubseccion = FontFactory.GetFont(BaseFont.HELVETICA, 11, BaseColor.BLACK);

            // Setea el espacio entre lineas por defecto.
            this.EspacioEntreLineas = 20;

            // Setea títulos y subtítulos con datos por defecto.
            _titulo.Setear(_fuenteSeccion, 72, 36, true);
            _subTitulo.Setear(_fuenteSubseccion, 36, 18, false);

        }

        /// <summary>
        /// Espacio en blanco que se considerará entre dos líneas consecutivas.
        /// </summary>
        protected float EspacioEntreLineas { get; set; }

        /// <summary>
        /// Prepara y deja listo un nuevo título para ser impreso en el documento.
        /// </summary>
        /// <param name="nombre">Texto a imprimir (título)</param>
        public void DisponerTitulo(string nombre)
        {
            _titulo.Disponer(nombre);
        }

        /// <summary>
        /// Prepara y deja listo un nuevo subtítulo para ser impreso en el documento.
        /// </summary>
        /// <param name="nombre">Texto a imprimir (subtítulo)</param>
        public void DisponerSubtitulo(string nombre)
        {
            _subTitulo.Disponer(nombre);
        }

        /// <summary>
        /// Setea las características de los títulos que se usarán de aquí en más y que por lo general no será necesario modificar.
        /// </summary>
        /// <param name="fuente">Fuente del título.</param>
        /// <param name="espaciadoAnterior">Espacio que se dispondrá por encima del título. Este espacio se omitirá si hay un título de mayor nivel inmediatamente antes, o la página está en cero.</param>
        /// <param name="espaciadoPosterior">Espacio que se dispondrá por debajo del título.</param>
        /// <param name="saltaPagina">Especifica si cada título deberá aparecer en una nueva página. Si es verdadero, antes de imprimir el mismo se procede a generar un salto.</param>
        protected void SetearTitulo(Font fuente, float espaciadoAnterior, float espaciadoPosterior, bool saltaPagina)
        {
            _titulo.Setear(fuente, espaciadoAnterior, espaciadoPosterior, saltaPagina);
        }

        /// <summary>
        /// Setea las características de los subtítulos que se usarán de aquí en más y que por lo general no será necesario modificar.
        /// </summary>
        /// <param name="fuente">Fuente del subtítulo.</param>
        /// <param name="espaciadoAnterior">Espacio que se dispondrá por encima del subtítulo. Este espacio se omitirá si hay un título/subtítulo de mayor nivel inmediatamente antes, o la página está en cero.</param>
        /// <param name="espaciadoPosterior">Espacio que se dispondrá por debajo del subtítulo.</param>
        protected void SetearSubtitulo(Font fuente, float espaciadoAnterior, float espaciadoPosterior)
        {
            _subTitulo.Setear(fuente, espaciadoAnterior, espaciadoPosterior, false);
        }

        /// <summary>
        /// Genera una tabla vacía que sirve al solo hecho de generar un espacio en blanco en el documento.
        /// </summary>
        /// <param name="espacio">Espacio a considerar (que no es más que la altura de la tabla)</param>
        /// <returns>Una tabla de la altura especificada.</returns>
        protected PdfPTable TablaEspaciadora(float espacio)
        {
            PdfPTable _tabla = new PdfPTable(1);
            PdfPCell _celda = new PdfPCell();
            _celda.MinimumHeight = espacio;
            _celda.BorrarFormato();
            _tabla.AddCell(_celda);
            return _tabla;
        }

        /// <summary>
        /// Agrega una nueva línea al documento considerando los títulos o subtítulos pendientes, gestionando correctamente el espaciado.
        /// </summary>
        /// <param name="element">Elemento que representa la línea a agregar.</param>
        public void AgregarLinea(IElement element)
        {

            // Inicio
            float _altoDelElemento = element.CalcularAlto(this.Document);

            // Comprueba si se debe hacer un salto de página en cada título nuevo.
            if (_titulo.Activada && _titulo.SaltaPagina && !_primeraLineaEnPagina)
            {
                this.Document.NewPage();
                _primeraLineaEnPagina = true;
            }

            // Obtiene la posición vertical actual.
            float _posicionDePartida = this.Writer.GetVerticalPosition(false);

            // Si corresponde, setea el título.
            if (_titulo.Activada)
            {
                if (_primeraLineaEnPagina) _titulo.ComienzaSinEspaciado = true;
                _titulo.AlturaDelTexto = _titulo.Disparar().CalcularAlto(this.Document);
            }

            // Si corresponde, setea el subtítulo.
            if (_subTitulo.Activada)
            {
                if (_titulo.Activada || !_titulo.Activada && _primeraLineaEnPagina) _subTitulo.ComienzaSinEspaciado = true;
                _subTitulo.AlturaDelTexto = _subTitulo.Disparar().CalcularAlto(this.Document);
            }

            // Decide si el elemento comienza con el espacio habitual.
            bool _agregaEspaciadoNormal = (!_titulo.Activada && !_subTitulo.Activada && !_primeraLineaEnPagina);
            float _espaciadoNormal = _agregaEspaciadoNormal ? EspacioEntreLineas : 0;

            // Si los datos a agregar no caben en la página,
            if (_posicionDePartida - _titulo.AlturaFinal - _subTitulo.AlturaFinal - _espaciadoNormal - _altoDelElemento < this.Document.BottomMargin)
            {

                // Se agregan en la siguiente.
                this.Document.NewPage();

                // La línea pasa a ser entonces la primera de la página.
                _primeraLineaEnPagina = true;

                // Se quita el espaciado del primer componente mostrado.
                if (_titulo.Activada) _titulo.ComienzaSinEspaciado = true;
                else if (_subTitulo.Activada) _subTitulo.ComienzaSinEspaciado = true;
                else
                {
                    _agregaEspaciadoNormal = false;
                    _espaciadoNormal = 0;
                }

            }

            // Actualiza el índice.
            if (_titulo.Activada) _indiceTitulos.Add(_titulo.Nombre, this.Writer.PageNumber);
            if (_subTitulo.Activada) _indiceSubtitulos.Add(_subTitulo.Nombre, this.Writer.PageNumber);

            // Agrega el título.
            if (_titulo.Activada && !_titulo.ComienzaSinEspaciado) this.Document.Add(this.TablaEspaciadora(_titulo.EspaciadoAnterior));
            if (_titulo.Activada) this.Document.Add(_titulo.Disparar());
            if (_titulo.Activada) this.Document.Add(this.TablaEspaciadora(_titulo.EspaciadoPosterior));

            // Agrega el subtítulo.
            if (_subTitulo.Activada && !_subTitulo.ComienzaSinEspaciado) this.Document.Add(this.TablaEspaciadora(_subTitulo.EspaciadoAnterior));
            if (_subTitulo.Activada) this.Document.Add(_subTitulo.Disparar());
            if (_subTitulo.Activada) this.Document.Add(this.TablaEspaciadora(_subTitulo.EspaciadoPosterior));

            // Agrega la línea.
            if (_agregaEspaciadoNormal) this.Document.Add(this.TablaEspaciadora(this.EspacioEntreLineas));
            this.Document.Add(element);

            // Termina.
            _primeraLineaEnPagina = false;
            _titulo.Resetear();
            _subTitulo.Resetear();

        }

        public PdfPTable GenerarLinea(Pedido pedido)
        {
            var pedidoLeido = lectorDePedido.LeePedido(pedido);
            return impresorDePedido.ImprimePedido(pedidoLeido.Item1, pedidoLeido.Item2);
        }

        /// <summary>
        /// Agrega una nueva página con el índice de títulos. Permite al usuario saber en qué página está cada título.
        /// </summary>
        public void AgregarIndiceDeTitulos()
        {

            PdfPTable _tabla = new PdfPTable(new float[] { 80, 20 });
            _tabla.WidthPercentage = 60;
            PdfPCell _celda;
            BaseColor _fondoGris = new BaseColor(235, 235, 235);
            Font _fuente = FontFactory.GetFont(BaseFont.HELVETICA, 12, BaseColor.BLACK);

            if (_indiceTitulos.Count == 0) return;
            _indiceTitulos = _indiceTitulos.OrderBy(tit => tit.Key).ToDictionary(k => k.Key, v => v.Value);

            foreach (var i in _indiceTitulos)
            {

                _celda = new PdfPCell(new Phrase(i.Key, _fuente));
                _celda.BorrarFormato();
                _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                if (_tabla.Rows.Count % 2 == 0) _celda.BackgroundColor = _fondoGris;
                _tabla.AddCell(_celda);

                _celda = new PdfPCell(new Phrase(i.Value.ToString(), _fuente));
                _celda.BorrarFormato();
                _celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                if (_tabla.Rows.Count % 2 == 0) _celda.BackgroundColor = _fondoGris;
                _tabla.AddCell(_celda);

            }

            this.SetearTitulo(_titulo.Fuente, _titulo.EspaciadoAnterior, _titulo.EspaciadoPosterior, true);
            this.DisponerTitulo("Índice de Zonas");
            this.AgregarLinea(_tabla);

        }

        /// <summary>
        /// Agrega una nueva página con el índice de subtítulos. Permite al usuario saber en qué página está cada subtítulo.
        /// </summary>
        public void AgregarIndiceDeSubtitulos()
        {

            PdfPTable _tabla = new PdfPTable(new float[] { 80, 20 });
            _tabla.WidthPercentage = 60;
            PdfPCell _celda;
            BaseColor _fondoGris = new BaseColor(235, 235, 235);
            Font _fuente = FontFactory.GetFont(BaseFont.HELVETICA, 12, BaseColor.BLACK);

            if (_indiceSubtitulos.Count == 0) return;
            _indiceSubtitulos = _indiceSubtitulos.OrderBy(tit => tit.Key).ToDictionary(k => k.Key, v => v.Value);

            foreach (var i in _indiceSubtitulos)
            {

                _celda = new PdfPCell(new Phrase(i.Key, _fuente));
                _celda.BorrarFormato();
                _celda.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                if (_tabla.Rows.Count % 2 == 0) _celda.BackgroundColor = _fondoGris;
                _tabla.AddCell(_celda);

                _celda = new PdfPCell(new Phrase(i.Value.ToString(), _fuente));
                _celda.BorrarFormato();
                _celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                if (_tabla.Rows.Count % 2 == 0) _celda.BackgroundColor = _fondoGris;
                _tabla.AddCell(_celda);

            }

            this.SetearTitulo(_titulo.Fuente, _titulo.EspaciadoAnterior, _titulo.EspaciadoPosterior, true);
            this.DisponerTitulo("Índice de Clientes");
            this.AgregarLinea(_tabla);

        }

    }

    public class InformeEnTabla : Informe
    {

        PdfPTable _tabla;
        List<Columna> _columnas;

        Font _fuenteDatos = FontFactory.GetFont(BaseFont.HELVETICA, 10, BaseColor.BLACK);
        BaseColor _bgColorDatos = new BaseColor(255, 255, 255);

        Font _fuenteEncabezado = FontFactory.GetFont(BaseFont.HELVETICA, 10, BaseColor.WHITE);
        BaseColor _bgColorEncabezado = new BaseColor(117, 113, 113);

        public InformeEnTabla(Rectangle pageSize, float leftMargin = 0, float rightMargin = 0, float topMargin = 0, float bottomMargin = 0) : base(pageSize, leftMargin, rightMargin, topMargin, bottomMargin) { }

        /// <summary>
        /// Crea la tabla con la estructura de columnas especificada.
        /// </summary>
        /// <param name="columnas">Datos de las columnas a crear.</param>
        public void CrearTabla(List<Columna> columnas)
        {

            // Comprobaciones iniciales.
            if (_tabla == null && columnas != null && columnas.Count > 0)
            {

                _tabla = new PdfPTable(columnas.Select(c => c.AnchoRelativo).ToArray());
                _tabla.WidthPercentage = 100;
                _columnas = columnas;

                // Agrega la linea de encabezado.
                foreach (Columna columna in columnas)
                {
                    _tabla.AddCell(this.CeldaEncabezado(columna.Contenido, columna.AlineacionHorizontal));
                }

                // Agrega una linea de separación.
                PdfPCell _celda = this.CeldaDatos(" ");
                _celda.Colspan = columnas.Count;
                _tabla.AddCell(_celda);

                _tabla.HeaderRows = 2;
                _tabla.FooterRows = 0;

            }

        }

        /// <summary>
        /// Devuelve una celda con el formato definido como usual (en contraposición con una celda con el formato de encabezado)
        /// </summary>
        /// <param name="contenido">Texto de la celda.</param>
        /// <param name="alineacion">Alineación solicitada. En caso de omisión, quedará centrada.</param>
        /// <returns>Celda inicializada en contenido y formato.</returns>
        public PdfPCell CeldaDatos(string contenido, int alineacion = PdfPCell.ALIGN_CENTER)
        {
            var _celda = new PdfPCell(new Phrase(contenido, this._fuenteDatos));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = alineacion;
            _celda.BackgroundColor = this._bgColorDatos;
            return _celda;
        }

        /// <summary>
        /// Devuelve una celda con el formato de encabezado.
        /// </summary>
        /// <param name="contenido">Texto de la celda.</param>
        /// <param name="alineacion">Alineación solicitada. En caso de omisión, quedará centrada.</param>
        /// <returns>Celda inicializada en contenido y formato.</returns>
        public PdfPCell CeldaEncabezado(string contenido, int alineacion = PdfPCell.ALIGN_CENTER)
        {
            var _celda = new PdfPCell(new Phrase(contenido, this._fuenteEncabezado));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = alineacion;
            _celda.BackgroundColor = this._bgColorEncabezado;
            return _celda;
        }

        /// <summary>
        /// Agrega una fila de datos entera, automatizando el proceso en la medida de lo posible.
        /// </summary>
        /// <param name="contenidos">Cada uno de los distintos contenidos de las celdas de la fila.</param>
        public void AgregarFila(List<string> contenidos)
        {

            // Comprobaciones previas.
            if (contenidos != null && contenidos.Count == _columnas.Count)
            {
                for (int indice = 0; indice < contenidos.Count(); indice++)
                {
                    _tabla.AddCell(this.CeldaDatos(contenidos[indice], _columnas[indice].AlineacionHorizontal));
                }
            }

            // Ha surgido un error.
            else
            {
                throw new Exception("La variable con los datos de la fila a agregar es nula, o el número de celdas a agregar no coincide con el número de columnas de la tabla.");
            }

        }

        /// <summary>
        /// Abre el documento, adicionándole previamente el encabezado que corresponde.
        /// </summary>
        public override void Open()
        {
            this.Encabezado = new EncabezadoEstandar(this.Empresa, this.Titulo, this.Fecha);
            base.Open();
        }

        /// <summary>
        /// Agrega la tabla al documento y posteriormente cierra todos los objetos en curso.
        /// </summary>
        public override void Close()
        {
            this.Document.Add(_tabla);
            base.Close();
        }

        /// <summary>
        /// Permite obtener una referencia a la tabla en proceso.
        /// </summary>
        public PdfPTable Tabla
        {
            get
            {
                return _tabla;
            }
        }

    }

    public class InformeDePedidosActivos : InformeDePedidos
    {

        public InformeDePedidosActivos(Rectangle pageSize, float leftMargin = 0, float rightMargin = 0, float topMargin = 0, float bottomMargin = 0)
            : base(pageSize, leftMargin, rightMargin, topMargin, bottomMargin)
        {

            lectorDePedido = new LectorDePedidosActivos();
            impresorDePedido = new ImpresorDePedidosActivos();

            Font _fuenteSeccion = FontFactory.GetFont(BaseFont.TIMES_ROMAN, 18, BaseColor.BLACK);
            Font _fuenteSubseccion = FontFactory.GetFont(BaseFont.HELVETICA, 12, BaseColor.BLACK);

            // Setea el espacio entre pedidos.
            EspacioEntreLineas = 20;

            // Setea el modo de funcionamiento de los títulos y los subtítulos.
            SetearTitulo(_fuenteSeccion, 60, 30, false);
            SetearSubtitulo(_fuenteSubseccion, 30, 15);

        }

        /// <summary>
        /// Genera una tabla y la agrega al documento,
        /// indicando que no hay datos que mostrar.
        /// </summary>
        public void ListadoVacio()
        {
            PdfPTable _tabla = new PdfPTable(1);
            PdfPCell _celda = new PdfPCell(new Phrase("Listado Vacío"));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            _tabla.AddCell(_celda);
            AgregarLinea(_tabla);
        }

        /// <summary>
        /// Abre el documento, adicionándole previamente el encabezado que corresponde.
        /// </summary>
        public override void Open()
        {
            this.Encabezado = new EncabezadoEstandar(this.Empresa, this.Titulo, this.Fecha);
            base.Open();
        }

    }

    public class InformeDePedidosDemorados : InformeDePedidos
    {

        public InformeDePedidosDemorados(Rectangle pageSize, float leftMargin = 0, float rightMargin = 0, float topMargin = 0, float bottomMargin = 0)
            : base(pageSize, leftMargin, rightMargin, topMargin, bottomMargin)
        {

            lectorDePedido = new LectorDePedidosDemorados();
            impresorDePedido = new ImpresorDePedidosDemorados();

            Font _fuenteSeccion = FontFactory.GetFont(BaseFont.TIMES_ROMAN, 18, BaseColor.BLACK);
            Font _fuenteSubseccion = FontFactory.GetFont(BaseFont.HELVETICA, 12, BaseColor.BLACK);

            // Setea el espacio entre pedidos.
            EspacioEntreLineas = 20;

            // Setea el modo de funcionamiento de los títulos y los subtítulos.
            SetearTitulo(_fuenteSeccion, 60, 30, false);
            SetearSubtitulo(_fuenteSubseccion, 30, 15);

        }

        /// <summary>
        /// Genera una tabla y la agrega al documento,
        /// indicando que no hay datos que mostrar.
        /// </summary>
        public void ListadoVacio()
        {
            PdfPTable _tabla = new PdfPTable(1);
            PdfPCell _celda = new PdfPCell(new Phrase("Listado Vacío"));
            _celda.BorrarFormato();
            _celda.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            _tabla.AddCell(_celda);
            AgregarLinea(_tabla);
        }

        /// <summary>
        /// Abre el documento, adicionándole previamente el encabezado que corresponde.
        /// </summary>
        public override void Open()
        {
            this.Encabezado = new EncabezadoEstandar(this.Empresa, this.Titulo, this.Fecha);
            base.Open();
        }

    }

    /// <summary>
    /// Representa a ciertos datos de una columna de una tabla.
    /// </summary>
    public class Columna
    {

        public float AnchoRelativo { get; set; }

        public string Contenido { get; set; }

        public int AlineacionHorizontal { get; set; }

    }

    /// <summary>
    /// Representa el encabezado de una nueva sección en el documento.
    /// </summary>
    public class Seccion
    {

        public Seccion()
        {
            // Setea los datos por default.
            this.Resetear();
        }

        // Almacena el valor pretendido para el espacio en blanco
        // que se dispondrá previo al texto del título.
        float _espaciadoAnterior = 0;

        /// <summary>
        /// Fuente de la sección.
        /// </summary>
        public Font Fuente { get; set; }

        /// <summary>
        /// Texto o nombre de la sección.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Indica si esta sección debe procesarse durante la escritura de la próxima linea.
        /// </summary>
        public bool Activada { get; set; }

        /// <summary>
        /// Almacena la altura que tendrá el texto al disponerse dentro del documento.
        /// </summary>
        public float AlturaDelTexto { get; set; }

        /// <summary>
        /// Premite saber cuál será la altura final de la sección, sumando el espacio anterior (si corresponde), el posterior y la altura del texto en sí.
        /// Si la sección no está activada, devuelve cero.
        /// </summary>
        public float AlturaFinal
        {
            get
            {
                if (Activada) return (ComienzaSinEspaciado ? 0 : EspaciadoAnterior) + AlturaDelTexto + EspaciadoPosterior;
                else return 0;
            }
        }

        /// <summary>
        /// Establece si la sección debe comenzar en una página nueva.
        /// </summary>
        public bool SaltaPagina { get; set; }

        /// <summary>
        /// Almacena el espacio que se dispondrá por encima del texto de la sección. Sea cual fuere el valor que se ingrese,
        /// si la sección está marcada para que comience sin este espaciado, su valor al intentar obtener el dato será cero.
        /// </summary>
        public float EspaciadoAnterior
        {
            get
            {
                return (this.ComienzaSinEspaciado ? 0 : _espaciadoAnterior);
            }
            set
            {
                _espaciadoAnterior = value;
            }
        }

        /// <summary>
        /// Almacena la altura del espacio en blanco que se dispondrá por debajo del texto.
        /// </summary>
        public float EspaciadoPosterior { get; set; }

        /// <summary>
        /// Indica si al escribirse la sección, debe omitirse el espaciado anterior.
        /// </summary>
        public bool ComienzaSinEspaciado { get; set; }

        /// <summary>
        /// Setea los datos iniciales de la sección, datos que usualmente no se han de modificar.
        /// </summary>
        /// <param name="fuente">Fuente a utilizar en la escritura del nombre.</param>
        /// <param name="espaciadoAnterior">Altura del espaciado que se dispondrá por encima del nombre.</param>
        /// <param name="espaciadoPosterior">Altura del espaciado que se dispondrá por debajo del nombre.</param>
        /// <param name="saltaPagina">Indica si la sección debe comenzar en una nueva página.</param>
        public void Setear(Font fuente, float espaciadoAnterior, float espaciadoPosterior, bool saltaPagina)
        {
            this.Fuente = fuente;
            this.EspaciadoAnterior = espaciadoAnterior;
            this.EspaciadoPosterior = espaciadoPosterior;
            this.SaltaPagina = saltaPagina;
        }

        /// <summary>
        /// Indica que se desea establecer una nueva sección, que se hará efectiva previo al procesamiento de la siguiente linea.
        /// </summary>
        /// <param name="nombre">Nombre de la sección.</param>
        public void Disponer(string nombre)
        {
            this.Nombre = nombre;
            this.Activada = true;
        }

        /// <summary>
        /// Resetea ciertas variables a sus valores por defecto.
        /// </summary>
        public void Resetear()
        {
            this.Activada = false;
            this.AlturaDelTexto = 0;
            this.ComienzaSinEspaciado = false;
            this.Nombre = "";
        }

        /// <summary>
        /// Genera el párrafo con el texto de la sección.
        /// </summary>
        public Paragraph Disparar()
        {
            return new Paragraph(this.Nombre, this.Fuente);
        }

    }

    /// <summary>
    /// Encabezado básico.
    /// </summary>
    public class EncabezadoEstandar : IEncabezado
    {

        string[] header = new string[4];
        int pageNumber;
        float espacioPosterior;
        Font fuente;
        Font fuenteMenor;
        Font fuenteTitulo;
        float _miAltoTotal;

        public EncabezadoEstandar(string empresa, string titulo, DateTime fecha)
        {

            header[0] = empresa;
            header[1] = titulo;
            header[2] = fecha.ToString("dd/MM/yyyy HH:mm:ss");
            header[3] = "Página {0}";

            // Espacio que se dispondrá por debajo del encabezado con el fin de separar el mismo de los datos del documento.
            espacioPosterior = 10;

            fuente = FontFactory.GetFont(BaseFont.HELVETICA, 10, BaseColor.BLACK);
            fuenteMenor = FontFactory.GetFont(BaseFont.HELVETICA, fuente.Size - 1, fuente.Color);
            fuenteTitulo = FontFactory.GetFont(BaseFont.TIMES_ROMAN, fuente.Size * 2, fuente.Color);

        }

        public void OnChapter(PdfWriter writer, Document document, float paragraphPosition, Paragraph title) { }

        public void OnChapterEnd(PdfWriter writer, Document document, float paragraphPosition) { }

        public void OnCloseDocument(PdfWriter writer, Document document) { }

        public void OnGenericTag(PdfWriter writer, Document document, Rectangle rect, string text) { }

        public void OnOpenDocument(PdfWriter writer, Document document) { }

        public void OnParagraph(PdfWriter writer, Document document, float paragraphPosition) { }

        public void OnParagraphEnd(PdfWriter writer, Document document, float paragraphPosition) { }

        public void OnSection(PdfWriter writer, Document document, float paragraphPosition, int depth, Paragraph title) { }

        public void OnSectionEnd(PdfWriter writer, Document document, float paragraphPosition) { }

        public void OnStartPage(PdfWriter writer, Document document)
        {
            pageNumber++;
            if (_miAltoTotal == 0) _miAltoTotal = this.Alto(document.PageSize, document.LeftMargin, document.RightMargin);
        }

        public void OnEndPage(PdfWriter writer, Document document)
        {
            this.GenerarTabla(pageNumber, document).WriteSelectedRows(0, 2, document.Left, document.Top + _miAltoTotal, writer.DirectContent);
        }

        /// <summary>
        /// Arma el encabezado por medio de una tabla.
        /// </summary>
        /// <param name="pagina">Indica el número de página que se debe imprimir en el encabezado.</param>
        /// <param name="document">Referencia al documento actual, a través del cual se leerán ciertas propiedades.</param>
        /// <returns>Devuelve una tabla con el encabezado armado.</returns>
        public PdfPTable GenerarTabla(int pagina, Document document)
        {

            PdfPTable _tabla = new PdfPTable(new float[] { 25, 50, 25 });
            _tabla.TotalWidth = document.Right - document.Left;
            float _espacioHastaLinea = 8;
            PdfPCell _celda;
            Paragraph _parrafo;

            #region Primera Fila

            _celda = new PdfPCell();
            _celda.BorrarFormato();
            _parrafo = new Paragraph(header[0], fuente);
            _parrafo.Alignment = Element.ALIGN_LEFT;
            _celda.AddElement(_parrafo);
            _celda.VerticalAlignment = PdfPCell.ALIGN_TOP;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell();
            _celda.BorrarFormato();
            _parrafo = new Paragraph(header[1], fuenteTitulo);
            _parrafo.Alignment = Element.ALIGN_CENTER;
            _celda.AddElement(_parrafo);
            _parrafo = new Paragraph(header[2], fuenteMenor);
            _parrafo.Alignment = Element.ALIGN_CENTER;
            _celda.AddElement(_parrafo);
            _celda.VerticalAlignment = PdfPCell.ALIGN_TOP;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell();
            _celda.BorrarFormato();
            _parrafo = new Paragraph(String.Format(header[3], pagina), fuente);
            _parrafo.Alignment = Element.ALIGN_RIGHT;
            _celda.AddElement(_parrafo);
            _celda.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
            _celda.VerticalAlignment = PdfPCell.ALIGN_TOP;
            _tabla.AddCell(_celda);

            #endregion

            _celda = new PdfPCell();
            _celda.MinimumHeight = _espacioHastaLinea;
            _celda.Colspan = 3;
            _celda.Border = PdfPCell.BOTTOM_BORDER;
            _tabla.AddCell(_celda);

            _celda = new PdfPCell();
            _celda.BorrarFormato();
            _celda.MinimumHeight = espacioPosterior;
            _celda.Colspan = 3;
            _tabla.AddCell(_celda);

            return _tabla;

        }

        /// <summary>
        /// Calcula el alto del encabezado.
        /// </summary>
        /// <param name="pageSize">Tamaño del papel. Debe coincidir con el tamaño real del papel del documento para que el cálculo sea preciso.</param>
        /// <param name="leftMargin">Margen izquierdo. Debe coincidir con el valor real del margen izquierdo del documento para que el cálculo sea preciso.</param>
        /// <param name="rightMargin">Margen derecho. Debe coincidir con el valor real del margen derecho del documento para que el cálculo sea preciso.</param>
        /// <returns>Altura del encabezado.</returns>
        public float Alto(Rectangle pageSize, float leftMargin, float rightMargin)
        {
            using (var doc = new Document(pageSize, leftMargin, rightMargin, 0, 0))
            {
                doc.Open();
                return GenerarTabla(999, doc).CalcularAlto(doc);
            }
        }

    }

    /// <summary>
    /// Métodos de extensión para el controlador Informes.
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Calcula el alto de un elemento dado insertándolo en un documento temporal,
        /// única forma de conocer el valor real. Se conoce un inconveniente con los objetos
        /// Chunk y Phrase. Como solución temporal, debe trabajarse con objetos Paragraph.
        /// </summary>
        /// <param name="element">Elemento del cual se calculará su alto.</param>
        /// <param name="document">Referencia al documento real donde el elemento tendrá influencia.</param>
        /// <returns>Altura del elemento.</returns>
        public static float CalcularAlto(this IElement element, Document document)
        {

            using (MemoryStream ms = new MemoryStream())
            {

                using (Document doc = new Document(document.PageSize, document.LeftMargin, document.RightMargin, 0, 0))
                {

                    using (PdfWriter w = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();
                        doc.Add(element);
                        float alto = doc.PageSize.GetTop(0) - w.GetVerticalPosition(false);
                        doc.Close();
                        return alto;
                    }

                }

            }

        }

        /// <summary>
        /// Quita los bordes de una celda y setea su contenido al centro, tanto vertical como horizontalmente. 
        /// </summary>
        /// <param name="celda">Celda de la cual se borrará su formato.</param>
        public static void BorrarFormato(this PdfPCell celda)
        {

            celda.Border = PdfPCell.NO_BORDER;
            celda.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            celda.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            celda.BorderWidthLeft = 0f;
            celda.BorderColorLeft = BaseColor.BLACK;
            celda.BorderWidthBottom = 0f;
            celda.BorderColorBottom = BaseColor.BLACK;
            celda.BorderWidthRight = 0f;
            celda.BorderColorRight = BaseColor.BLACK;
            celda.BorderWidthTop = 0f;
            celda.BorderColorTop = BaseColor.BLACK;
        }


    }

    /// <summary>
    /// Constantes del controlador Informes.
    /// </summary>
    public static class Constantes
    {

        public const string EMPRESA = "Mesas LT SRL";

    }

    /// <summary>
    /// Interface que describe un encabezado.
    /// </summary>
    public interface IEncabezado : IPdfPageEvent
    {

        PdfPTable GenerarTabla(int pagina, Document document);

        float Alto(Rectangle pageSize, float leftMargin, float rightMargin);

    }

}