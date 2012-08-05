using System;
using System.Collections.Generic;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Target
{
    interface IRepository
    {
        void AddHosting(HostingInfo info);
        void RemoveHosting(string tag);
//        void HostIdDead(string callbackUri);

        int HostingsCount { get; }
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<TargetsPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private IDictionary<string, IHostingPeer> _hosts;

        #endregion

        #region construction

        public static IRepository New()
        {
            return 
                new Repository();
        }

        private Repository()
        {
            _hosts = new Dictionary<string, IHostingPeer>();
        }

        #endregion

        #region IRepository

        void IRepository.AddHosting(HostingInfo info)
        {
            throw 
                Helpers.NewNotImplementedException();
        }

        void IRepository.RemoveHosting(string tag)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        int IRepository.HostingsCount
        {
            get
            {
                throw
                    Helpers.NewNotImplementedException();
            }
        }

        #endregion
    }
}