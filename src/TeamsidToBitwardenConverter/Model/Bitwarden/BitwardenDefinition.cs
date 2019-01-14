using System;

namespace TeamsidToBitwardenConverter.Model.Bitwarden
{
    public class BitwardenDefinition
    {
        public BitwardenCollection[] collections { get; set; }
        public BitwardenFolder[] folders { get; set; }
        public BitwardenItem[] items { get; set; }
    }

    public class BitwardenItem
    {
        public string id { get; set; }
        public string organizationId { get; set; }
        public string[] collectionIds { get; set; }
        public bool favorite { get; set; }
        public string folderId { get; set; }
        // login = 1, card = 3
        public int type { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public BitwardenField[] fields { get; set; }
        public BitwardenLogin login { get; set; }
    }

    public class BitwardenLogin
    {
        public BitwardenUri[] uris { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        // authenticator = null (premium only)
        public string totp { get; set; }
    }

    public class BitwardenUri
    {
        // default = null
        public object match { get; set; }
        public string uri { get; set; }
    }

    public class BitwardenField
    {
        public string name { get; set; }
        public string value { get; set; }
        // custom = 0, hidden = 1, boolean = 2
        public int type { get; set; }
    }
}
