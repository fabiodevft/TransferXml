using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace TransferXml
{
    public class RecepcionarLoteRPS
    {
        public XmlDocument Xml { get; set; }
        public X509Certificate2 Certificado { get; set; }

        public string Retorno { get; set; }


        public RecepcionarLoteRPS()
        {
        }

        public RecepcionarLoteRPS(XmlDocument xml, X509Certificate2 certificado)
        {
            Xml = xml;
            Certificado = certificado;
        }

        public void Executar()
        {
            // consumir ws


        }     

    }
}
