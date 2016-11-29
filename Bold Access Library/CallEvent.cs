using System;
using System.Collections.Generic;

namespace Microsoft.BAL
{
    internal class CallEvent : BoldBaseEvent
    {
        protected override void Initialize(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }

        protected internal override bool IsSameSourceInternal(BoldBaseEvent boldEvent)
        {
            throw new NotImplementedException();
        }

        protected internal override bool HasEventValueChange(BoldBaseEvent boldEvent)
        {
            throw new NotImplementedException();
        }

        protected internal override BoldEventHandlerArgs CreateBoldEventHandlerArgs()
        {
            throw new NotImplementedException();
        }
    }
}