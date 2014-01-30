using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;

namespace TestOnvif
{
    /// <summary>
    /// Класс, который нужен для вызова методов камеры, которым нужен привелегированный доступ
    /// (админский). Как устроено взаимодействие объектов - хз. Точно понятно только, что объект
    /// этого класса добавляет объект PasswordDigestMessageInspector для того, чтобы он засунул
    /// в заголовок посылаемого xml'я имя пользователя и пароль.
    /// </summary>
    public class PasswordDigestBehavior : IEndpointBehavior
    {


        public string Username { get; set; }
        public string Password { get; set; }

        public PasswordDigestBehavior(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //throw new NotImplementedException();
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new PasswordDigestMessageInspector(this.Username, this.Password));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            //throw new NotImplementedException();
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
