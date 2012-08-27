/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using l4p.VcallModel.Utils;

namespace l4p.VcallModel.Core
{
    struct DurableOperationArgs
    {
        public int? RequiredRepeats { get; set; }
        public int? RepeatTimeout { get; set; }
        public int? RetryTimeout { get; set; }
        public int? RetriesToLive { get; set; }
        public int? TimeToLive { get; set; }

        public string CancelationTag { get; set; }
        public Action Action { get; set; }
        public string Comments { get; set; }
    }

    interface IDurableOperation
    {
        bool IsDone { get; }
        bool IsFailed { get; }
        int FailureCount { get; }
        Exception LastError { get; }
        string LastErrorMsg { get; }
        string Comments { get; }
        DateTime ToBeInvokedAt { get; }
        string CancelationTag { get; }

        bool Invoke();
    }

    class DurableOperation : IDurableOperation
    {
        #region internals

        struct CompiledArgs
        {
            public int RequiredRepeats { get; set; }
            public TimeSpan RepeatTimeout { get; set; }
            public TimeSpan RetryTimeout { get; set; }
            public int RetriresToLive { get; set; }
            public TimeSpan TimeToLive { get; set; }
            public DateTime StartedAt { get; set; }
            public string CancelationTag { get; set; }
            public Action Action { get; set; }
            public string Comments { get; set; }
        }

        #endregion

        #region members

        private static readonly ILogger _log = Logger.New<DurableOperation>();
        private static readonly IHelpers Helpers = HelpersInUse.All;

        private readonly CompiledArgs _args;

        private int _successCount;
        private int _failureCount;
        private int _consequentFailures;
        private DateTime _toBeInvokedAt;

        private DateTime _lastSucceededAt;
        private DateTime _lastFailedAt;
        private Exception _lastFailedWith;

        private bool _isDone;
        private bool _isFailed;

        #endregion

        #region construction

        public static IDurableOperation New(DurableOperationArgs args)
        {
            throw 
                Helpers.NewNotImplementedException();
        }

        public static IDurableOperation NewDoOnce(int retryTimeout, int timeToLive, string cancelationTag, Action action, string commentsFmt, params object[] commentsArgs)
        {
            var args = new DurableOperationArgs
                           {
                                RequiredRepeats = 1,
                                RetryTimeout = retryTimeout,
                                TimeToLive = timeToLive,
                                CancelationTag = cancelationTag,
                                Action = action,
                                Comments = Helpers.SafeFormat(commentsFmt, commentsArgs)
                           };

            return
                new DurableOperation(args, DateTime.Now);
        }

        private DurableOperation(DurableOperationArgs args, DateTime now)
        {
            _args = new CompiledArgs
            {
                RequiredRepeats = args.RequiredRepeats ?? 1,
                RepeatTimeout = TimeSpan.FromMilliseconds(args.RepeatTimeout ?? 0),
                RetryTimeout = TimeSpan.FromMilliseconds(args.RetryTimeout ?? 0),
                RetriresToLive = args.RetriesToLive ?? 0,
                TimeToLive = TimeSpan.FromMilliseconds(args.TimeToLive ?? 0),
                StartedAt = now,
                CancelationTag = args.CancelationTag,
                Action = args.Action,
                Comments = args.Comments,
            };

            initialize_state();
        }

        #endregion

        #region private

        private void assert_is_not_done()
        {
            Helpers.Assert(_isDone == false, _log, "Durable operation '{0}' is already done", _args.Comments);
        }

        private void assert_is_not_failed()
        {
            Helpers.Assert(_isFailed == false, _log, "Durable operation '{0}' has failure state", _args.Comments);
        }

        private void initialize_state()
        {
            _successCount = 0;
            _failureCount = 0;
            _consequentFailures = 0;
            _toBeInvokedAt = DateTime.MinValue;
            _lastSucceededAt = _args.StartedAt;
            _lastFailedAt = DateTime.MinValue;
            _lastFailedWith = null;
            _isDone = false;
            _isFailed = false;
        }

        private void had_successful_call(DateTime now)
        {
            assert_is_not_done();
            assert_is_not_failed();

            _successCount++;
            _consequentFailures = 0;

            _lastSucceededAt = now;
            _toBeInvokedAt = now + _args.RepeatTimeout;

            _isDone = _successCount == _args.RequiredRepeats;
        }

        private void had_failed_call(DateTime now, Exception ex)
        {
            assert_is_not_done();
            assert_is_not_failed();

            _failureCount++;
            _consequentFailures++;

            _lastFailedAt = now;
            _lastFailedWith = ex;
            _toBeInvokedAt = now + _args.RetryTimeout;

            // failed if has too many retries or has failure time span which is expired

            _isFailed =
                _consequentFailures == _args.RetriresToLive ||
                (_args.TimeToLive > TimeSpan.Zero && _lastSucceededAt + _args.TimeToLive < now);
        }

        #endregion

        #region IDurableOperation

        bool IDurableOperation.IsDone
        {
            get { return _isDone; }
        }

        bool IDurableOperation.IsFailed
        {
            get { return _isFailed; }
        }

        int IDurableOperation.FailureCount
        {
            get { return _failureCount; }
        }

        Exception IDurableOperation.LastError
        {
            get { return _lastFailedWith; }
        }

        string IDurableOperation.LastErrorMsg
        {
            get
            {
                return 
                    _lastFailedWith != null ? _lastFailedWith.Message : "no errors";
            }
        }

        string IDurableOperation.Comments
        {
            get { return _args.Comments; }
        }

        DateTime IDurableOperation.ToBeInvokedAt
        {
            get { return _toBeInvokedAt; }
        }

        string IDurableOperation.CancelationTag
        {
            get { return _args.CancelationTag; }
        }

        bool IDurableOperation.Invoke()
        {
            Exception error = null;

            try
            {
                _args.Action();
            }
            catch (Exception ex)
            {
                error = ex;
            }

            var now = DateTime.Now;

            if (error != null)
            {
                had_failed_call(now, error);
                _log.Warn(error, "'{0}' has failed (retries={2}); {1}", _args.Comments, error.Message, _consequentFailures);
                return false;
            }

            had_successful_call(now);
            _log.Trace("'{0}' is done (count={1})", _args.Comments, _successCount);

            return true;
        }

        #endregion
    }
}