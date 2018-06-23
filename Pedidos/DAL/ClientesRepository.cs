using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using Pedidos.Models;
using Pedidos.DAL;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pedidos.DAL
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {

    }

    public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(PedidosDbContext context) : base(context) { }

        private IQueryable<Cliente> PlantillaBase
        {
            get
            {
                return context.Clientes;
            }
        }
    }
}