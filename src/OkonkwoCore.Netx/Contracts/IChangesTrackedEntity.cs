using System;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IChangesTrackedEntity
    {
        DateTime? DateCreated { get; set; }
        string CreatedBy { get; set; }
        DateTime? DateModified { get; set; }
        string ModifiedBy { get; set; }
    }
}
