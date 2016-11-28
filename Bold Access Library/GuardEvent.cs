using System;
using System.Collections.Generic;

namespace Microsoft.BAL
{
    internal class GuardEvent : BoldBaseEvent
    {
        protected override void Initialize(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }

        protected internal override bool IsSameSourceInternal(BoldBaseEvent boldEvent)
        {
            throw new NotImplementedException();
        }

        protected internal override bool EventValueChanged(BoldBaseEvent boldEvent)
        {
            throw new NotImplementedException();
        }

        protected internal override BoldEventHandlerArgs CreateBoldEventHandlerArgs(BoldBaseEvent boldEvent)
        {
            throw new NotImplementedException();
        }
    }
    public class DoorEventHandlerArgs
    {
        public int DoorId { get; }
        public SensorStatus DoorStatus { get; }
    }

    public enum SensorStatus
    {
        Open,
        Closed
    }

    public class RelayEventHandlerArgs
    {
        public int RelayId { get; }
        public SensorStatus RelayStatus { get; }
    }


}