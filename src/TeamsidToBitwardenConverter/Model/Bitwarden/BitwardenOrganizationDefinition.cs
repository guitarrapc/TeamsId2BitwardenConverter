using System;
using System.Linq;

namespace TeamsidToBitwardenConverter.Model.Bitwarden
{
    public class BitwardenOrganizationDefinition
    {
        public BitwardenOrganization[] organizations { get; set; }

        private string defaultIdName = null;
        public string GetId(string name)
        {
            if (organizations == null || !organizations.Any()) return defaultIdName;
            return organizations.FirstOrDefault(x => x.name == name)?.id ?? defaultIdName;
        }
        public void SetDefaultIdName(string name)
        {
            defaultIdName = name;
        }
    }

    public class BitwardenOrganization
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
