// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace DevToolsCmdPal;

public partial class DevToolsCmdPalCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public DevToolsCmdPalCommandsProvider()
    {
        DisplayName = "開発ツールdesuyo";
        Icon = Icons.Logo;
        _commands = [
            new CommandItem(new DevToolsCmdPalPage()) { Title = DisplayName, Subtitle = "Base64やハッシュ化などの簡単な変換を提供します" },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
