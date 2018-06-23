using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Pedidos.ViewModels
{
    
    public class SubirVM
    {

        public SubirVM()
        {
            this.Permitido = true;
        }

        public int PedidoId { get; set; }

        [Display(Name = "Archivo")]
        public HttpPostedFileBase Archivo { get; set; }

        public bool Permitido { get; set; }

        public string ReturnURL { get; set; }

    }

}