using System.Collections.Generic;
using System.Linq;

namespace Microsoft.BAL
{
    internal abstract class BoldBaseEvent
    {
        private const string EventTypeKey = "EVENT";
        private const string RegistrationEventTypeKey = "REGISTRATION";
        private const string CallEventTypeKey = "CALL";
        private const string GuardEventTypeKey = "GUARD";

        internal static BoldBaseEvent CreateEvent(Dictionary<string, string> values)
        {
            BoldBaseEvent boldBaseEvent;
            KeyValuePair<string, string> eventPair = values.First(p => p.Key.Equals(EventTypeKey));
            switch (eventPair.Key)
            {
                case RegistrationEventTypeKey:
                    boldBaseEvent = new RegistrationEvent();
                    break;
                case CallEventTypeKey:
                    boldBaseEvent = new CallEvent();
                    break;
                case GuardEventTypeKey:
                    boldBaseEvent = new GuardEvent();
                    break;
                default:
                    boldBaseEvent = new UnknownBoldEvent();
                    break;
            }
            boldBaseEvent.Initialize(values);
            return boldBaseEvent;
        }

        protected abstract void Initialize(Dictionary<string, string> values);

        public bool IsSameSource(BoldBaseEvent boldEvent)
        {
            return GetType() == boldEvent.GetType() && IsSameSourceInternal(boldEvent);
        }

        protected internal abstract bool IsSameSourceInternal(BoldBaseEvent boldEvent);
        protected internal abstract bool HasEventValueChange(BoldBaseEvent boldEvent);
        protected internal abstract BoldEventHandlerArgs CreateBoldEventHandlerArgs();
    }
}