using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CustomExtensions;
using Pedidos.Models.Enums;
using System.Xml.Serialization;
using System.Globalization;
using Pedidos.DAL;

namespace Pedidos.Models
{

    [Table("Pedidos")]
    public class Pedido : IXmlSerializable
    {

        public Pedido()
        {
            this.Cantidad = 1;
            this.ArchivosAdjuntos = new HashSet<ArchivoAdjuntoEnPedido>();
            this.Historial = new HashSet<Pedido>();
        }

        #region IXmlSerializable

        public virtual System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            bool _IsEmptyElement;
            string _Value;

            // Mueve el cursor hasta el primer elemento que contiene datos.
            reader.MoveToContent();

            // Lee los atributos del elemento.
            this.PedidoId = Convert.ToInt16(reader.GetAttribute("PedidoId"));

            // Pregunta si el elemento contiene datos o está vacío y lee el elemento de entrada.
            Boolean isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();

            // Si el elemento de entrada no contiene información,
            // entonces no tiene datos que rescatar ni tampoco elemento de cierre.
            if (!isEmptyElement)
            {
                // Si el elemento actual es "Gestion"
                if (reader.Name == "Gestion")
                {
                    // Si ese elemento no se cierra en sí mismo.
                    if (!reader.IsEmptyElement)
                    {
                        Gestion gestion = new Gestion();
                        gestion.ReadXml(reader.ReadSubtree());
                        this.GestionId = gestion.GestionId;
                        this.Gestion = gestion;
                    }
                    // Si el elemento no está vacío: Luego del comando ReadSubtree, el cursor queda sobre el elemento de cierre.
                    // Si el elemento está vacío: El elemento se cierra en sí mismo, el cursor queda sobre el elemento (que también es de cierre).
                    reader.Read();
                }

                // Si el elemento actual es "Cliente"
                if (reader.Name == "Cliente")
                {
                    // Si ese elemento no se cierra en sí mismo.
                    if (!reader.IsEmptyElement)
                    {
                        Cliente cliente = new Cliente();
                        cliente.ReadXml(reader.ReadSubtree());
                        // Si se generó una entidad "Gestion" con éxito y se agregó a este pedido,
                        if (this.Gestion != null)
                        {
                            this.Gestion.ClienteId = cliente.ClienteId;
                            this.Gestion.Cliente = cliente;
                        }
                    }
                    // Si el elemento no está vacío: Luego del comando ReadSubtree, el cursor queda sobre el elemento de cierre.
                    // Si el elemento está vacío: El elemento se cierra en sí mismo, el cursor queda sobre el elemento (que también es de cierre).
                    reader.Read();
                }
                
                if (reader.Name == "Cantidad") this.Cantidad = Convert.ToInt16(reader.ReadElementString("Cantidad"));
                if (reader.Name == "EstructuraSolicitada") this.EstructuraSolicitada = reader.ReadElementString("EstructuraSolicitada");

                if (reader.Name == "FechaBaja")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("FechaBaja");
                    this.FechaBaja = _IsEmptyElement ? null : (DateTime?)DateTime.ParseExact(_Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }

                if (reader.Name == "FechaEntrega")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("FechaEntrega");
                    this.FechaEntrega = _IsEmptyElement ? null : (DateTime?)DateTime.ParseExact(_Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }

                if (reader.Name == "RegistroOriginalId")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("RegistroOriginalId");
                    this.RegistroOriginalId = _IsEmptyElement ? null : (int?)Convert.ToInt16(_Value);
                }

                if (reader.Name == "Referencia") this.Referencia = reader.ReadElementString("Referencia");
                if (reader.Name == "UserName") this.UserName = reader.ReadElementString("UserName");
                if (reader.Name == "RequiereAprobacion") this.RequiereAprobacion = reader.ReadElementString("RequiereAprobacion") == "Sí";
                //if (reader.Name == "Seguimiento") this.Seguimiento = (CircuitoDePedidos)Convert.ToInt16(reader.ReadElementString("Seguimiento"));
                //if (reader.Name == "SeguimientoNombre") reader.ReadElementString("SeguimientoNombre");

                if (reader.Name == "Articulo" || reader.Name == "Tapa" || reader.Name == "Base" || reader.Name == "Vitrea")
                {
                    if (!reader.IsEmptyElement)
                    {
                        Articulo ElementoArticulo;
                        switch (reader.Name)
                        {
                            case "Tapa":
                                ElementoArticulo = new Tapa();
                                break;
                            case "Base":
                                ElementoArticulo = new Base();
                                break;
                            case "Vitrea":
                                ElementoArticulo = new Vitrea();
                                break;
                            default:
                                ElementoArticulo = new Articulo();
                                break;
                        }
                        ElementoArticulo.ReadXml(reader.ReadSubtree());
                        this.Articulo = ElementoArticulo;
                    }
                    // Si el elemento no está vacío: Luego del comando ReadSubtree, el cursor queda sobre el elemento de cierre.
                    // Si el elemento está vacío: El elemento se cierra en sí mismo, el cursor queda sobre el elemento (que también es de cierre).
                    reader.Read();
                }

