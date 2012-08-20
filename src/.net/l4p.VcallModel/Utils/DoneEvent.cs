using System.Threading;

namespace l4p.VcallModel.Utils
{
    public interface IDoneEvent
    {
        int NotReadyCount { get; }

        void Signal();
        bool Wait(int timeout);
    }

    class DoneEvent : IDoneEvent
    {
        #region members

        private CountdownEvent _event;

        #endregion

        #region construction

        public static IDoneEvent New(int initialCount)
        {
            return
                new DoneEvent(initialCount);
        }

        private DoneEvent(int initialCount)
        {
            _event = new CountdownEvent(initialCount);
        }

        #endregion

        #region IDoneEvent

        int IDoneEvent.NotReadyCount
        {
            get { return _event.CurrentCount; }
        }

        void IDoneEvent.Signal()
        {
            _event.Signal();
        }

        bool IDoneEvent.Wait(int timeout)
        {
            return
                _event.Wait(timeout);
        }

        #endregion
    }
}