using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Pedidos.Models.Enums;

namespace Pedidos.Models.Enums
{

    public enum NivelesDeMuestrarios
	{
        Fabricante = 1,
        MesasLT = 2,
        Grupo = 3,
        Cliente = 4
	}

}

namespace Pedidos.Models
{

    [Table("Muestrarios")]
    public class Muestrario : IXmlSerializable
    {

        protected const bool _HasEndElement = true;

        public Muestrario()
        {
            this.Laminados = new HashSet<Laminado>();
        }

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
            this.MuestrarioId = Convert.ToInt16(reader.GetAttribute("MuestrarioId"));

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
            }

            return _HasEndElement;
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("MuestrarioId", MuestrarioId.ToString());
            writer.WriteElementString("Nombre", Nombre);
        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="muestrario">El muestrario que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Muestrario muestrario, bool omiteEntidades = true, bool omiteClave = false)
        {

            // El overriding de Equals debe devolver falso cuando el parámetro pasado es null.
            if (muestrario == null) return false;

            // Si ambos laminados son en realidad el mismo laminado, debe devolver verdadero.
            if (ReferenceEquals(muestrario, this)) return true;

            return
                (omiteClave ? true : this.MuestrarioId == muestrario.MuestrarioId) &&
                this.ClienteId.Equals(muestrario.ClienteId) &&
                (this.Nivel == muestrario.Nivel) &&
                this.Nombre.Equals(muestrario.Nombre);

        }

        /// <summary>
        /// Comprueba si una entidad es igual a esta comparando sus propiedades por valor.
        /// La referencia de Equals indica que además deben analizarse, por valor y en cascada, todos los POCOs asociados al que se está analizando.
        /// Sin embargo, este método en particular se diferencia del original en que está pensado específicamente para entidades,
        /// por lo que el método no actua en cascada sobre entidades asociadas como debería actuar si se siguieran los lineamientos oficiales
        /// (sí actúa sobre POCOs que no sean entidades o ComplexProperties) Las entidades relacionadas no se compararán por valor, sino que se hará por referencia.
        /// </summary>
        /// <param name="muestrario">El muestrario que se comparará con este.</param>
        /// <param name="omiteEntidades">Indica si se deben incluir en la comparación las entidades relacionadas, o deben quedar afuera del cálculo por completo.
        /// Si se activa este parametro (valor por defecto) no se analizará nada de las mismas, ni por valor ni por referencia,
        /// y sólo se procederá a comparar las propiedades "locales" del objeto, los subobjetos POCOs del tipo ComplexProperties, y los POCOs que no sean entidades de base de datos.</param>
        /// <param name="omiteClave">Indica si debe omitirse la comparación de claves. Esto se debe a que podría pensarse que dos entidades jamás podrán ser iguales, dado que obligatoriamente tendrán claves distintas. Esta opción permite cierta flexibilidad a nivel código.</param>
        /// <returns>Devuelve verdadero si ambos objetos son considerados iguales en valores contenidos.</returns>
        public bool EntityEquals(Object objeto, bool omiteEntidades = true, bool omiteClave = false)
        {
            // El overriding de Equals debe devolver falso cuando el parámetro pasado es null.
            if ((objeto as Muestrario) == null) return false;

            // Si ambos laminados son en realidad el mismo laminado, debe devolver verdadero.
            if (ReferenceEquals(objeto, this)) return true;

            else return this.EntityEquals(objeto as Muestrario, omiteEntidades, omiteClave);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode() ^
                    (!String.IsNullOrEmpty(this.ClienteId) ? this.ClienteId.GetHashCode() : 25695) ^
                    (this.MuestrarioId + 2365231) ^
                    ((int)this.Nivel + 3215) ^
                    (!String.IsNullOrEmpty(this.Nombre) ? this.Nombre.GetHashCode() : 3654);
            }
        }

        public int MuestrarioId { get; set; }

        public string Nombre { get; set; }

        public NivelesDeMuestrarios Nivel { get; set; }

        public string ClienteId { get; set; }

        public virtual Cliente Cliente { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [InverseProperty("Muestrario")]
        public virtual ICollection<Laminado> Laminados { get; set; }

    }

}