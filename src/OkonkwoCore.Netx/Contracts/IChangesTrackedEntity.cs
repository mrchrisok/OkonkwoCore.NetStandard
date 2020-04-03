using System;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IChangesTrackedEntity
    {
        DateTime? DateCreated { get; set; }
        DateTime? DateModified { get; set; }
    }
}
