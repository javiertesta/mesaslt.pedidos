using System;
using Pedidos.DAL;
using Pedidos.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Pedidos.DAL
{
    public class UnitOfWork : IDisposable
    {
        private PedidosDbContext context;

        private PedidoRepository pedidoRepository;
        private ClienteRepository clienteRepository;
        private GestionRepository gestionRepository;
        private ArticuloRepository articuloRepository;
        private SeguimientoIndividualRepository seguimientoIndividualRepository;
        private EtapaDelNegocioInternaRepository etapaDelNegocioInternaRepository;
        private MuestrarioRepository muestrarioRepository;
        private LaminadoRepository laminadoRepository;
        private PedidosListadosRepository pedidosListadosRepository;
        private ArchivoEnDiscoRepository archivoEnDiscoRepository;
        
        private bool disposed = false;
        private List<EtapaDelNegocioPublica> _etapasDelNegocioPublicas;
        private List<EtapaDelNegocioInterna> _etapasDelNegocioInternas;

        public UnitOfWork() : this(new PedidosDbContext()) { }

        public UnitOfWork(PedidosDbContext context)
        {
            this.context = context;
            this.ProxyCreationEnabled = false;
            this.LazyLoadingEnabled = false;
            _etapasDelNegocioPublicas = context.EtapasDelNegocioPublicas.ToList();
            _etapasDelNegocioInternas = this.EtapaDelNegocioInternaRepository.Obtener();
        }

        public PedidoRepository PedidoRepository
        {
            get
            {

                if (this.pedidoRepository == null)
                {
                    this.pedidoRepository = new PedidoRepository(context);
                }
                return pedidoRepository;
            }
        }

        public ClienteRepository ClienteRepository
        {
            get
            {

                if (this.clienteRepository == null)
                {
                    this.clienteRepository = new ClienteRepository(context);
                }
                return clienteRepository;
            }
        }

        public GestionRepository GestionRepository
        {
            get
            {

                if (this.gestionRepository == null)
                {
                    this.gestionRepository = new GestionRepository(context);
                }
                return gestionRepository;
            }
        }

        public ArticuloRepository ArticuloRepository
        {
            get
            {

                if (this.articuloRepository == null)
                {
                    this.articuloRepository = new ArticuloRepository(context);
                }
                return articuloRepository;
            }
        }

        public SeguimientoIndividualRepository SeguimientoIndividualRepository
        {
            get
            {

                if (this.seguimientoIndividualRepository == null)
                {
                    this.seguimientoIndividualRepository = new SeguimientoIndividualRepository(context);
                }
                return seguimientoIndividualRepository;
            }
        }

        public EtapaDelNegocioInternaRepository EtapaDelNegocioInternaRepository
        {
            get
            {

                if (this.etapaDelNegocioInternaRepository == null)
                {
                    this.etapaDelNegocioInternaRepository = new EtapaDelNegocioInternaRepository(context);
                }
                return etapaDelNegocioInternaRepository;
            }
        }

        public MuestrarioRepository MuestrarioRepository
        {
            get
            {

                if (this.muestrarioRepository == null)
                {
                    this.muestrarioRepository = new MuestrarioRepository(context);
                }
                return muestrarioRepository;
            }
        }

        public LaminadoRepository LaminadoRepository
        {
            get
            {

                if (this.laminadoRepository == null)
                {
                    this.laminadoRepository = new LaminadoRepository(context);
                }
                return laminadoRepository;
            }
        }

        public PedidosListadosRepository PedidosListadosRepository
        {
            get
            {

                if (this.pedidosListadosRepository == null)
                {
                    this.pedidosListadosRepository = new PedidosListadosRepository(context);
                }
                return pedidosListadosRepository;
            }
        }

        public ArchivoEnDiscoRepository ArchivoEnDiscoRepository
        {
            get
            {

                if (this.archivoEnDiscoRepository == null)
                {
                    this.archivoEnDiscoRepository = new ArchivoEnDiscoRepository(context);
                }
                return archivoEnDiscoRepository;
            }
        }

        public Action<string> Log
        {
            get
            {
                return context.Database.Log;
            }
            set
            {
                context.Database.Log = value;
            }
        }

        public bool LazyLoadingEnabled
        {
            get
            {
                return context.Configuration.LazyLoadingEnabled;
            }
            set
            {
                context.Configuration.LazyLoadingEnabled = value;
            }
        }

        public bool ProxyCreationEnabled
        {
            get
            {
                return context.Configuration.ProxyCreationEnabled;
            }
            set
            {
                context.Configuration.ProxyCreationEnabled = value;
            }
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}