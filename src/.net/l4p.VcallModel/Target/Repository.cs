using System;
using System.Collections.Generic;
using l4p.VcallModel.Hosting;

namespace l4p.VcallModel.Target
{
    interface IRepository
    {
        void AddHost(string callbackUri, IHostingPeer host);
        void RemoveHost(string callbackUri);
        bool HasHost(string callbackUri);

        int HostsCount { get; }
    }

    class Repository : IRepository
    {
        #region members

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

        void IRepository.AddHost(string callbackUri, IHostingPeer host)
        {
            throw new NotImplementedException();
        }

        void IRepository.RemoveHost(string callbackUri)
        {
            throw new NotImplementedException();
        }

        bool IRepository.HasHost(string callbackUri)
        {
            throw new NotImplementedException();
        }

        int IRepository.HostsCount
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}