using Pedidos.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using CustomExtensions;
using System.ComponentModel.DataAnnotations;

namespace Pedidos.Models
{
    [Table("Laminados")]
    public class Laminado : IXmlSerializable
    {

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
            Muestrario ElementoMuestrario = new Muestrario();

            // Mueve el cursor hasta el primer elemento que contiene datos.
            reader.MoveToContent();

            // Lee los atributos del elemento.
            this.Laminado_CodigoId = reader.GetAttribute("Laminado_CodigoId");
            this.Laminado_MuestrarioId = Convert.ToInt16(reader.GetAttribute("Laminado_MuestrarioId"));

            // Pregunta si el elemento contiene datos o está vacío y lee el elemento de entrada.
            Boolean isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();

            // El valor devuelto indicará si la etiqueta en cuestión debe cerrarse posteriormente.
            _HasEndElement = !isEmptyElement;

            // Si el elemento de entrada contiene etiqueta de cierre,
            if (_HasEndElement)
            {
                // Lee contenido.
                if (reader.Name == "Nombre") this.Nombre = reader.ReadElementString("Nombre");
                if (reader.Name == "Textura") this.Textura = (TexturasDeLaminados)Convert.ToInt16(reader.ReadElementString("Textura"));
                if (reader.Name == "TexturaNombre") reader.ReadElementString("TexturaNombre");
                if (reader.Name == "Proveedor") this.Proveedor = (Proveedores)Convert.ToInt16(reader.ReadElementString("Proveedor"));
                if (reader.Name == "ProveedorNombre") reader.ReadElementString("ProveedorNombre");
                if (reader.Name == "Muestrario")
                {
                    if (!reader.IsEmptyElement)
                    {
                        ElementoMuestrario.ReadXml(reader.ReadSubtree());
                        this.Muestrario = ElementoMuestrario;
                    }
                    // Si el elemento no está vacío: Luego del comando ReadSubtree, el cursor queda sobre el elemento de cierre.
                    // Si el elemento está vacío: El elemento se cierra en sí mismo, el cursor queda sobre el elemento (que también es de cierre).
                    reader.Read();
                }
            }

            return _HasEndElement;
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Laminado_CodigoId", Laminado_CodigoId);
            writer.WriteAttributeString("Laminado_MuestrarioId", Laminado_MuestrarioId.ToString());
            writer.WriteElementString("Nombre", Nombre);
            writer.WriteElementString("Textura", ((int)Textura).ToString());
            writer.WriteElementString("TexturaNombre", Textura.GetEnumMemberDisplayName<TexturasDeLaminados>().ToString());
            writer.WriteElementString("Proveedor", ((int)Proveedor).ToString());
            writer.WriteElementString("ProveedorNombre", Proveedor.GetEnumMemberDisplayName<Proveedores>().ToString());
            writer.WriteStartElement("Muestrario");
            Muestrario.WriteXml(writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="laminado">El laminado que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Laminado laminado, bool omiteEntidades = true, bool omiteClave = false)
        {
            
            // El overriding de Equals debe devolver falso cuando el parámetro pasado es null.
            if (laminado == null) return false;

            // Si ambos laminados son en realidad el mismo laminado, debe devolver verdadero.
            if (ReferenceEquals(laminado, this)) return true;

            return
                (omiteClave ? true : this.Laminado_CodigoId.Equals(laminado.Laminado_CodigoId)) &&
                (omiteClave ? true : this.Laminado_MuestrarioId == laminado.Laminado_MuestrarioId) &&
                this.Nombre.Equals(laminado.Nombre) &&
                (this.Proveedor == laminado.Proveedor) &&
                (this.Textura == this.Textura);

        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="laminado">El laminado que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver falso cuando el parámetro pasado es null.
            if ((objeto as Laminado) == null) return false;

            // Si ambos laminados son en realidad el mismo laminado, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Laminado, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (!String.IsNullOrEmpty(this.Laminado_CodigoId) ? this.Laminado_CodigoId.GetHashCode() : 82156) ^
                    (this.Laminado_MuestrarioId + 98765) ^
                    (!String.IsNullOrEmpty(this.Nombre) ? this.Nombre.GetHashCode() : 321) ^
                    ((int)this.Proveedor + 3218) ^
                    ((int)this.Textura + 21578);
            }
        }

        [Key, Column(Order = 0), InverseProperty("MuestrarioId")]
        public int Laminado_MuestrarioId { get; set; }

        [Key, Column(Order = 1)]
        public string Laminado_CodigoId { get; set; }

        public string Nombre { get; set; }

        public Pedidos.Models.Enums.TexturasDeLaminados Textura { get; set; }

        public string TexturaNombre
        {
            get
            {
                return Textura.GetEnumMemberDisplayName<Pedidos.Models.Enums.TexturasDeLaminados>();
            }
        }

        public Pedidos.Models.Enums.Proveedores Proveedor { get; set; }

        public string ProveedorNombre
        {
            get
            {
                return Proveedor.GetEnumMemberDisplayName<Pedidos.Models.Enums.Proveedores>();
            }
        }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey("Laminado_MuestrarioId")]
        public virtual Muestrario Muestrario { get; set; }

    }

}