/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Diagnostics;
using System.Threading;
using l4p.VcallModel.Core;
using NUnit.Framework;

namespace l4p.VcallTests.Core
{
    [TestFixture]
    class ActiveThreadWithDurablesTests
    {
        private bool _done;
        private int _counter;
        private Stopwatch _tm;

        [SetUp]
        public void Init()
        {
            _done = false;
            _counter = 0;
            _tm = Stopwatch.StartNew();
        }

        [Test]
        public void PostAction_should_succeed()
        {
            var thr = ActiveThread.New("testing");
            thr.Start();

            thr.PostAction(() => _done = true);
            thr.Stop();

            Assert.That(_done, Is.True);
        }

        private void schedule_once(IActiveThread thr)
        {
            thr.DoOnce(500, "",
                () => { _done = true; _tm.Stop(); } , "testing once");
        }

        private void failure_for_first_call()
        {
            if (_counter == 0)
            {
                _counter++;
                throw new ApplicationException("First call is programmed to fail");
            }

            _done = true;
            _tm.Stop();
        }

        private void schedule_once_with_retry(IActiveThread thr)
        {
            thr.DoOnce(500, "cancelation_token",
                failure_for_first_call, "testing once");
        }

        [Test]
        public void DoOnce_should_succeed()
        {
            var thr = ActiveThread.New("testing");
            thr.Start();

            thr.PostAction(() => schedule_once(thr));

            Thread.Sleep(300);
            thr.Stop();

            Assert.That(_done, Is.True);
            Assert.That(_tm.ElapsedMilliseconds, Is.LessThan(100));

        }

        [Test]
        public void DoOnceWithRetry_should_succeed_after_retry()
        {
            var thr = ActiveThread.New("testing");
            thr.Start();

            thr.PostAction(() => schedule_once_with_retry(thr));

            Thread.Sleep(1000);
            thr.Stop();

            Assert.That(_counter, Is.EqualTo(1));
            Assert.That(_done, Is.True);
            Assert.That(_tm.ElapsedMilliseconds, Is.GreaterThan(400));
        }

        [Test, Ignore("Is not supported yet")]
        public void FailureAfterTwoRetries_should_throw_after_two_retries()
        {
            Assert.Fail();
        }

        [Test, Ignore("Is not supported yet")]
        public void FailureAfterTimeout_should_throw_after_consequent_failure_timeout_is_expired()
        {
            Assert.Fail();
        }

        [Test, Ignore("Is not supported yet")]
        public void ConcurrentDurablesTest_should_manage_two_durables()
        {
            Assert.Fail();
        }
    }
}