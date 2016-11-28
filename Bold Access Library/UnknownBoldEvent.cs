using System;
using System.Collections.Generic;

namespace Microsoft.BAL
{
    internal class UnknownBoldEvent : BoldBaseEvent
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
}