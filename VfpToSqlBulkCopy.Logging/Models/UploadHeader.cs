//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// http://www.entityframeworktutorial.net/entityframework6/create-entity-data-model.aspx
namespace VfpToSqlBulkCopy.Logging.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UploadHeader
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UploadHeader()
        {
            this.UploadDetail = new HashSet<UploadDetail>();
        }
    
        public int Id { get; set; }
        public System.DateTime Begin { get; set; }
        public Nullable<System.DateTime> End { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UploadDetail> UploadDetail { get; set; }
    }
}
