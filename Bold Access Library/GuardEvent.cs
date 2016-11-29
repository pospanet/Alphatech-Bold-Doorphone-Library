using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.BAL
{
    internal class GuardEvent : BoldBaseEvent
    {
        public string SensorId { get; private set; }
        public SensorStatus SensorStatus { get; private set; }

        private const string MessageKey = "MESSAGE";

        public GuardEvent()
        {
            SensorId = string.Empty;
        }

        protected override void Initialize(Dictionary<string, string> values)
        {
            SensorId = values.Where(v => v.Key.Equals(MessageKey)).Select(v => v.Value.Substring(0, 2)).First();
            SensorStatus = values.Where(v => v.Key.Equals(MessageKey)).Select(v => v.Value.Substring(2, 1)).First() == "0"
                ? SensorStatus.Off
                : SensorStatus.On;
        }

        protected internal override bool IsSameSourceInternal(BoldBaseEvent boldEvent)
        {
            if (string.IsNullOrEmpty(SensorId))
            {
                throw new NullReferenceException("Not initialized, yet");
            }
            if (!(boldEvent is GuardEvent))
            {
                return false;
            }
            return SensorId.Equals(((GuardEvent) boldEvent).SensorId);
        }

        protected internal override bool HasEventValueChange(BoldBaseEvent boldEvent)
        {
            return !SensorStatus.Equals(((GuardEvent) boldEvent).SensorStatus);
        }

        protected internal override BoldEventHandlerArgs CreateBoldEventHandlerArgs()
        {
            BoldEventHandlerArgs eventHandlerArgs;
            switch (SensorId[1])
            {
                case 'S':
                    eventHandlerArgs=new RelayEventHandlerArgs(int.Parse(SensorId[0].ToString()), SensorStatus);
                    break;
                case 'D':
                    eventHandlerArgs = new DoorEventHandlerArgs(int.Parse(SensorId[0].ToString()), SensorStatus);
                    break;
                default:
                    eventHandlerArgs=new BoldEventHandlerArgs();
                    break;
            }
            return eventHandlerArgs;
        }
    }
    public class DoorEventHandlerArgs: BoldEventHandlerArgs
    {
        public int DoorId { get; private set; }
        public SensorStatus DoorStatus { get; private set; }
        public DoorEventHandlerArgs(int id, SensorStatus sensorStatus)
        {
            DoorId = id;
            DoorStatus = sensorStatus;
        }
    }

    public enum SensorStatus
    {
        On,
        Off
    }

    public class RelayEventHandlerArgs: BoldEventHandlerArgs
    {
        public int RelayId { get; private set; }
        public SensorStatus RelayStatus { get; private set; }

        public RelayEventHandlerArgs(int id, SensorStatus sensorStatus)
        {
            RelayId = id;
            RelayStatus = sensorStatus;
        }
    }
}