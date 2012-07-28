/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace l4p.VcallModel.Helpers
{
    static class TcpHelpers
    {
        private static Socket _busySock;
        private static int _busyPort;

        public static int FindBusyTcpPort(this IHelpers Helpers)
        {
            if (_busyPort != 0)
                return _busyPort;

            var epoint = new IPEndPoint(IPAddress.Any, 0);
            _busySock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _busySock.Bind(epoint);
            var local = (IPEndPoint) _busySock.LocalEndPoint;
            _busyPort = local.Port;

            return _busyPort;
        }

        public static int FindAvailableTcpPort(this IHelpers Helpers)
        {
            var epoint = new IPEndPoint(IPAddress.Any, 0);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(epoint);
                var local = (IPEndPoint) socket.LocalEndPoint;
                return local.Port;
            }
        }
    }
}