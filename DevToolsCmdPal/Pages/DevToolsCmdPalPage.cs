// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Security.Cryptography;

namespace DevToolsCmdPal;

internal sealed partial class DevToolsCmdPalPage : DynamicListPage
{
    private List<IListItem> _items;
    private ListItem[] EmptyListItem;
    private readonly Lock _resultsLock = new();

    public DevToolsCmdPalPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "開発ツール";
        Name = "Open";
        PlaceholderText = "Type To Create Base64 or Compute Hash";
        _items = [];

        EmptyContent = new CommandItem(new NoOpCommand()
        {
            Name = "文字列を入力して機能を表示..."
        });

        UpdateGuid();
    }

    private void UpdateGuid()
    {
        EmptyListItem = [new ListItem(new NoOpCommand()
        {
            Name = "文字列を入力してBase64とハッシュ化機能を利用..."
        }),
        new ListItem(new AnonymousCommand(() =>ClipboardHelper.SetText( Guid.NewGuid().ToString())){
            Name = "クリップボードにランダムなGUIDをコピー",
            Result = CommandResult.ShowToast("GUIDをコピーしました"),
            Icon = Icons.CopyIcon
        }),
        new ListItem(new AnonymousCommand(() =>ClipboardHelper.SetText( Guid.CreateVersion7().ToString())){
            Name = "クリップボードにランダムなGUIDv7をコピー",
            Result = CommandResult.ShowToast("GUIDv7をコピーしました"),
            Icon = Icons.CopyIcon
        })];
    }

    private void UpdateItems(string searchText)
    {
        lock (_resultsLock)
        {
            _items.Clear();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                UpdateGuid();
                _items.AddRange(EmptyListItem);
            } else
            {
                _items.AddRange(hasContentItems(searchText));
            }
        }
        RaiseItemsChanged(_items.Count);
    }

    List<ListItem> hasContentItems(string search) =>
        [
        new ListItem(new CopyTextCommand(EncodeBase64(search)){
            Name = $"'{search}' をBase64エンコードしてコピー"
        }),
        TryDecodeBase64(search, out var decoded) ? new ListItem(new CopyTextCommand(decoded){
            Name = $"'{search}' をBase64デコードしてコピー"
        }) : new ListItem(new NoOpCommand(){
            Name = "Base64デコードできません"
        }),
        new ListItem(new CopyTextCommand(ComputeSha256Hash(search)){
            Name = $"'{search}' のSHA256ハッシュをコピー"
        }),
        new ListItem(new CopyTextCommand(ComputeSha1Hash(search)){
            Name = $"'{search}' のSHA1ハッシュをコピー"
        }),
        new ListItem(new CopyTextCommand(ComputeMd5Hash(search)){
            Name = $"'{search}' のMD5ハッシュをコピー"
        })
    ];

    private string EncodeBase64(string input)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(data);
    }

    private bool TryDecodeBase64(string input, out string decoded)
    {
        try
        {
            byte[] data = Convert.FromBase64String(input);
            decoded = System.Text.Encoding.UTF8.GetString(data);
            return true;
        } catch
        {
            decoded = string.Empty;
            return false;
        }
    }

    private static string ComputeSha256Hash(string input)
    {
        byte[] data = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(data);
    }

    private static string ComputeSha1Hash(string input)
    {
        byte[] data = SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(data);
    }

    private static string ComputeMd5Hash(string input)
    {
        byte[] data = MD5.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(data);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        UpdateGuid(); // 毎回GUIDを更新して新しいものを表示する
        if (oldSearch == newSearch)
        {
            return;
        }

        UpdateItems(newSearch);

    }

    public override IListItem[] GetItems()
    {
        return _items.ToArray();
    }
}
