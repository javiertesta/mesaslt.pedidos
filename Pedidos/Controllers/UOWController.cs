using Pedidos.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pedidos.Controllers
{
    public class UOWController : Controller
    {

        protected UnitOfWork _UOW;

        public UOWController()
        {
            this._UOW = new UnitOfWork();
        }

        public UOWController(UnitOfWork UOW)
        {
            this._UOW = UOW;
        }

        public UnitOfWork UOW
        {
            get
            {
                return _UOW;
            }
        }

    }
}