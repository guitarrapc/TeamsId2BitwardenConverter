using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TeamsidToBitwardenConverter.Model.Bitwarden;
using TeamsidToBitwardenConverter.Model.TeamsId;

namespace TeamsidToBitwardenConverter
{
    public class BitwardenConverter
    {
        private BitwardenFolderDefinition folderDefinition;

        private string organizationId;
        private BitwardenCollectionDefinition collectionDefinition;

        public BitwardenConverter(BitwardenFolderDefinition folderDefinition)
        {
            this.folderDefinition = folderDefinition;
        }
        public BitwardenConverter(string organizationId, BitwardenCollectionDefinition collectionDefinition)
        {
            this.organizationId = organizationId;
            this.collectionDefinition = collectionDefinition;
        }
        private class FieldRecord
        {
            public string Url { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public (string key, string value)[] SecureFields { get; set; }
            public (string key, string value)[] Fields { get; set; }
            public string Group { get; set; }
            public string Collection { get; set; }
        }

        public BitwardenItem[] Convert(ITeamsIdDefinition[] from, string defaultGroup = null, string defaultCollection = null)
        {
            var records = from.Select(record =>
            {
                // reflection to check fieldXX and get valueXX
                var t = record.GetType();
                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var description = GetPropertyValue(record, t, "description");
                var note = GetPropertyValue(record, t, "note");
                var fieldRecord = ParseFieldRecord(props, t, record);

                // group fallback
                if (string.IsNullOrWhiteSpace(organizationId) && defaultGroup != null && string.IsNullOrWhiteSpace(fieldRecord.Group))
                {
                    fieldRecord.Group = defaultGroup;
                }

                // collection fallback
                if (!string.IsNullOrWhiteSpace(organizationId) && defaultCollection != null && string.IsNullOrWhiteSpace(fieldRecord.Collection))
                {
                    fieldRecord.Group = null;
                    fieldRecord.Collection = defaultCollection;
                }

                // generate typed item
                var userEmpty = string.IsNullOrWhiteSpace(fieldRecord.UserName);
                var emailEmpty = string.IsNullOrWhiteSpace(fieldRecord.Email);
                var userName = !userEmpty ? fieldRecord.UserName : fieldRecord.Email;
                var bitwarden = new BitwardenItem
                {
                    id = Guid.NewGuid().ToString(),
                    organizationId = string.IsNullOrWhiteSpace(organizationId) ? null : organizationId,
                    collectionIds = collectionDefinition?.GetIds(fieldRecord.Collection),
                    type = 1,
                    name = description,
                    login = new BitwardenLogin
                    {
                        username = !string.IsNullOrWhiteSpace(fieldRecord.UserName) 
                            ? fieldRecord.UserName
                            : fieldRecord.Email,
                        password = fieldRecord.Password,
                        uris = new[] {
                        new BitwardenUri
                        {
                            uri = fieldRecord.Url,
                            match = null,
                        }
                    }
                    },
                    favorite = false,
                    notes = note,
                    folderId = folderDefinition?.GetId(fieldRecord.Group),
                };
                // initialize fields
                var customField = fieldRecord.Fields == null
                    ? Array.Empty<BitwardenField>()
                    : fieldRecord.Fields.Select(x => new BitwardenField { name = x.key, value = x.value, type = 0 });
                var customSecretField = fieldRecord.SecureFields == null
                    ? Array.Empty<BitwardenField>()
                    : fieldRecord.SecureFields.Select(x => new BitwardenField { name = x.key, value = x.value, type = 1 });
                var fields = customField.Concat(customSecretField);
                if (!userEmpty && !emailEmpty)
                {
                    fields = customField.Concat(new[] { new BitwardenField { name = "email", value = fieldRecord.Email, type = 0 } });
                }
                if (fields.Any())
                {
                    bitwarden.fields = fields.ToArray();
                }
                return bitwarden;
            })
            .ToArray();
            return records;
        }

        private FieldRecord ParseFieldRecord(PropertyInfo[] props, Type t, ITeamsIdDefinition source)
        {
            var fieldRecord = new FieldRecord();
            // get FieldXX properties via reflection
            var fieldRecords = props
                .Where(x => Match(x.Name, @"Field\d+"))
                .Where(x => GetPropertyValue(source, t, x.Name) != "")
                .Select(x => (key: GetPropertyValue(source, t, x.Name), value: GetPropertyValue(source, t, x.Name.Replace("Field", "Value"))))
                .ToArray();

            // get field's value and categolize each via field name regex pattern
            var secureMemoList = new List<(string, string)>();
            var memoList = new List<(string, string)>();
            foreach (var record in fieldRecords)
            {
                switch (record.key)
                {
                    case var _ when Match(record.key, "url") && fieldRecord.Url == null:
                        fieldRecord.Url = record.value;
                        break;
                    case var _ when Match(record.key, "email|e-mail|mail") && fieldRecord.Email == null:
                        fieldRecord.Email = record.value;
                        break;
                    case var _ when Match(record.key, "username|id") && fieldRecord.UserName == null:
                        fieldRecord.UserName = record.value;
                        break;
                    case var _ when Match(record.key, "password|pass") && fieldRecord.Password == null:
                        fieldRecord.Password = record.value;
                        break;
                    case var _ when Match(record.key, "access|secret|pin|token|秘密|暗号"):
                        secureMemoList.Add((record.key, record.value));
                        break;
                    case var _ when Match(record.key, "group"):
                        fieldRecord.Group = record.value;
                        break;
                    default:
                        memoList.Add((record.key, record.value));
                        break;
                }
            }
            fieldRecord.Fields = memoList.ToArray();
            fieldRecord.SecureFields = secureMemoList.ToArray();
            return fieldRecord;
        }

        private bool Match(string text, string pattern)
        {
            return Regex.IsMatch(text, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        private string GetPropertyValue(ITeamsIdDefinition record, Type t, string propertyField)
        {
            return (string)t.GetProperty(propertyField).GetValue(record);
        }
    }
}
