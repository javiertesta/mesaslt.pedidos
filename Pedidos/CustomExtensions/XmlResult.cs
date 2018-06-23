using System;
using System.IO;
using System.Web.Mvc;
using System.Xml.Serialization;
using System.Web;
using System.Text;

namespace CustomExtensions
{
    public class XmlResult<T> : ActionResult
    {
        public T Data { private get; set; }

        public XmlResult(T objectToSerialize)
        {
            this.Data = objectToSerialize;
        }
        
        public override void ExecuteResult(ControllerContext context)
        {
            HttpContextBase httpContextBase = context.HttpContext;
            httpContextBase.Response.Buffer = true;
            httpContextBase.Response.Clear();

            string fileName = DateTime.Now.ToString("yyyyMMddHHmm") + ".xml";
            httpContextBase.Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            httpContextBase.Response.ContentType = "text/xml";

            using (MemoryStream stream = new MemoryStream()) 
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(writer, Data);
                httpContextBase.Response.BinaryWrite(stream.ToArray());
            }
        }
    }
}