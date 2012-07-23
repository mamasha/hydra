/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

namespace l4p.VcallModel.Discovery
{
    struct DebugCounters
    {
        public int HelloMsgsSent { get; set; }
        public int HelloMsgsRecieved { get; set; }
        public int HelloMsgsFiltered { get; set; }
        public int MyHelloMsgsReceived { get; set; }
        public int OtherHelloMsgsReceived { get; set; }
        public int HelloNotificationsProduced { get; set; }
        public int ByeNotificationsProduced { get; set; }
    }
}