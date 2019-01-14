using System;
using System.Linq;

namespace TeamsidToBitwardenConverter.Model.Bitwarden
{
    public class BitwardenFolderDefinition
    {
        public BitwardenFolder[] folders { get; set; }

        private string defaultIdName = null;
        public string GetId(string name)
        {
            if (folders == null || !folders.Any()) return defaultIdName;
            return folders.FirstOrDefault(x => x.name == name)?.id ?? defaultIdName;
        }
        public void SetDefaultIdName(string name)
        {
            defaultIdName = name;
        }
    }
    public class BitwardenFolder
    {
        public string id { get; set; }
        public string name { get; set; }
    }

}
