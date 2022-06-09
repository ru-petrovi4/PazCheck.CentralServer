using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class Simuser : VersionEntity
    {
        [NotMapped]
        public string SubjectId => Id.ToString();
        /*
         * Login name
         */
        [Attr(PublicName="username")]
        public string Username { get; set; } = @"";
        [Attr(PublicName="password")]
        public string Password { get; set; } = @"";
        [Attr(PublicName="providername")]
        public string ProviderName { get; set; } = @"";
        [Attr(PublicName="providersubjectid")]
        public string ProviderSubjectId { get; set; } = @"";
        [Attr(PublicName="isactive")]
        public bool IsActive { get; set; } = true;
        //Castom properties
        [NotMapped]
        public string Name => $"{FirstName} {LastName}";
        [Attr(PublicName="firstname")]
        public string FirstName { get; set; } = @"";
        [Attr(PublicName="lastname")]
        public string LastName { get; set; } = @"";
        [Attr(PublicName="middlename")]
        public string MiddleName { get; set; } = @"";
        [Attr(PublicName="personnelnumber")]
        public string PersonnelNumber { get; set; } = @"";
        public int OfficeId { get; set; }
        [HasOne(PublicName="office")]
        public Office Office { get; set; }
        [Attr(PublicName="role")]
        public string Role { get; set; } = @"";        
    }
}
