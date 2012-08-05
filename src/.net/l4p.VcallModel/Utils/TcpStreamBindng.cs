/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System.ServiceModel;

namespace l4p.VcallModel.Utils
{
    class TcpStreamBindng : NetTcpBinding
    {
        private readonly int MAX_MSG_SIZE = 2 * 1024 * 1024;

        public TcpStreamBindng()
        {
            TransferMode = TransferMode.Streamed;

            MaxReceivedMessageSize = MAX_MSG_SIZE;
            MaxBufferSize = MAX_MSG_SIZE;
            MaxBufferPoolSize = MAX_MSG_SIZE;

            ReaderQuotas.MaxStringContentLength = MAX_MSG_SIZE;
            ReaderQuotas.MaxArrayLength = MAX_MSG_SIZE;

            Security.Mode = SecurityMode.None;
        }
    }
}