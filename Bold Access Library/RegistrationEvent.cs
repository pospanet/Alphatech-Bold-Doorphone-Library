using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.BAL
{
    internal class RegistrationEvent : BoldBaseEvent
    {
        private const string RegistrationStatusKey = "SUCCESS";
        public bool Registered { get; private set; }
        protected override void Initialize(Dictionary<string, string> values)
        {
            Registered = values.Where(v => v.Key.Equals(RegistrationStatusKey)).Select(v => v.Value).First() == "0";
        }

        protected internal override bool IsSameSourceInternal(BoldBaseEvent boldEvent)
        {
            return boldEvent is RegistrationEvent;
        }

        protected internal override bool HasEventValueChange(BoldBaseEvent boldEvent)
        {
            return !((RegistrationEvent)boldEvent).Registered.Equals(Registered);
        }

        protected internal override BoldEventHandlerArgs CreateBoldEventHandlerArgs()
        {
            return new RegistrationEventHandlerArgs(Registered);
        }
    }
    public class RegistrationEventHandlerArgs : BoldEventHandlerArgs
    {
        public bool Registered { get; private set; }

        public RegistrationEventHandlerArgs(bool registered)
        {
            Registered = registered;
        }
    }
}