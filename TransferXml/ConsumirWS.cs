using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace TransferXml
{
    public class ConsumirWS
    {
        public string EnderecoWeb { get; private set; }
        public string RetornoServicoString { get; private set; }
        public XmlDocument RetornoServicoXML { get; private set; }

        private readonly CookieContainer cookies = new CookieContainer();
        private static string EnveloparXML(string xmlBody)
        {
            var retorna = string.Empty;
            string cabecalho = "<ns2:cabecalho versao=\"3\" xmlns:ns2=\"http://www.ginfes.com.br/cabecalho_v03.xsd\"><versaoDados>3</versaoDados></ns2:cabecalho>";

            if (xmlBody.IndexOf("?>") >= 0)
            {
                xmlBody = xmlBody.Substring(xmlBody.IndexOf("?>") + 2);
            }

            string envelop = string.Empty;

            envelop = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">";
            envelop += "<soap:Body>";
            envelop += "<ns1:RecepcionarLoteRpsV3 xmlns:ns1=\"http://www.ginfes.com.br/\">";
            envelop += "<arg0>" + cabecalho + "</arg0>";
            envelop += "<arg1>" + xmlBody + "</arg1>";
            envelop += "</ns1:RecepcionarLoteRpsV3>";
            envelop += "</soap:Body>";
            envelop += "</soap:Envelope>";

            return envelop;
        }


        public void ExecutarServico(XmlDocument xml, object servico, X509Certificate2 certificado)
        {
            var soap = (WSSoap)servico;

            try
            {
                var urlpost = new Uri(EnderecoWeb);
                var soapXML = EnveloparXML(xml.OuterXml);
                var buffer2 = Encoding.UTF8.GetBytes(soapXML);

                ServicePointManager.Expect100Continue = false;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(RetornoValidacao);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(urlpost);
                httpWebRequest.Headers.Add("SOAPAction: " + soap.ActionWeb);
                httpWebRequest.CookieContainer = cookies;
                httpWebRequest.Timeout = 60000;
                httpWebRequest.ContentType = (string.IsNullOrEmpty(soap.ContentType) ? "application/soap+xml; charset=utf-8;" : soap.ContentType);
                httpWebRequest.Method = "POST";
                httpWebRequest.ClientCertificates.Add(certificado);
                httpWebRequest.ContentLength = buffer2.Length;

                var postData = httpWebRequest.GetRequestStream();
                postData.Write(buffer2, 0, buffer2.Length);
                postData.Close();

                var responsePost = (HttpWebResponse)httpWebRequest.GetResponse();
                var streamPost = responsePost.GetResponseStream();
                var streamReaderResponse = new StreamReader(streamPost, Encoding.UTF8);

                var retornoXml = new XmlDocument();
                retornoXml.LoadXml(streamReaderResponse.ReadToEnd());

                if (retornoXml.GetElementsByTagName(soap.TagRetorno)[0] == null)
                {
                    throw new Exception("Não foi possível localizar a tag <" + soap.TagRetorno + "> no XML retornado pelo webservice.");
                }

                RetornoServicoString = retornoXml.GetElementsByTagName(soap.TagRetorno)[0].ChildNodes[0].OuterXml;
                RetornoServicoXML = new XmlDocument
                {
                    PreserveWhitespace = false
                };
                RetornoServicoXML.LoadXml(RetornoServicoString);
            }
            catch
            {
                throw;
            }
        }

        public bool RetornoValidacao(object sender,
           X509Certificate certificate,
           X509Chain chain,
           SslPolicyErrors sslPolicyErros) => true;

    }
}
