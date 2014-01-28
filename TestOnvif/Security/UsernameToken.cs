using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml;

namespace TestOnvif
{
    /// <summary>
    /// Класс, с помошью объектов которого создаётся заголовок, содержащий
    /// информацию о пользователе, вызывающем какой-то привелегированный метод
    /// (например, перезагрузка камеры, прошивка новой версии по и т. д.)
    /// Стандарт описан в ONVIF Programmer's Guide.pdf в разделе 6.1 Authentication
    /// </summary>
    class UsernameToken
    {
        /// <summary>
        /// Бредогенератор
        /// </summary>
        //private static Random rnd = new Random((int)(Stopwatch.GetTimestamp() % int.MaxValue));
        private RNGCryptoServiceProvider cryptoRandom = new RNGCryptoServiceProvider();

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Случайная последовательность из 16 байт (бред)
        /// </summary>
        public byte[] Nonce { get; private set; }

        /// <summary>
        /// Время создания этого объекта
        /// </summary>
        public DateTime Created { get; private set; }

        public UsernameToken(string username, string password)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            if (username.Length == 0)
            {
                throw new ArgumentException("username");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            if (password.Length == 0)
            {
                throw new ArgumentException("password");
            }

            Username = username;
            Password = password;
            Nonce = new byte[16];
            cryptoRandom.GetBytes(Nonce);
            //rnd.NextBytes(Nonce);
            Created = DateTime.Now;
        }

        private static byte[] ComputePasswordDigest(byte[] nonce, DateTime created, string secret)
        {
            if ((nonce == null) || (nonce.Length == 0))
            {
                throw new ArgumentNullException("nonce");
            }
            if (secret == null)
            {
                throw new ArgumentNullException("secret");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(XmlConvert.ToString(created.ToUniversalTime(), "yyyy-MM-ddTHH:mm:ssZ"));
            byte[] sourceArray = Encoding.UTF8.GetBytes(secret);
            byte[] destinationArray = new byte[(nonce.Length + bytes.Length) + sourceArray.Length];

            Array.Copy(nonce, destinationArray, nonce.Length);
            Array.Copy(bytes, 0, destinationArray, nonce.Length, bytes.Length);
            Array.Copy(sourceArray, 0, destinationArray, nonce.Length + bytes.Length, sourceArray.Length);

            return Hash(destinationArray);
        }

        private static byte[] Hash(byte[] value)
        {
            return SHA1.Create().ComputeHash(value);
        }

        public XmlElement GetXml(XmlDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            XmlElement usernameToken = document.CreateElement("wsse", "UsernameToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

            XmlElement username = document.CreateElement("wsse", "Username", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            username.InnerText = Username;
            usernameToken.AppendChild(username);

            XmlElement password = document.CreateElement("wsse", "Password", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            byte[] passwordDigest = ComputePasswordDigest(Nonce, this.Created, Password);
            password.SetAttribute("Type", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest");
            password.InnerText = Convert.ToBase64String(passwordDigest);
            usernameToken.AppendChild(password);

            XmlElement nonce = document.CreateElement("wsse", "Nonce", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            nonce.InnerText = Convert.ToBase64String(Nonce);
            usernameToken.AppendChild(nonce);

            XmlElement created = document.CreateElement("wsu", "Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            created.InnerText = XmlConvert.ToString(Created.ToUniversalTime(), "yyyy-MM-ddTHH:mm:ssZ");
            usernameToken.AppendChild(created);

            return usernameToken;
        }
    }
}
