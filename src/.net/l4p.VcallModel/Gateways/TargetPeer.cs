/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System.ServiceModel;

namespace l4p.VcallModel.Gateways
{
    [ServiceContract]
    interface ITargetPeer
    {
        [OperationContract]
        void UpdateSubjects();
    }

    class TargetPeer
    {
    }
}