using CustomExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml.Serialization;

namespace Pedidos.Models
{
    [Table("Gestiones")]
    public class Gestion : IXmlSerializable
    {
        public Gestion()
        {
            this.Pedidos = new HashSet<Pedido>();
            this.Historial = new HashSet<Gestion>();
        }

        public Gestion GetCopy()
        {
            var _output = new Gestion();
            _output.ClienteId = this.ClienteId;
            _output.FechaGestion = this.FechaGestion;
            _output.FechaBaja = this.FechaBaja;
            _output.UserName = this.UserName;
            _output.Observaciones = this.Observaciones;
            _output.RegistroOriginalId = this.RegistroOriginalId;
            return _output;
        }

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool _IsEmptyElement;
            string _Value;

            // Mueve el cursor hasta el primer elemento que contiene datos.
            reader.MoveToContent();

            // Lee los atributos del elemento.
            GestionId = Convert.ToInt16(reader.GetAttribute("GestionId"));
            
            // Pregunta si el elemento contiene datos o está vacío y lee el elemento de entrada.
            Boolean isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            
            // Si el elemento de entrada no contiene información,
            // entonces no tiene datos que rescatar ni tampoco elemento de cierre.
            if (!isEmptyElement)
            {
                FechaGestion = DateTime.ParseExact(reader.ReadElementString("FechaGestion"), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                UserName = reader.ReadElementString("UserName");
                Observaciones = reader.ReadElementString("Observaciones");
                ClienteId = reader.ReadElementString("ClienteId");

                if (reader.Name == "FechaBaja")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("FechaBaja");
                    FechaBaja = _IsEmptyElement ? null : (DateTime?)DateTime.ParseExact(_Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }

                if (reader.Name == "RegistroOriginalId")
                {
                    _IsEmptyElement = reader.IsEmptyElement;
                    _Value = reader.ReadElementString("RegistroOriginalId");
                    RegistroOriginalId = _IsEmptyElement ? null : (int?)Convert.ToInt16(_Value);
                }

                // Lee el elemento que finaliza la deserialización.
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("GestionId", GestionId.ToString());
            writer.WriteElementString("FechaGestion", FechaGestion.ToString("yyyy-MM-dd HH:mm:ss"));
            writer.WriteElementString("UserName", UserName);
            writer.WriteElementString("Observaciones", Observaciones);
            writer.WriteElementString("ClienteId", this.ClienteId);
            writer.WriteElementString("FechaBaja", FechaBaja.HasValue ? FechaBaja.Value.ToString("yyyy-MM-dd HH:mm:ss") : String.Empty);
            writer.WriteElementString("RegistroOriginalId", RegistroOriginalId.HasValue ? RegistroOriginalId.ToString() : String.Empty);
        }

        [Display(Name = "Código de Gestión", ShortName = "Gestión")]
        public int GestionId { get; set; }

        [Display(Name = "Fecha de Gestión", ShortName = "Fecha")]
        [CustomDate]
        [Required]
        public System.DateTime FechaGestion { get; set; }

        [Display(Name = "Último Usuario que Intervino", ShortName = "Usuario")]
        public string UserName { get; set; }

        [Display(Name = "Observaciones de la Gestión", ShortName = "Observaciones")]
        public string Observaciones { get; set; }

        [CustomClienteId(CustomClienteIdAttribute.Options.shouldExist, false)]
        [MaxLength(15)]
        public string ClienteId { get; set; }

        [Display(Name = "Fecha de Baja de la Gestión", ShortName = "Baja")]
        [CustomDate]
        public DateTime? FechaBaja { get; set; }

        [Display(Name = "Fecha de Baja de la Gestión", ShortName = "Baja")]
        public int? RegistroOriginalId { get; set; }

        public Gestion RegistroOriginal { get; set; }

        public virtual HashSet<Gestion> Historial { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Cliente Cliente { get; set; }

        public virtual HashSet<Pedido> Pedidos { get; set; }
    }
}