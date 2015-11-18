using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace pure_csharp
{
    class Client
    {
        string GetUrl;
        string BeginUrl;
        int FirstParam;
        string SecondParam;
        string EndUrl;
        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient Client)
        {
            string Request = "";
            byte[] Buffer = new byte[1024];
            int Count;
            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                {
                    break;
                }
            }
            Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (ReqMatch == Match.Empty)
            {
                SendError(Client, 400);
                return;
            }
            string RequestUri = ReqMatch.Groups[1].Value;

            GetUrl = ReqMatch.Value;
            BeginUrl = Regex.Match(GetUrl, @"\?(.*)\.").Groups[1].Value;
            if (BeginUrl.Any())
            {
                if (BeginUrl[0] == 'c')
                {
                    FirstParam = Convert.ToInt32(Regex.Match(GetUrl, @"\=(.*)\&").Groups[1].Value);
                    EndUrl = Regex.Replace(GetUrl, "(concurrency=" + FirstParam.ToString() + "&sort=\\.?)", String.Empty);
                    SecondParam = Regex.Match(EndUrl, @"\?(.*)\ ").Groups[1].Value;
                    Thread.Sleep(1000);

                }
                if (BeginUrl[0] == 'g')
                {
                    FirstParam = Convert.ToInt32(Regex.Match(GetUrl, @"\=(.*)\ ").Groups[1].Value);
                    SecondParam = null;
                    Thread.Sleep(1000);
                }
            }

            string rUri = Uri.UnescapeDataString(Request);
            RequestUri = Uri.UnescapeDataString(RequestUri);

            if (RequestUri.IndexOf("..") >= 0)
            {
                SendError(Client, 400);
                return;
            }

            if (RequestUri.EndsWith("/"))
            {
                RequestUri += "index.html";
            }
        }
        public int GetFirstParam()
        {
            return FirstParam;
        }
        public string GetSecondParam()
        {
            return SecondParam;
        }
        private void SendError(TcpClient Client, int Code)
        {
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.Close();
        }
    }
}
