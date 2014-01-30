using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.ServiceModel.Channels;

namespace TestOnvif
{
    /// <summary>
    /// Объект этого класса просто добавляет в каждый пакет информацию о пользователе и пароле.
    /// UsernameToken используется для того, чтобы перевести данные "пользователь-пароль" в
    /// заголовок xml, который требует камера
    /// </summary>
    public class PasswordDigestMessageInspector : IClientMessageInspector
    {
        #region IClientMessageInspector Members

        public string Username { get; set; }
        public string Password { get; set; }

        public PasswordDigestMessageInspector(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            //throw new NotImplementedException();
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            UsernameToken token = new UsernameToken(this.Username, this.Password);

            // Serialize the token to XML
            XmlElement securityToken = token.GetXml(new XmlDocument());
            MessageHeader securityHeader = MessageHeader.CreateHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", securityToken, false);
            request.Headers.Add(securityHeader);

            // complete
            return Convert.DBNull;
        }

        #endregion
    }
}
