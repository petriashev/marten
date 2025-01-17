using System.Threading;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Marten.Internal.CodeGeneration;
using Marten.Schema.Identity;
using NpgsqlTypes;

namespace Marten.Schema.Arguments
{
    /// <summary>
    /// "docVersion" function argument for interaction with "mt_version" column.
    /// </summary>
    public class VersionArgument: UpsertArgument
    {
        public const string ArgName = "docVersion";

        public VersionArgument()
        {
            Arg = ArgName;
            Column = SchemaConstants.VersionColumn;
            DbType = NpgsqlDbType.Uuid;
            PostgresType = "uuid";
        }

        public override void GenerateCodeToModifyDocument(GeneratedMethod method, GeneratedType type, int i, Argument parameters,
            DocumentMapping mapping, StoreOptions options)
        {
            if (mapping.Metadata.Version.Member != null)
            {
                // "_version" would be a field in the StorageOperation base class
                method.Frames.SetMemberValue(mapping.Metadata.Version.Member, "_version", mapping.DocumentType, type);
            }
        }

        public override void GenerateCodeToSetDbParameterValue(GeneratedMethod method, GeneratedType type, int i, Argument parameters,
            DocumentMapping mapping, StoreOptions options)
        {
            method.Frames.Code("setVersionParameter({0}[{1}]);", parameters, i);
        }

        public override void GenerateBulkWriterCode(GeneratedType type, GeneratedMethod load, DocumentMapping mapping)
        {
            if (mapping.Metadata.Version.Member == null)
            {
                load.Frames.Code($"writer.Write({typeof(CombGuidIdGeneration).FullNameInCode()}.NewGuid(), {{0}});", NpgsqlDbType.Uuid);
            }
            else
            {
                load.Frames.Code($@"
var version = {typeof(CombGuidIdGeneration).FullNameInCode()}.NewGuid();
writer.Write(version, {{0}});
", NpgsqlDbType.Uuid);

                load.Frames.SetMemberValue(mapping.Metadata.Version.Member, "version", mapping.DocumentType, type);
            }
        }

        public override void GenerateBulkWriterCodeAsync(GeneratedType type, GeneratedMethod load, DocumentMapping mapping)
        {
            if (mapping.Metadata.Version.Member == null)
            {
                load.Frames.CodeAsync($"await writer.WriteAsync({typeof(CombGuidIdGeneration).FullNameInCode()}.NewGuid(), {{0}}, {{1}});", NpgsqlDbType.Uuid, Use.Type<CancellationToken>());
            }
            else
            {
                load.Frames.Code($@"
var version = {typeof(CombGuidIdGeneration).FullNameInCode()}.NewGuid();
writer.Write(version, {{0}});
", NpgsqlDbType.Uuid);

                load.Frames.SetMemberValue(mapping.Metadata.Version.Member, "version", mapping.DocumentType, type);
            }
        }
    }
}
