/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace l4p.VcallModel.Discovery
{
    class Subscriber
    {
        public IDisposable Subject { get; set; }
        public string ResolvingKey { get; set; }
        public HostPeerNotification Notify { get; set; }
        public Uri ResolvingScope { get; set; }

        private readonly Dictionary<string, DateTime> _aliveCallbacks = new Dictionary<string, DateTime>();

        public void GotAliveCallback(string callbackUri, DateTime now, List<Action> notifications)
        {
            if (_aliveCallbacks.ContainsKey(callbackUri) == false)
            {
                notifications.Add(() => Notify(callbackUri, true));
            }

            _aliveCallbacks[callbackUri] = now;
        }

        public void RemoveDeadCallbacks(DateTime now, TimeSpan aliveSpan, List<Action> notifications)
        {
            var aliveCallbacks = _aliveCallbacks.ToArray();

            foreach (var pair in aliveCallbacks)
            {
                var lastAliveMsg = pair.Value;

                if (now - lastAliveMsg > aliveSpan)
                {
                    string callbackUri = pair.Key;
                    _aliveCallbacks.Remove(callbackUri);

                    notifications.Add(() => Notify(callbackUri, false));
                }
            }
        }
    }
}