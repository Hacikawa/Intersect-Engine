using Intersect.Config;
using Intersect.Logging;
using Intersect.Server.Core.CommandParsing;
using Intersect.Server.Database;
using Intersect.Server.Database.GameData;
using Intersect.Server.Database.Logging;
using Intersect.Server.Database.PlayerData;
using Intersect.Server.Localization;

namespace Intersect.Server.Core.Commands;

internal sealed partial class MigrateCommand : ServerCommand
{
    public MigrateCommand() : base(Strings.Commands.Migrate)
    {
    }

    protected override void HandleValue(ServerContext context, ParserResult result) =>
        DbInterface.HandleMigrationCommand();
}
