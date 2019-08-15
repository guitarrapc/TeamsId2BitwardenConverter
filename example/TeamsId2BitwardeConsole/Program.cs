using System;
using System.IO;
using TeamsidToBitwardenConverter;
using TeamsidToBitwardenConverter.Lib;
using TeamsidToBitwardenConverter.Model.Bitwarden;
using TeamsidToBitwardenConverter.Model.TeamsId;

namespace TeamsId2BitwardeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputPath = $@"./data/bitwarden/import/bitwarden_personal_importdata{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json";
            if (args.Length >= 1 && bool.TryParse(args[0], out var isOrganization))
            {
                var teamsidOrganizationCsv = @"./data/teamsid_export/org_records.csv";
                var bitwardenCollectionDefinitionJson = @"./data/bitwarden/definitions/bitwarden_org_export_collection_definition.json";
                var bitwardenOrganizationDefinitionJson = @"./data/bitwarden/definitions/bitwarden_org_export_organization_definition.json";
                Organization(outputPath, teamsidOrganizationCsv, bitwardenCollectionDefinitionJson, bitwardenOrganizationDefinitionJson);
            }
            else
            {
                var teamsidPersonalCsv = @"./data/teamsid_export/personal_records.csv";
                var bitwardenFolderDefinitionJson = @"./data/bitwarden/definitions/bitwarden_export_folder_definition.json";
                Personal(outputPath, teamsidPersonalCsv, bitwardenFolderDefinitionJson);
            }
        }

        static void Personal(string outputPath, string teamsidPersonalCsv, string bitwardenFolderDefinitionJson)
        {
            var folderDefinition = DeserializeFolderJson<BitwardenFolderDefinition>(bitwardenFolderDefinitionJson);

            // convert teamsid to bitwarden
            var teamsIdDatas = new CsvParser(teamsidPersonalCsv).Parse<TeamsIdDefinition4>();
            var bitwardenItems = new BitwardenConverter(folderDefinition).Convert(teamsIdDatas, defaultGroup: "TestGroup");

            // serialize bitwarden import data
            var importData = new BitwardenDefinition
            {
                folders = folderDefinition.folders,
                items = bitwardenItems,
            };
            importData.WriteJson(outputPath);
        }

        static void Organization(string outputPath, string teamsidOrganizationCsv, string bitwardenCollectionDefinitionJson, string bitwardenOrganizationDefinitionJson)
        {
            var collectionDefinition = DeserializeFolderJson<BitwardenCollectionDefinition>(bitwardenCollectionDefinitionJson);
            var organizationDefinition = DeserializeFolderJson<BitwardenOrganizationDefinition>(bitwardenOrganizationDefinitionJson);

            // convert teamsid to bitwarden
            var teamsIdDatas = new CsvParser(teamsidOrganizationCsv).Parse<TeamsIdDefinition4>();
            var bitwardenItems = new BitwardenConverter(organizationDefinition.GetId("SampleOrg"), collectionDefinition).Convert(teamsIdDatas, defaultCollection: "TestCollection");

            // serialize bitwarden import data
            var importData = new BitwardenDefinition
            {
                collections = collectionDefinition.collections,
                items = bitwardenItems,
            };
            importData.WriteJson(outputPath);
        }

        // deserialize folder definitions
        static T DeserializeFolderJson<T>(string path) where T : class
        {
            using (var stream = File.OpenRead(path))
            {
                var deserialized = Utf8Json.JsonSerializer.Deserialize<T>(stream);
                return deserialized;
            }
        }
    }
}
