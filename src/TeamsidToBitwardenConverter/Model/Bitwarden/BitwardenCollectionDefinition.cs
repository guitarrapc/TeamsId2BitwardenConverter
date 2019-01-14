using System;
using System.Linq;

namespace TeamsidToBitwardenConverter.Model.Bitwarden
{
    public class BitwardenCollectionDefinition
    {
        public BitwardenCollection[] collections { get; set; }

        private string[] defaultIdNames = null;
        public string[] GetIds(string name)
        {
            if (collections == null || !collections.Any()) return defaultIdNames;
            return collections?.Where(x => x.name == name).Select(x => x.id).ToArray() ?? defaultIdNames;
        }
        public void SetDefaultIdName(string[] names)
        {
            defaultIdNames = names;
        }
    }

    public class BitwardenCollection
    {
        public string id { get; set; }
        public string organizationId { get; set; }
        public string name { get; set; }
    }
}
