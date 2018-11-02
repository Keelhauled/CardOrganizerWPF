using System;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace CardOrganizerWPF
{
    class RPCServer
    {
        public RPCServer(string name, int port)
        {
            TcpChannel tcpChannel = new TcpChannel(port);
            ChannelServices.RegisterChannel(tcpChannel, false);

            Type commonInterfaceType = typeof(RemotingMessenger);
            RemotingConfiguration.RegisterWellKnownServiceType(commonInterfaceType, name, WellKnownObjectMode.Singleton);
        }
    }
}