                // Lee el elemento que finaliza la deserialización.
                reader.ReadEndElement();
            }
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("PedidoId", PedidoId.ToString());
            writer.WriteStartElement("Gestion");
            Gestion.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteStartElement("Cliente");
            Gestion.Cliente.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteElementString("Cantidad", Cantidad.ToString());
            writer.WriteElementString("EstructuraSolicitada", EstructuraSolicitada);
            writer.WriteElementString("FechaBaja", FechaBaja.HasValue ? FechaBaja.Value.ToString("yyyy-MM-dd HH:mm:ss") : String.Empty);
            writer.WriteElementString("FechaEntrega", FechaEntrega.HasValue ? FechaEntrega.Value.ToString("yyyy-MM-dd HH:mm:ss") : String.Empty);
            writer.WriteElementString("RegistroOriginalId", RegistroOriginalId.HasValue ? RegistroOriginalId.ToString() : String.Empty);
            writer.WriteElementString("Referencia", Referencia);
            writer.WriteElementString("UserName", UserName);
            writer.WriteElementString("RequiereAprobacion", RequiereAprobacion ? "Sí" : "No");
            //writer.WriteElementString("Seguimiento", "0"); // MODIFICAR
            //writer.WriteElementString("SeguimientoNombre", "MODIFICAR"); // MODIFICAR
            writer.WriteStartElement(Articulo.TipoDeArticulo);
            Articulo.WriteXml(writer);
            writer.WriteEndElement();
        }

        #endregion

        #region Sección Copy & Paste

        public Pedido Copiar()
        {
            var _output = new Pedido();
            _output.Articulo = this.Articulo.Copiar();
            _output.Cantidad = this.Cantidad;
            _output.EstructuraSolicitada = this.EstructuraSolicitada;
            _output.FechaBaja = this.FechaBaja;
            _output.FechaEntrega = this.FechaEntrega;
            _output.Gestion = this.Gestion;
            _output.GestionId = this.GestionId;
            _output.PedidoId = this.PedidoId;
            _output.Referencia = this.Referencia;
            _output.RegistroOriginal = this.RegistroOriginal;
            _output.RegistroOriginalId = this.RegistroOriginalId;
            _output.RequiereAprobacion = this.RequiereAprobacion;
            _output.SeguimientoGlobal = this.SeguimientoGlobal.Copiar();
            _output.UserName = this.UserName;
            return _output;
        }

        public void Pegar(Pedido desde)
        {
            this.Articulo.Pegar(desde.Articulo);
            this.Cantidad = desde.Cantidad;
            this.EstructuraSolicitada = desde.EstructuraSolicitada;
            this.FechaBaja = desde.FechaBaja;
            this.FechaEntrega = desde.FechaEntrega;
            this.Gestion = desde.Gestion;
            this.GestionId = desde.GestionId;
            this.PedidoId = desde.PedidoId;
            this.Referencia = desde.Referencia;
            this.RegistroOriginal = desde.RegistroOriginal;
            this.RegistroOriginalId = desde.RegistroOriginalId;
            this.RequiereAprobacion = desde.RequiereAprobacion;
            this.SeguimientoGlobal = desde.SeguimientoGlobal.Copiar();
            this.UserName = desde.UserName;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="pedido">El pedido que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Pedido pedido, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver falso cuando el parámetro pasado es null.
            if (pedido == null) return false;

            // Si ambos pedidos son en realidad el mismo pedido, debe devolver verdadero.
            if (ReferenceEquals(pedido, this)) return true;

            return
                (omiteEntidades ? true : ReferenceEquals(pedido.Articulo, this.Articulo)) &&
                (omiteEntidades ? true : pedido.GestionId == this.GestionId) &&
                (omiteEntidades ? true : pedido.RegistroOriginalId == this.RegistroOriginalId) &&
                (omiteEntidades ? true : ReferenceEquals(pedido.SeguimientoGlobal, this.SeguimientoGlobal)) &&
                (omiteClave ? true : pedido.PedidoId == this.PedidoId) &&
                (pedido.Cantidad == this.Cantidad) &&
                (pedido.EstructuraSolicitada == this.EstructuraSolicitada) &&
                (pedido.FechaBaja == this.FechaBaja) &&
                (pedido.FechaEntrega == this.FechaEntrega) &&
                (pedido.Referencia == this.Referencia) &&
                (pedido.RequiereAprobacion == this.RequiereAprobacion) &&
                (pedido.UserName == this.UserName);
        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="pedido">El pedido que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver falso cuando el parámetro pasado es null.
            if ((objeto as Pedido) == null) return false;

            // Si ambos pedidos son en realidad el mismo pedido, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Pedido, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (this.Cantidad + 65225) ^
                    (!String.IsNullOrEmpty(this.EstructuraSolicitada) ? this.EstructuraSolicitada.GetHashCode() : 3156) ^
                    (this.FechaBaja.HasValue ? this.FechaBaja.GetHashCode() : 32168) ^
                    (this.FechaEntrega.HasValue ? this.FechaEntrega.GetHashCode() : 23489) ^
                    (this.GestionId + 2165) ^
                    (this.PedidoId + 22222) ^
                    (!String.IsNullOrEmpty(this.Referencia) ? this.Referencia.GetHashCode() : 58588) ^
                    (this.RegistroOriginalId.HasValue ? this.RegistroOriginalId.Value : 328) ^
                    (this.RequiereAprobacion ? 7 : 13) ^
                    (!String.IsNullOrEmpty(this.UserName) ? this.UserName.GetHashCode() : 1212121);
            }
        }

        #endregion

        /// <summary>
        /// Corrobora que la estructura básica del artículo concuerde con la estructura especificada en el Código Tango solicitado al crear el pedido,
        /// código que se almacena en el campo EstructuraSolicitada. La estructura básica es el armado fundamental que da razón de ser a dicho código Tango.
        /// Por ejemplo, un cambio en el color de PVC de un artículo no rompería con dicha estructura porque carece de relevancia en cuanto a su Código Tango.
        /// </summary>
        /// <returns>Verdadero si la estructura actual respeta a la contenida originalmente en el Código Tango que se solicitó al crear el pedido.</returns>
        public virtual bool ComprobarEstructura()
        {
            return this.Articulo.ComprobarEstructura(this.EstructuraSolicitada);
        }

        public override string ToString()
        {
            return "Pedido [" + this.PedidoId.ToString() + "] => Cantidad[" + this.Cantidad.ToString() + "] => " + this.Articulo.ToString();
        }

        [Key, ForeignKey("Articulo")]
        [Display(Name = "Código de Pedido", ShortName = "Código")]
        public int PedidoId { get; set; }

        public int Cantidad { get; set; }

        [Display(Name = "Fecha de Entrega", ShortName = "Entrega")]
        [CustomDate]
        public DateTime? FechaEntrega { get; set; }

        [Display(Name = "Referencia del Cliente", ShortName = "Referencia")]
        public string Referencia { get; set; }

        [Display(Name = "Requiere Aprobación", ShortName = "A Aprobar")]
        public bool RequiereAprobacion { get; set; }

        [Display(Name = "Seguimiento")]
        public virtual SeguimientoGlobal SeguimientoGlobal { get; set; }

        [Display(Name = "Código Tango")]
        public string EstructuraSolicitada { get; set; }

        [Display(Name = "Último Usuario que Intervino", ShortName = "Usuario")]
        public string UserName { get; set; }

        [Display(Name = "Fecha de Baja", ShortName = "Baja")]
        public DateTime? FechaBaja { get; set; }

        public int? RegistroOriginalId { get; set; }

        public Pedido RegistroOriginal { get; set; }

        [Display(Name = "Observaciones del Pedido", ShortName = "Observaciones")]
        public string Observaciones { get; set; }

        public virtual HashSet<Pedido> Historial { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Articulo Articulo { get; set; }

        public int GestionId { get; set; }

        public virtual Gestion Gestion { get; set; }

        public bool ContieneAdjuntos()
        {
            return (this.ArchivosAdjuntos.Count > 0);
        }

        public virtual HashSet<ArchivoAdjuntoEnPedido> ArchivosAdjuntos { get; set; }

    }

    [Table("Articulos")]
    public class Articulo : IXmlSerializable
    {

        #region Sección IXmlSerializable

        protected const bool _HasEndElement = true;

        public virtual System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            // Si el elemento de partida no cerró en sí mismo,
            if (ReadXml_Start(reader) == _HasEndElement)
            {
                // Lee el elemento que finaliza la deserialización.
                reader.ReadEndElement();
            }
        }

        public virtual bool ReadXml_Start(System.Xml.XmlReader reader)
        {
            bool _HasEndElement;

            // Mueve el cursor hasta el primer elemento que contiene datos.
            reader.MoveToContent();

            // Lee los atributos del elemento.
            this.ArticuloId = Convert.ToInt16(reader.GetAttribute("ArticuloId"));

            // Pregunta si el elemento contiene datos o está vacío y lee el elemento de entrada.
            Boolean isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();

            // El valor devuelto indicará si la etiqueta en cuestión debe cerrarse posteriormente.
            _HasEndElement = !isEmptyElement;

            // Si el elemento de entrada contiene etiqueta de cierre,
            if (_HasEndElement)
            {
                // Lee contenido.
                if (reader.Name == "Particularidades") this.Particularidades = reader.ReadElementString("Particularidades");
                if (reader.Name == "CodigoTango") this.CodigoTango = reader.ReadElementString("CodigoTango");
            }
            
            return _HasEndElement;
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("ArticuloId", ArticuloId.ToString());
            writer.WriteElementString("Particularidades", Particularidades);
            writer.WriteElementString("CodigoTango", CodigoTango);
        }

        #endregion

        #region Sección Copy & Paste

        /// <summary>
        /// Copia el contenido de este artículo.
        /// </summary>
        /// <returns>Una copia del artículo actual.</returns>
        public virtual Articulo Copiar()
        {
            Articulo _output = CrearInstancia();
            _output.ArticuloId = this.ArticuloId;
            _output.Particularidades = this.Particularidades;
            _output.CodigoTango = this.CodigoTango;
            return _output;
        }

        /// <summary>
        /// Pega datos sobre este artículo.
        /// </summary>
        /// <param name="desde">Artículo con los datos que se desean pegar aquí.</param>
        public virtual void Pegar(Articulo desde)
        {
            if (desde != null)
            {
                this.ArticuloId = desde.ArticuloId;
                this.CodigoTango = desde.CodigoTango;
                this.Particularidades = desde.Particularidades;
            }
        }

        #endregion

        #region Sección Equals

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="articulo">El artículo que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public virtual bool EntityEquals(Articulo articulo, bool omiteEntidades = true, bool omiteClave = false)
        {
            
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if (articulo == null) return false;

            // Si ambos artículos son en realidad el mismo artículo, debe devolver verdadero.
            if (ReferenceEquals(articulo, this)) return true;
            
            return
                (omiteEntidades ? true : ReferenceEquals(articulo.Pedido, this.Pedido)) &&
                (omiteClave ? true : articulo.ArticuloId == this.ArticuloId) &&
                articulo.CodigoTango.Equals(this.CodigoTango) &&
                (articulo.Particularidades == this.Particularidades);
            
        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="articulo">El artículo que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public virtual bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {

            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if ((objeto as Articulo) == null) return false;

            // Si ambos artículos son en realidad el mismo artículo, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Articulo, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (!String.IsNullOrEmpty(this.CodigoTango) ? this.CodigoTango.GetHashCode() : 354) ^
                    (!String.IsNullOrEmpty(this.Particularidades) ? this.Particularidades.GetHashCode() : 223335) ^
                    (this.ArticuloId + 685415);
            }
        }

        #endregion

        /// <summary>
        /// Crea una nueva instancia de la clase Articulo.
        /// Este método sirve a los fines de que pueda obtenerse una nueva instancia de un artículo específico aunque se llame desde una variable más amplia.
        /// </summary>
        /// <returns>Una nueva instancia de la clase Articulo.</returns>
        public virtual Articulo CrearInstancia()
        {
            return new Articulo();
        }

        /// <summary>
        /// Corrobora que la estructura básica de este artículo coincida con la estructura básica del artículo cuyo Código Tango se pasa como argumento.
        /// La estructura básica es el armado fundamental que da razón de ser a dicho código Tango.
        /// Por ejemplo, un cambio en el color de PVC de un artículo no rompería con dicha estructura porque carece de relevancia en cuanto a su Código Tango.
        /// </summary>
        /// <param name="codigoTango">El código Tango cuya estructura se validará contra esta.</param>
        /// <returns>Verdadero si la estructura actual respeta a la contenida en el artículo cuyo codigo Tango se pasa como argumento.</returns>
        public virtual bool ComprobarEstructura(string codigoTango)
        {
            
            if (String.IsNullOrEmpty(codigoTango)) return true;
            
            using (UnitOfWork UOW = new UnitOfWork())
            {
                Articulo articuloOriginal = UOW.ArticuloRepository.ObtenerPorCodigoTango(codigoTango);
                return
                    (articuloOriginal != null) &&
                    (this.TipoDeArticulo == articuloOriginal.TipoDeArticulo) &&
                    String.Equals(this.Particularidades, articuloOriginal.Particularidades);
            }

        }

        public override string ToString()
        {
            return "Código Tango: " + this.CodigoTango;
        }

        [Display(Name = "Código de Artículo", ShortName = "Código")]
        public int ArticuloId { get; set; }

        [Display(Name = "Código Tango", ShortName = "Tango")]
        public string CodigoTango { get; set; }

        [Display(Name = "Particularidades")]
        public string Particularidades { get; set; }

        public virtual string TipoDeArticulo
        {
            get
            {
                return "Articulo";
            }
        }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Pedido Pedido { get; set; }

    }

    public class Base : Articulo
    {

        #region Sección IXmlSerializable

        public override System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // Si el elemento de partida no cerró en sí mismo,
            if (ReadXml_Start(reader) == _HasEndElement)
            {
                // Lee el elemento que finaliza la deserialización.
                reader.ReadEndElement();
            }
        }

        public override bool ReadXml_Start(System.Xml.XmlReader reader)
        {
            // Inicializaciones.
            bool _IsEmptyElement;
            string _Value;

            // Deriva el comienzo de la lectura del elemento a la clase base.
            // Si el elemento de partida no cerró en sí mismo,
            if (base.ReadXml_Start(reader) == _HasEndElement)
            {
                if (reader.Name == "Modelo") this.Modelo = reader.ReadElementString("Modelo");
                if (reader.Name == "Espesor") this.Espesor = (EspesoresDeBases)Convert.ToInt16(reader.ReadElementString("Espesor"));
                if (reader.Name == "EspesorNombre") reader.ReadElementString("EspesorNombre");
                if (reader.Name == "Color")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("Color");
                    this.Color = _IsEmptyElement ? null : (ColoresDeBases?)Convert.ToInt16(_Value);
                }
                if (reader.Name == "ColorNombre") reader.ReadElementString("ColorNombre");
                if (reader.Name == "Proveedor") this.Proveedor = (Proveedores)Convert.ToInt16(reader.ReadElementString("Proveedor"));
                if (reader.Name == "ProveedorNombre") reader.ReadElementString("ProveedorNombre");
                return _HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
            }
            else
	        {
                return !_HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
	        }
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("Modelo", Modelo);
            writer.WriteElementString("Espesor", ((int)Espesor).ToString());
            writer.WriteElementString("EspesorNombre", Espesor.GetEnumMemberDisplayName<EspesoresDeBases>().ToString());
            writer.WriteElementString("Color", Color.HasValue ? ((int?)Color).ToString() : String.Empty);
            writer.WriteElementString("ColorNombre", Color.HasValue ? Color.GetEnumMemberDisplayName<ColoresDeBases>().ToString() : String.Empty);
            writer.WriteElementString("Proveedor", ((int)Proveedor).ToString());
            writer.WriteElementString("ProveedorNombre", Proveedor.GetEnumMemberDisplayName<Proveedores>().ToString());
        }

        #endregion

        #region Sección Copy & Paste

        /// <summary>
        /// Copia el contenido de esta vitrea.
        /// </summary>
        /// <returns>Una copia de la vitrea actual.</returns>
        public override Articulo Copiar()
        {
            Articulo _output = base.Copiar();
            (_output as Base).Color = this.Color;
            (_output as Base).Espesor = this.Espesor;
            (_output as Base).Modelo = this.Modelo;
            (_output as Base).Proveedor = this.Proveedor;
            return _output;
        }

        /// <summary>
        /// Pega datos sobre esta vitrea.
        /// </summary>
        /// <param name="desde">Vitrea con los datos que se desean pegar aquí.</param>
        public override void Pegar(Articulo desde)
        {
            if (desde != null)
            {
                base.Pegar(desde);
                if (desde is Base)
                {
                    this.Color = (desde as Base).Color;
                    this.Espesor = (desde as Base).Espesor;
                    this.Modelo = (desde as Base).Modelo;
                    this.Proveedor = (desde as Base).Proveedor;
                }
            }
        }

        #endregion

        #region Sección Equals

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="b">La base que se comparará con esta.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Base b, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if (b == null) return false;

            // Si ambas bases son en realidad la misma base, debe devolver verdadero.
            if (ReferenceEquals(b, this)) return true;

            return base.EntityEquals(b, omiteEntidades, omiteClave) &&
                (b.Color == this.Color) &&
                (b.Espesor == this.Espesor) &&
                (b.Modelo == this.Modelo) &&
                (b.Proveedor == this.Proveedor);
        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="b">La base que se comparará con esta.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public override bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if ((objeto as Base) == null) return false;

            // Si ambas bases son en realidad la misma base, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Base, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (int)this.Color ^
                    (int)this.Espesor ^
                    (int)this.Proveedor;
            }
        }

        #endregion

        /// <summary>
        /// Crea una nueva instancia de la clase Base.
        /// Este método sirve a los fines de que pueda obtenerse una nueva instancia de una base aunque se llame desde una variable más amplia.
        /// </summary>
        /// <returns>Una nueva instancia de la clase Base.</returns>
        public override Articulo CrearInstancia()
        {
            return new Base();
        }

        /// <summary>
        /// Corrobora que la estructura básica de esta base coincida con la estructura básica de la base cuyo Código Tango se pasa como argumento.
        /// La estructura básica es el armado fundamental que da razón de ser a dicho código Tango.
        /// Por ejemplo, un cambio en el color de la base no rompería con dicha estructura porque carece de relevancia en cuanto a su Código Tango.
        /// </summary>
        /// <param name="codigoTango">El código Tango cuya estructura se validará contra esta.</param>
        /// <returns>Verdadero si la estructura actual respeta a la contenida en la base cuyo codigo Tango se pasa como argumento.</returns>
        public override bool ComprobarEstructura(string codigoTango)
        {

            if (String.IsNullOrEmpty(codigoTango)) return true;

            using (UnitOfWork UOW = new UnitOfWork())
            {
                Base articuloOriginal = UOW.ArticuloRepository.ObtenerPorCodigoTango(codigoTango) as Base;
                return
                    (articuloOriginal != null) &&
                    base.ComprobarEstructura(codigoTango) &&
                    String.Equals(this.Modelo, articuloOriginal.Modelo) &&
                    (this.Espesor == articuloOriginal.Espesor) &&
                    (this.Proveedor == articuloOriginal.Proveedor);
            }

        }
        
        public override string ToString()
        {
            return String.Format("{0} => Modelo[{1}] Espesor[{2}] Color[{3}] Proveedor[{4}]", this.TipoDeArticulo, this.Modelo, this.EspesorNombre, this.ColorNombre, this.ProveedorNombre);
        }

        public string ToString(string modo)
        {
            
            switch (modo)
            {

                case "Optimizado":
                    var partes = new List<string>();
                    if (String.IsNullOrWhiteSpace(Modelo)) partes.Add("Base"); else partes.Add(String.Format("Base {0}", Modelo));
                    if (Espesor != EspesoresDeBases.NC && Espesor != EspesoresDeBases.NE) partes.Add(EspesorNombre);
                    if (Color.HasValue && Color != ColoresDeBases.NC && Color != ColoresDeBases.NE) partes.Add(String.Format("({0})", ColorNombre));
                    return String.Join(" ", partes.ToArray());

                default:
                    return this.ToString();

            }

        }

        public override string TipoDeArticulo
        {
            get
            {
                return "Base";
            }
        }

        [Display(Name = "Modelo de Base", ShortName = "Modelo")]
        public string Modelo { get; set; }

        [Display(Name = "Espesor del Caño", ShortName = "Espesor")]
        public Pedidos.Models.Enums.EspesoresDeBases Espesor { get; set; }

        [Display(Name = "Espesor del Caño", ShortName = "Espesor")]
        public string EspesorNombre
        {
            get
            {
                return Espesor.GetEnumMemberDisplayName<Pedidos.Models.Enums.EspesoresDeBases>();
            }
        }

        [Display(Name = "Color de la Base", ShortName = "Color")]
        public Nullable<Pedidos.Models.Enums.ColoresDeBases> Color { get; set; }

        [Display(Name = "Color de la Base", ShortName = "Color")]
        public string ColorNombre
        {
            get
            {
                return Color.GetEnumMemberDisplayName<Pedidos.Models.Enums.ColoresDeBases>();
            }
        }

        public Pedidos.Models.Enums.Proveedores Proveedor { get; set; }

        [Display(Name = "Proveedor")]
        public string ProveedorNombre
        {
            get
            {
                return Proveedor.GetEnumMemberDisplayName<Pedidos.Models.Enums.Proveedores>();
            }
        }

    }

    public class Tapa : Articulo
    {

        public Tapa()
        {
            this.Borde = new Borde();
        }

        #region Sección IXmlSerializable

        public override System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // Si el elemento de partida no cerró en sí mismo,
            if (ReadXml_Start(reader) == _HasEndElement)
            {
                // Lee el elemento que finaliza la deserialización.
                reader.ReadEndElement();
            }
        }

        public override bool ReadXml_Start(System.Xml.XmlReader reader)
        {
            // Inicializaciones.
            bool _IsEmptyElement;
            string _Value;
            Laminado ElementoLaminado = new Laminado();

            // Deriva el comienzo de la lectura del elemento a la clase base.
            // Si el elemento de partida no cerró en sí mismo,
            if (base.ReadXml_Start(reader) == _HasEndElement)
            {
                if (reader.Name == "Tipo") this.Tipo = (TiposDeTapas)Convert.ToInt16(reader.ReadElementString("Tipo"));
                if (reader.Name == "TipoNombre") reader.ReadElementString("TipoNombre");
                if (reader.Name == "Medida") this.Medida = reader.ReadElementString("Medida");
                if (reader.Name == "Melamina")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("Melamina");
                    this.Melamina = _IsEmptyElement ? null : ((_Value == "Sí") ? (bool?)true : false);
                }
                if (reader.Name == "BordeTipo") this.Borde.Tipo = (TiposDeBordesDeTapas)Convert.ToInt16(reader.ReadElementString("BordeTipo"));
                if (reader.Name == "BordeTipoNombre") reader.ReadElementString("BordeTipoNombre");
                if (reader.Name == "BordeEspesor") this.Borde.Espesor = (EspesoresDeBordesDeTapas)Convert.ToInt16(reader.ReadElementString("BordeEspesor"));
                if (reader.Name == "BordeEspesorNombre") reader.ReadElementString("BordeEspesorNombre");
                if (reader.Name == "BordeColor")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("BordeColor");
                    this.Borde.Color = _IsEmptyElement ? null : (ColoresDeBordesDeTapas?)Convert.ToInt16(_Value);
                }
                if (reader.Name == "BordeColorNombre") reader.ReadElementString("BordeColorNombre");
                if (reader.Name == "Laminado")
                {
                    if (!reader.IsEmptyElement)
                    {
                        ElementoLaminado.ReadXml(reader.ReadSubtree());
                        this.Laminado_CodigoId = this.Laminado.Laminado_CodigoId;
                        this.Laminado_MuestrarioId = this.Laminado.Laminado_MuestrarioId;
                        this.Laminado = ElementoLaminado;
                    }
                    // Si el elemento no está vacío: Luego del comando ReadSubtree, el cursor queda sobre el elemento de cierre.
                    // Si el elemento está vacío: El elemento se cierra en sí mismo, el cursor queda sobre el elemento (que también es de cierre).
                    reader.Read();
                }
                return _HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
            }
            else
            {
                return !_HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
            }
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("Tipo", ((int)Tipo).ToString());
            writer.WriteElementString("TipoNombre", Tipo.GetEnumMemberDisplayName<TiposDeTapas>().ToString());
            writer.WriteElementString("Medida", Medida);
            writer.WriteElementString("Melamina", Melamina.HasValue ? (Melamina.Value ? "Sí" : "No") : String.Empty);
            writer.WriteElementString("BordeTipo", ((int)Borde.Tipo).ToString());
            writer.WriteElementString("BordeTipoNombre", Borde.Tipo.GetEnumMemberDisplayName<TiposDeBordesDeTapas>().ToString());
            writer.WriteElementString("BordeEspesor", ((int)Borde.Espesor).ToString());
            writer.WriteElementString("BordeEspesorNombre", Borde.Espesor.GetEnumMemberDisplayName<EspesoresDeBordesDeTapas>().ToString());
            writer.WriteElementString("BordeColor", Borde.Color.HasValue ? ((int)Borde.Color).ToString() : String.Empty);
            writer.WriteElementString("BordeColorNombre", Borde.Color.HasValue ? Borde.Color.GetEnumMemberDisplayName<ColoresDeBordesDeTapas>().ToString() : String.Empty);
            writer.WriteStartElement("Laminado");
            if (Laminado_CodigoId != null) this.Laminado.WriteXml(writer);
            writer.WriteEndElement();
        }

        #endregion

        #region Sección Copy & Paste

        /// <summary>
        /// Copia el contenido de esta vitrea.
        /// </summary>
        /// <returns>Una copia de la vitrea actual.</returns>
        public override Articulo Copiar()
        {
            Articulo _output = base.Copiar();
            (_output as Tapa).Borde.Color = this.Borde.Color;
            (_output as Tapa).Borde.Espesor = this.Borde.Espesor;
            (_output as Tapa).Borde.Tipo = this.Borde.Tipo;
            (_output as Tapa).Laminado_CodigoId = this.Laminado_CodigoId;
            (_output as Tapa).Laminado_MuestrarioId = this.Laminado_MuestrarioId;
            (_output as Tapa).Medida = this.Medida;
            (_output as Tapa).Melamina = this.Melamina;
            (_output as Tapa).Tipo = this.Tipo;
            return _output;
        }

        /// <summary>
        /// Pega datos sobre esta vitrea.
        /// </summary>
        /// <param name="desde">Vitrea con los datos que se desean pegar aquí.</param>
        public override void Pegar(Articulo desde)
        {
            if (desde != null)
            {
                base.Pegar(desde);
                if (desde is Tapa)
                {
                    this.Borde.Color = (desde as Tapa).Borde.Color;
                    this.Borde.Espesor = (desde as Tapa).Borde.Espesor;
                    this.Borde.Tipo = (desde as Tapa).Borde.Tipo;
                    this.Laminado_CodigoId = (desde as Tapa).Laminado_CodigoId;
                    this.Laminado_MuestrarioId = (desde as Tapa).Laminado_MuestrarioId;
                    this.Medida = (desde as Tapa).Medida;
                    this.Melamina = (desde as Tapa).Melamina;
                    this.Tipo = (desde as Tapa).Tipo;
                }
            }
        }

        #endregion

        #region Sección Equals

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="tapa">La tapa que se comparará con esta.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Tapa tapa, bool omiteEntidades = true, bool omiteClave = false)
        {

            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if (tapa == null) return false;

            // Si ambas tapas son en realidad la misma tapa, debe devolver verdadero.
            if (ReferenceEquals(tapa, this)) return true;

            return base.EntityEquals(tapa, omiteEntidades, omiteClave) &&
                (omiteClave ? true : String.Equals(tapa.Laminado_CodigoId, this.Laminado_CodigoId)) &&
                (omiteClave ? true : tapa.Laminado_MuestrarioId == this.Laminado_MuestrarioId) &&
                tapa.Borde.EntityEquals(this.Borde, omiteEntidades, omiteClave) &&
                (tapa.Tipo == this.Tipo) &&
                String.Equals(tapa.Medida, this.Medida) &&
                (tapa.Melamina == this.Melamina);

        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="tapa">La tapa que se comparará con esta.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public override bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if ((objeto as Tapa) == null) return false;

            // Si ambas tapas son en realidad la misma tapa, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Tapa, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int cualquiera = 7;
                return base.GetHashCode() ^
                    this.Borde.GetHashCode() ^
                    (!String.IsNullOrEmpty(this.Laminado_CodigoId) ? this.Laminado_CodigoId.GetHashCode() : 13*654) ^
                    (((int?)this.Laminado_MuestrarioId ?? (int?)cualquiera).Value + 7*25) ^
                    (!String.IsNullOrEmpty(this.Medida) ? this.Medida.GetHashCode() : 3*85) ^
                    ((this.Melamina ?? (bool?)true).Value ? 3162 : 651) ^
                    (((int?)this.Tipo ?? (int?)1354).Value + 21*8);
            }
        }

        #endregion

        /// <summary>
        /// Crea una nueva instancia de la clase Tapa.
        /// Este método sirve a los fines de que pueda obtenerse una nueva instancia de una tapa aunque se llame desde una variable más amplia.
        /// </summary>
        /// <returns>Una nueva instancia de la clase Tapa.</returns>
        public override Articulo CrearInstancia()
        {
            return new Tapa();
        }

        /// <summary>
        /// Corrobora que la estructura básica de esta tapa coincida con la estructura básica de la tapa cuyo Código Tango se pasa como argumento.
        /// La estructura básica es el armado fundamental que da razón de ser a dicho código Tango.
        /// Por ejemplo, un cambio en el color de PVC de la tapa no rompería con dicha estructura porque carece de relevancia en cuanto a su Código Tango.
        /// </summary>
        /// <param name="codigoTango">El código Tango cuya estructura se validará contra esta.</param>
        /// <returns>Verdadero si la estructura actual respeta a la contenida en la tapa cuyo codigo Tango se pasa como argumento.</returns>
        public override bool ComprobarEstructura(string codigoTango)
        {

            if (String.IsNullOrEmpty(codigoTango)) return true;

            using (UnitOfWork UOW = new UnitOfWork())
            {
                Tapa articuloOriginal = UOW.ArticuloRepository.ObtenerPorCodigoTango(codigoTango) as Tapa;
                return
                    (articuloOriginal != null) &&
                    base.ComprobarEstructura(codigoTango) &&
                    (this.Tipo == articuloOriginal.Tipo) &&
                    String.Equals(this.Medida, articuloOriginal.Medida) &&
                    (this.Melamina == articuloOriginal.Melamina) &&
                    (this.Borde.Tipo == articuloOriginal.Borde.Tipo) &&
                    (this.Borde.Espesor == articuloOriginal.Borde.Espesor);
            }

        }

        public override string TipoDeArticulo
        {
            get
            {
                return "Tapa";
            }
        }

        public override string ToString()
        {
            return String.Format("{0} => Tipo[{1}] Medida[{2}] BordeTipo[{3}] BordeEspesor[{4}] BordeColor[{5}] Melamina[{6}] Muestrario[{7}] Color[{8}]", this.TipoDeArticulo, this.TipoNombre, this.Medida, this.Borde.TipoNombre, this.Borde.EspesorNombre, this.Borde.ColorNombre, this.Melamina.HasValue ? (this.Melamina.Value ? "Sí" : "No") : "N/E", this.Laminado_MuestrarioId, this.Laminado_CodigoId);
        }

        public string ToString(string modo)
        {

            List<string> partes = new List<string>();
            switch (modo)
            {

                case "Controllers.Informes.PedidosEnteros":
                    if (Tipo.HasValue) partes.Add(Tipo.Value.GetEnumMemberDisplayName<Models.Enums.TiposDeTapas>()); else partes.Add("Tapa");
                    if (!String.IsNullOrWhiteSpace(Medida)) partes.Add(Medida);
                    if (Melamina.HasValue && Melamina.Value) partes.Add("(MELAMINA)");
                    break;

                case "Controllers.InformesController.CorteDeLaminado":
                    partes.Add(this.TipoNombre);
                    if (!String.IsNullOrWhiteSpace(this.Medida)) partes.Add(this.Medida);
                    if (this.Borde.Tipo != TiposDeBordesDeTapas.NC && this.Borde.Tipo != TiposDeBordesDeTapas.NE) partes.Add(this.Borde.TipoNombre);
                    if (this.Borde.Espesor != EspesoresDeBordesDeTapas.NC && this.Borde.Espesor != EspesoresDeBordesDeTapas.NE) partes.Add(String.Format("Borde en {0}", this.Borde.EspesorNombre));
                    if (this.Borde.Color.HasValue && this.Borde.Color.Value != ColoresDeBordesDeTapas.NC && this.Borde.Color.Value != ColoresDeBordesDeTapas.NE) partes.Add(this.Borde.ColorNombre);
                    break;

                case "Controllers.InformesController.MDF":
                    partes.Add(this.TipoNombre);
                    if (!String.IsNullOrWhiteSpace(this.Medida)) partes.Add(this.Medida);
                    break;

                default:
                    return this.ToString();

            }
            return String.Join(" ", partes.ToArray());

        }

        [Display(Name = "Tipo de Tapa", ShortName = "Tipo")]
        public Pedidos.Models.Enums.TiposDeTapas? Tipo { get; set; }

        [Display(Name = "Tipo de Tapa", ShortName = "Tipo")]
        public string TipoNombre
        {
            get
            {
                return Tipo.GetEnumMemberDisplayName<Pedidos.Models.Enums.TiposDeTapas>();
            }
        }

        [Display(Name = "Medida de la Tapa", ShortName = "Medida")]
        public string Medida { get; set; }

        public bool? Melamina { get; set; }

        [Display(Name = "Código de Muestrario", ShortName = "Muestrario")]
        [ForeignKey("Laminado"), Column(Order = 0)]
        public int? Laminado_MuestrarioId { get; set; }

        [Display(Name = "Código de Laminado", ShortName = "Laminado")]
        [ForeignKey("Laminado"), Column(Order = 1)]
        public string Laminado_CodigoId { get; set; }

        public Borde Borde { get; set; }

        public virtual Laminado Laminado { get; set; }

    }

    public class FueraDeLista : Articulo
    {
        
        #region Sección IXmlSerializable

        public override System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // Si el elemento de partida no cerró en sí mismo,
            if (ReadXml_Start(reader) == _HasEndElement)
            {
                // Lee el elemento que finaliza la deserialización.
                reader.ReadEndElement();
            }
        }

        public override bool ReadXml_Start(System.Xml.XmlReader reader)
        {
            // Deriva el comienzo de la lectura del elemento a la clase base.
            // Si el elemento de partida no cerró en sí mismo,
            if (base.ReadXml_Start(reader) == _HasEndElement)
            {
                if (reader.Name == "Titulo") this.Titulo = reader.ReadElementString("Titulo");
                if (reader.Name == "Detalle") this.Detalle = reader.ReadElementString("Detalle");
                return _HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
            }
            else
            {
                return !_HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
            }
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("Titulo", Titulo);
            writer.WriteElementString("Detalle", Detalle);
        }

        #endregion

        #region Sección Copy & Paste

        /// <summary>
        /// Copia el contenido de este artículo fuera de lista.
        /// </summary>
        /// <returns>Una copia del artículo fuera de lista actual.</returns>
        public override Articulo Copiar()
        {
            Articulo _output = base.Copiar();
            (_output as FueraDeLista).Titulo = this.Titulo;
            (_output as FueraDeLista).Detalle = this.Detalle;
            return _output;
        }

        /// <summary>
        /// Pega datos sobre este artículo fuera de lista.
        /// </summary>
        /// <param name="desde">Artículo fuera de lista con los datos que se desean pegar aquí.</param>
        public override void Pegar(Articulo desde)
        {
            if (desde != null)
            {
                base.Pegar(desde);
                if (desde is FueraDeLista)
                {
                    this.Titulo = (desde as FueraDeLista).Titulo;
                    this.Detalle = (desde as FueraDeLista).Detalle;
                }
            }
        }

        #endregion

        #region Sección Equals

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="fueraDeLista">El artículo fuera de lista que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(FueraDeLista fueraDeLista, bool omiteEntidades = true, bool omiteClave = false)
        {

            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if (fueraDeLista == null) return false;

            // Si ambas vitreas son en realidad el mismo artículo fuera de lista, debe devolver verdadero.
            if (ReferenceEquals(fueraDeLista, this)) return true;

            return base.EntityEquals(fueraDeLista, omiteEntidades, omiteClave) &&
                String.Equals(fueraDeLista.Titulo, this.Titulo) &&
                String.Equals(fueraDeLista.Detalle, this.Detalle);

        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="fueraDeLista">El artículo fuera de lista que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public override bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if ((objeto as FueraDeLista) == null) return false;

            // Si ambos artículos fuera de lista son en realidad el mismo artículo fuera de lista, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as FueraDeLista, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (!String.IsNullOrEmpty(this.Titulo) ? this.Titulo.GetHashCode() : 13 * 231) ^
                    (!String.IsNullOrEmpty(this.Detalle) ? this.Detalle.GetHashCode() : 7 * 659);
            }
        }

        #endregion

        /// <summary>
        /// Crea una nueva instancia de la clase FueraDeLista.
        /// Este método sirve a los fines de que pueda obtenerse una nueva instancia de un artículo fuera de lista aunque se llame desde una variable más amplia.
        /// </summary>
        /// <returns>Una nueva instancia de la clase FueraDeLista.</returns>
        public override Articulo CrearInstancia()
        {
            return new FueraDeLista();
        }

        /// <summary>
        /// Corrobora que la estructura básica de un artículo coincida con la estructura básica del artículo cuyo Código Tango se pasa como argumento.
        /// La estructura básica es el armado fundamental que da razón de ser a dicho código Tango.
        /// Por ejemplo, en una vitrea, un cambio en la opacidad (transparente/fumé) no rompería con dicha estructura porque carece de relevancia en cuanto a su Código Tango.
        /// En el caso de este tipo de artículo en particular, si el código es válido y corresponde al tipo de artículo, el método devuelve siempre verdadero,
        /// dado que el tipo de artículo habrá de nacer como "fuera de lista" y morirá del mismo modo.
        /// </summary>
        /// <param name="codigoTango">El código Tango cuya estructura se validará contra esta.</param>
        /// <returns>Para este tipo de artículo, el método devuelve siempre Verdadero.</returns>
        public override bool ComprobarEstructura(string codigoTango)
        {

            if (String.IsNullOrEmpty(codigoTango)) return true;

            using (UnitOfWork UOW = new UnitOfWork())
            {
                FueraDeLista articuloOriginal = UOW.ArticuloRepository.ObtenerPorCodigoTango(codigoTango) as FueraDeLista;
                return
                    (articuloOriginal != null);
            }

        }

        public override string TipoDeArticulo
        {
            get
            {
                return "FueraDeLista";
            }
        }
        
        public override string ToString()
        {
            return this.Titulo;
        }

        public string ToString(string modo)
        {

            switch (modo)
            {

                case "Controllers.Informes.PedidosEnteros":
                    return this.Titulo;

                case "Controllers.InformesController.CorteDeLaminado":
                    List<string> partes = new List<string>();
                    partes.Add(this.Titulo);
                    return String.Join(" ", partes.ToArray());

                default:
                    return this.ToString();

            }

        }

        public string Titulo { get; set; }

        [DataType(DataType.MultilineText)]
        public string Detalle { get; set; }

    }

    public class Vitrea : Articulo
    {

        #region Sección IXmlSerializable

        public override System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // Si el elemento de partida no cerró en sí mismo,
            if (ReadXml_Start(reader) == _HasEndElement)
            {
                // Lee el elemento que finaliza la deserialización.
                reader.ReadEndElement();
            }
        }

        public override bool ReadXml_Start(System.Xml.XmlReader reader)
        {
            // Inicializaciones.
            bool _IsEmptyElement;
            string _Value;
            Laminado ElementoLaminado = new Laminado();

            // Deriva el comienzo de la lectura del elemento a la clase base.
            // Si el elemento de partida no cerró en sí mismo,
            if (base.ReadXml_Start(reader) == _HasEndElement)
            {
                if (reader.Name == "Tipo") this.Medida = reader.ReadElementString("Tipo");
                if (reader.Name == "Medida") this.Medida = reader.ReadElementString("Medida");
                if (reader.Name == "Transparente")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("Transparente");
                    this.Transparente = _IsEmptyElement ? null : ((_Value == "Sí") ? (bool?)true : false);
                }
                return _HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
            }
            else
            {
                return !_HasEndElement; // Transfiere el valor devuelto por la función ReadXml_Start;
            }
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("Tipo", Tipo);
            writer.WriteElementString("Medida", Medida);
            writer.WriteElementString("Transparente", Transparente.HasValue ? (Transparente.Value ? "Sí" : "No") : String.Empty);
        }

        #endregion

        #region Sección Copy & Paste

        /// <summary>
        /// Copia el contenido de esta vitrea.
        /// </summary>
        /// <returns>Una copia de la vitrea actual.</returns>
        public override Articulo Copiar()
        {
            Articulo _output = base.Copiar();
            (_output as Vitrea).Medida = this.Medida;
            (_output as Vitrea).Tipo = this.Tipo;
            (_output as Vitrea).Transparente = this.Transparente;
            return _output;
        }

        /// <summary>
        /// Pega datos sobre esta vitrea.
        /// </summary>
        /// <param name="desde">Vitrea con los datos que se desean pegar aquí.</param>
        public override void Pegar(Articulo desde)
        {
            if (desde != null)
            {
                base.Pegar(desde);
                if (desde is Vitrea)
                {
                    this.Medida = (desde as Vitrea).Medida;
                    this.Tipo = (desde as Vitrea).Tipo;
                    this.Transparente = (desde as Vitrea).Transparente;
                }
            }
        }

        #endregion

        #region Sección Equals

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="vitrea">La vitrea que se comparará con esta.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Vitrea vitrea, bool omiteEntidades = true, bool omiteClave = false)
        {

            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if (vitrea == null) return false;

            // Si ambas vitreas son en realidad la misma vitrea, debe devolver verdadero.
            if (ReferenceEquals(vitrea, this)) return true;

            return base.EntityEquals(vitrea, omiteEntidades, omiteClave) &&
                String.Equals(vitrea.Medida, this.Medida) &&
                String.Equals(vitrea.Tipo, this.Tipo) &&
                (vitrea.Transparente == this.Transparente);

        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="vitrea">La vitrea que se comparará con esta.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public override bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if ((objeto as Vitrea) == null) return false;

            // Si ambas vitreas son en realidad la misma vitrea, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Vitrea, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (!String.IsNullOrEmpty(this.Medida) ? this.Medida.GetHashCode() : 13*2222) ^
                    (!String.IsNullOrEmpty(this.Tipo) ? this.Tipo.GetHashCode() : 7*1321) ^
                    (!this.Transparente.HasValue ? 7 : (this.Transparente.Value ? 3 : 11));
            }
        }

        #endregion

        /// <summary>
        /// Crea una nueva instancia de la clase Vitrea.
        /// Este método sirve a los fines de que pueda obtenerse una nueva instancia de una vitrea aunque se llame desde una variable más amplia.
        /// </summary>
        /// <returns>Una nueva instancia de la clase Vitrea.</returns>
        public override Articulo CrearInstancia()
        {
            return new Vitrea();
        }

        /// <summary>
        /// Corrobora que la estructura básica de esta vitrea coincida con la estructura básica de la vitrea cuyo Código Tango se pasa como argumento.
        /// La estructura básica es el armado fundamental que da razón de ser a dicho código Tango.
        /// Por ejemplo, un cambio en la opacidad de la vitrea (transparente/fumé) no rompería con dicha estructura porque carece de relevancia en cuanto a su Código Tango.
        /// </summary>
        /// <param name="codigoTango">El código Tango cuya estructura se validará contra esta.</param>
        /// <returns>Verdadero si la estructura actual respeta a la contenida en la vitrea cuyo codigo Tango se pasa como argumento.</returns>
        public override bool ComprobarEstructura(string codigoTango)
        {

            if (String.IsNullOrEmpty(codigoTango)) return true;

            using (UnitOfWork UOW = new UnitOfWork())
            {
                Vitrea articuloOriginal = UOW.ArticuloRepository.ObtenerPorCodigoTango(codigoTango) as Vitrea;
                return
                    (articuloOriginal != null) &&
                    base.ComprobarEstructura(codigoTango) &&
                    String.Equals(this.Medida, articuloOriginal.Medida) &&
                    String.Equals(this.Tipo, articuloOriginal.Tipo);
            }

        }

        public override string TipoDeArticulo
        {
            get
            {
                return "Vitrea";
            }
        }

        public override string ToString()
        {
            return String.Format("{0} => Tipo[{1}] Medida[{2}] Transparente[{3}]", this.TipoDeArticulo, this.Tipo, this.Medida, this.Transparente.HasValue ? (this.Transparente.Value ? "Sí" : "No") : "N/E");
        }

        public string ToString(string modo)
        {

            switch (modo)
            {

                case "Optimizado":
                    var partes = new List<string>();
                    if (!String.IsNullOrEmpty(Tipo)) partes.Add(Tipo);
                    if (!String.IsNullOrEmpty(Medida)) partes.Add(Medida);
                    if (partes.Count == 0) partes.Add("Vitrea");
                    else partes[0] = String.Format("Vitrea {0}", partes[0]);
                    if (Transparente.HasValue && Transparente.Value) partes.Add("(TRANSPARENTE)");
                    return String.Join(" ", partes.ToArray());

                default:
                    return "";

            }

        }

        [Display(Name = "Tipo de Vitrea", ShortName = "Tipo")]
        public string Tipo { get; set; }

        [Display(Name = "Medida de la Vitrea", ShortName = "Medida")]
        public string Medida { get; set; }

        public bool? Transparente { get; set; }

    }

    public class Borde
    {

        #region Sección Equals

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="borde">El borde que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Borde borde, bool omiteEntidades = true, bool omiteClave = false)
        {
            
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if (borde == null) return false;

            // Si ambos artículos son en realidad el mismo artículo, debe devolver verdadero.
            if (ReferenceEquals(borde, this)) return true;

            return
                (this.Tipo == borde.Tipo) &&
                (this.Color == borde.Color) &&
                (this.Espesor == borde.Espesor);

        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="borde">El borde que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver false cuando el parámetro pasado es null.
            if ((objeto as Borde) == null) return false;

            // Si ambos artículos son en realidad el mismo artículo, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Borde, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (this.Color.HasValue ? (int)this.Color.Value + 6501 : 7 * 367) ^
                    ((int)this.Espesor  + 6542) ^
                    ((int)this.Tipo + 2323);
            }
        }

        #endregion

        [Display(Name = "Tipo de Borde de la Tapa", ShortName = "Borde Tipo")]
        public Pedidos.Models.Enums.TiposDeBordesDeTapas Tipo { get; set; }

        [Display(Name = "Tipo de Borde de la Tapa", ShortName = "Borde Tipo")]
        public string TipoNombre
        {
            get
            {
                return Tipo.GetEnumMemberDisplayName<Pedidos.Models.Enums.TiposDeBordesDeTapas>();
            }
        }

        [Display(Name = "Espesor del Borde de la Tapa", ShortName = "Borde Espesor")]
        public Pedidos.Models.Enums.EspesoresDeBordesDeTapas Espesor { get; set; }

        [Display(Name = "Espesor del Borde de la Tapa", ShortName = "Borde Espesor")]
        public string EspesorNombre
        {
            get
            {
                return Espesor.GetEnumMemberDisplayName<Pedidos.Models.Enums.EspesoresDeBordesDeTapas>();
            }
        }

        [Display(Name = "Color del Borde de la Tapa", ShortName = "Borde Color")]
        public Pedidos.Models.Enums.ColoresDeBordesDeTapas? Color { get; set; }

        [Display(Name = "Color del Borde de la Tapa", ShortName = "Borde Color")]
        public string ColorNombre
        {
            get
            {
                return Color.HasValue ? Color.GetEnumMemberDisplayName<Pedidos.Models.Enums.ColoresDeBordesDeTapas>() : null;
            }
        }

        public override string ToString()
        {
            var partes = new List<string>();
            if (Tipo != TiposDeBordesDeTapas.NC && Tipo != TiposDeBordesDeTapas.NE) partes.Add(TipoNombre);
            if (Espesor != EspesoresDeBordesDeTapas.NC && Espesor != EspesoresDeBordesDeTapas.NE) partes.Add(EspesorNombre);
            if (Color.HasValue && Color != ColoresDeBordesDeTapas.NC && Color != ColoresDeBordesDeTapas.NE) partes.Add(String.Format("({0})", ColorNombre));
            return String.Join(" ", partes.ToArray());
        }

    }

}