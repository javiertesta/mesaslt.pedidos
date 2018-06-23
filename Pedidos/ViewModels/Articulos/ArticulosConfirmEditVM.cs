using System.Collections.Generic;
using Pedidos.Models;

namespace Pedidos.ViewModels
{
    public class ArticulosConfirmEditVM : ArticulosEditVM
    {
        public ArticulosConfirmEditVM() : base()
        {
        }

        public ArticulosConfirmEditVM(IEnumerable<Articulo> articulos) : base(articulos)
        {
        }

        public ArticulosConfirmEditVM(ArticulosEditVM VM) : this()
        {
            this.items = VM.items;
            this.ReturnURL = VM.ReturnURL;
        }

        public string Codigos { get; set; }
    }
}