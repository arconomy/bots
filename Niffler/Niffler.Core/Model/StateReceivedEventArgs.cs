using System;

namespace Niffler.Model
{
    public enum StateDataType
    {
        FULL = 0,
        ITEM = 1
    }

    public class StateChangedEventArgs : EventArgs
    {
        public StateDataType StateDataType { get; set; }
        public State State { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
        public Exception Exception { get; set; }
    }
}