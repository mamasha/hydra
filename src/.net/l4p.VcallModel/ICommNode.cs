/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using l4p.VcallModel.Utils;

namespace l4p.VcallModel
{
    public interface ICommNode
    {
        /// <summary>
        /// A unique guid based tag (id) </summary>
        string Tag { get; }

        /// <summary>
        /// Close this node </summary>
        void Close();

        /// 
    }
}