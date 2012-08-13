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
    class ActiveThreadTests
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

        #region private

        private void success_for_first_call()
        {
            _done = true; 
            _tm.Stop();
        }

        private void failure_then_success()
        {
            if (_counter == 0)
            {
                _counter++;
                throw new ApplicationException("First call is programmed to fail");
            }

            _done = true;
            _tm.Stop();
        }

        private void permanent_failure()
        {
            throw new ApplicationException("Permanent failure");
        }

        #endregion

        [Test]
        public void Start_should_wait_until_thread_is_started()
        {
            var thr = ActiveThread.New();

            thr.Start();

            thr.PostAction(() => _done = true);
            Thread.Sleep(100);

            Assert.That(_done, Is.True);

            thr.Stop();
        }

        [Test]
        public void PostAction_should_succeed()
        {
            var thr = ActiveThread.New();
            thr.Start();

            thr.PostAction(() => _done = true);
            thr.Stop();

            Assert.That(_done, Is.True);
        }

        [Test]
        public void DoOnce_should_succeed()
        {
            var thr = ActiveThread.New();
            thr.Start();

            thr.PostAction(
                () => thr.DoOnce(500, "", success_for_first_call, "testing once"));

            Thread.Sleep(300);
            thr.Stop();

            Assert.That(_done, Is.True);
            Assert.That(_tm.ElapsedMilliseconds, Is.LessThan(100));

        }

        [Test]
        public void DoOnceWithRetry_should_succeed_after_retry()
        {
            var thr = ActiveThread.New();
            thr.Start();

            thr.PostAction(
                () => thr.DoOnce(500, "cancelation_token", failure_then_success, "testing once"));

            Thread.Sleep(1000);
            thr.Stop();

            Assert.That(_counter, Is.EqualTo(1));
            Assert.That(_done, Is.True);
            Assert.That(_tm.ElapsedMilliseconds, Is.GreaterThan(400));
        }

        [Test]
        public void FailureAfterTimeout_should_be_skipped_after_failure_timeout()
        {
            var thr = ActiveThread.New(new ActiveThread.Config { FailureTimeout = 500 });
            thr.Start();

            thr.PostAction(
                () => thr.DoOnce(100, "cancelation_token", permanent_failure, "testing once"));

            Thread.Sleep(200);

            var counters = thr.Counters;
            Assert.That(counters.Vcall_State_DurableOperations, Is.EqualTo(1));

            Thread.Sleep(1500);
            thr.Stop();

            counters = thr.Counters;
            Assert.That(counters.Vcall_State_DurableOperations, Is.EqualTo(0));
        }
    }
}