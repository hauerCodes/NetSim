using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetSim.Lib.Routing.Helpers
{
    public class MessageHandlerResolver
    {
        /// <summary>
        /// The search type
        /// </summary>
        private readonly Type searchType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerResolver"/> class.
        /// </summary>
        /// <param name="searchType">Type of the search.</param>
        public MessageHandlerResolver(Type searchType)
        {
            this.searchType = searchType;
        }

        /// <summary>
        /// Gets the handler method.
        /// searches a handler method with the dsrmessagehandler attribute and the 
        /// right message type and for incoming(false) or outgoing (true, default) messages.
        /// e.g. IncomingDsrRouteRequestMessageHandler
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="searchOutgoing">if set to <c>true</c> [search outgoing].</param>
        /// <returns></returns>
        public MethodInfo GetHandlerMethod(Type messageType, bool searchOutgoing = true)
        {
            var method =
                searchType
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault(m =>
                    {
                        MessageHandlerAttribute attribute =
                            m.GetCustomAttributes()
                                .FirstOrDefault(c => c.GetType() == typeof(MessageHandlerAttribute)) as
                            MessageHandlerAttribute;
                        return attribute != null && attribute.MessageToHandle == messageType && attribute.Outgoing == searchOutgoing;
                    });
            return method;
        }

    }
}
