using System;
using System.Collections.Generic;
using System.Linq;
using l4p.VcallModel.Core;
using l4p.VcallModel.Hosting;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Target
{
    interface IRepository
    {
        int HostingCount { get; }
        bool HasHosting(string tag);

        void AddHosting(HostingInfo info);
        void RemoveHosting(HostingInfo info);

        HostingInfo FindHosting(string tag);
        HostingInfo GetHosting(string tag);
        HostingInfo[] GetHostings();
    }

    class Repository : IRepository
    {
        #region members

        private static readonly ILogger _log = Logger.New<TargetsPeer>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly IDictionary<string, HostingInfo> _hostings;
        private readonly DebugCounters _counters;

        #endregion

        #region construction

        public static IRepository New()
        {
            return 
                new Repository();
        }

        private Repository()
        {
            _hostings = new Dictionary<string, HostingInfo>();
            _counters = Context.Get<ICountersDb>().NewCounters();
        }

        #endregion

        #region private

        private HostingInfo find_hosting(string tag)
        {
            var hostings =
                from hosting in _hostings.Values
                where hosting.Tag == tag || hosting.CallbackUri == tag
                select hosting;

            return
                hostings.FirstOrDefault();
        }

        #endregion

        #region IRepository

        int IRepository.HostingCount
        {
            get { return _hostings.Count; }
        }

        bool IRepository.HasHosting(string tag)
        {
            return
                _hostings.ContainsKey(tag);
        }

        void IRepository.AddHosting(HostingInfo info)
        {
            _hostings[info.Tag] = info;
            _counters.Targets_State_AliveHostings++;
        }

        void IRepository.RemoveHosting(HostingInfo info)
        {
            bool wasThere = _hostings.Remove(info.Tag);

            if (wasThere)
                _counters.Targets_State_AliveHostings--;
        }

        HostingInfo IRepository.FindHosting(string tag)
        {
            return
                find_hosting(tag);
        }

        HostingInfo IRepository.GetHosting(string tag)
        {
            var hosting = find_hosting(tag);

            if (hosting == null)
            {
                throw
                    Helpers.MakeNew<TargetsPeerException>(null, _log, "Can't find hosting.{0}", tag);
            }

            return hosting;
        }

        HostingInfo[] IRepository.GetHostings()
        {
            return
                _hostings.Values.ToArray();
        }

        #endregion
    }
}