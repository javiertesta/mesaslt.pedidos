using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pedidos.Models
{
    
    [Table("PedidosListados")]
    public class PedidoListado
    {

        public int Id { get; set; }
        
        public DateTime Fecha { get; set; }

        public string Titulo { get; set; }

        public string Creador { get; set; }

        public string Pedidos { get; set; }

    }

}