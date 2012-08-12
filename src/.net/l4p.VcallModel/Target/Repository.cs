using System;
using System.Collections.Generic;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Target
{
    interface IRepository
    {
        void CleanUp(string id);

        void AddHosting(HostingInfo info);
        void RemoveHosting(string tag);
        bool HasHosting(HostingInfo info);
//        void HostIdDead(string callbackUri);

        int AliveCount { get; }
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

        void IRepository.CleanUp(string id)
        {
            throw
                Helpers.NewNotImplementedException();
        }

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

        bool IRepository.HasHosting(HostingInfo info)
        {
            throw
                Helpers.NewNotImplementedException();
        }

        int IRepository.AliveCount
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