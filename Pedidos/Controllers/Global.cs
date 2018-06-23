using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pedidos.Controllers
{
    public static class Control
    {
        [Flags]
        public enum Propagacion
        {
            No = 0,
            HaciaArriba = 1,
            HaciaAbajo = 2
        }

    }

}