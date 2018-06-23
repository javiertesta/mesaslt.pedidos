using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace Pedidos.Models
{
    
    [Table("ArchivosEnDisco")]
    public abstract class ArchivoEnDisco
    {

        public ArchivoEnDisco()
        {
            this.Ubicacion = this.Ubicacion = System.Web.Hosting.HostingEnvironment.MapPath("~/Archivos");
        }
        
        public int ArchivoEnDiscoId { get; set; }

        [StringLength(255)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string NombreOriginal { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string URL
        {
            get
            {
                return String.Format("\\{0}", this.Ruta.Replace(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, String.Empty));
            }
        }
        
        public virtual string Ubicacion { get; set; }

        public virtual string Ruta
        {
            get
            {
                return String.Format("{0}\\{1}", this.Ubicacion, this.Nombre);
            }
        }

        public string Tipo { get; set; }

        public virtual string GenerarNuevoNombre(string original)
        {
            
            var prefijo = String.Format("{0} - ", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            var archivoExtension = System.IO.Path.GetExtension(original);
            var archivoNombre = original.Substring(0, original.Length - archivoExtension.Length);
            var largoFijo = prefijo.Length + archivoExtension.Length;

            return String.Format("{0}{1}{2}",
                prefijo,
                (largoFijo + archivoNombre.Length > 255) ? archivoNombre.Substring(0, 255 - largoFijo) : archivoNombre,
                archivoExtension
                );

        }

    }

    public class ArchivoAdjuntoEnPedido : ArchivoEnDisco
    {

        public ArchivoAdjuntoEnPedido()
        {
            this.Ubicacion = System.Web.Hosting.HostingEnvironment.MapPath("~/Archivos/AdjuntosEnPedidos");
        }
        
        [ForeignKey("Pedido")]
        public int? PedidoId { get; set; }

        public virtual Pedido Pedido { get; set; }

        public override string GenerarNuevoNombre(string original)
        {

            var prefijo = String.Format("Pedido {0} - {1} - ", this.PedidoId.Value.ToString("000000000"), DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            var archivoExtension = System.IO.Path.GetExtension(original);
            var archivoNombre = original.Substring(0, original.Length - archivoExtension.Length);
            var largoFijo = prefijo.Length + archivoExtension.Length;

            return String.Format("{0}{1}{2}",
                prefijo,
                (largoFijo + archivoNombre.Length > 255) ? archivoNombre.Substring(0, 255 - largoFijo) : archivoNombre,
                archivoExtension
                );

        }

    }

}