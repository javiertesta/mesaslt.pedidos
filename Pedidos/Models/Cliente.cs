using Pedidos.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using CustomExtensions;
using System.Xml.Linq;

namespace Pedidos.Models
{
    [Table("Clientes")]
    public class Cliente : IXmlSerializable
    {
        public Cliente()
        {
            this.Contactos = new HashSet<Gestion>();
        }

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();
            ClienteId = reader.GetAttribute("ClienteId");
            Boolean isEmptyElement = reader.IsEmptyElement; // (1)
            reader.ReadStartElement();
            if (!isEmptyElement) // (1)
            {
                if (reader.Name == "Zona") Zona = (Zonas)Convert.ToInt16(reader.ReadElementString("Zona"));
                if (reader.Name == "ZonaNombre") reader.ReadElementString("ZonaNombre");
                if (reader.Name == "RazonSocial") RazonSocial = reader.ReadElementString("RazonSocial");
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("ClienteId", ClienteId.ToString());
            writer.WriteElementString("Zona", ((int)Zona).ToString());
            writer.WriteElementString("ZonaNombre", Zona.GetEnumMemberDisplayName<Zonas>().ToString());
            writer.WriteElementString("RazonSocial", RazonSocial);
        }

        [Display(Name = "Código de Cliente", ShortName = "Cliente")]
        [Key]
        [MaxLength(15)]
        public string ClienteId { get; set; }

        [Display(Name = "Zona Geográfica", ShortName = "Zona")]
        public Pedidos.Models.Enums.Zonas Zona { get; set; }

        [Display(Name = "Zona Geográfica", ShortName = "Zona")]
        public string ZonaNombre
        {
            get
            {
                return Zona.GetEnumMemberDisplayName<Zonas>();
            }
        }

        [Display(Name = "Razón Social", ShortName = "Razón Social")]
        public string RazonSocial { get; set; }

        public virtual HashSet<Muestrario> Muestrarios { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual HashSet<Gestion> Contactos { get; set; }
    }
}