using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Gui.ImGuiTable;
using HaselCommon.Services;
using HaselDebug.Sheets;
using ImGuiNET;

namespace HaselDebug.Tabs.UnlocksTabs.Outfits.Columns;

[RegisterSingleton, AutoConstruct]
public partial class SetColumn : ColumnString<CustomMirageStoreSetItem>
{
    private const float IconSize = OutfitsTable.IconSize;

    private readonly TextService _textService;
    private readonly TextureService _textureService;
    private readonly ITextureProvider _textureProvider;
    private readonly ImGuiContextMenuService _imGuiContextMenuService;

    [AutoPostConstruct]
    public void Initialize()
    {
        SetFixedWidth(300);
        Flags |= ImGuiTableColumnFlags.DefaultSort;
    }

    public override string ToName(CustomMirageStoreSetItem row)
        => _textService.GetItemName(row.RowId).ExtractText().StripSoftHyphen();

    public override unsafe void DrawColumn(CustomMirageStoreSetItem row)
    {
        var isSetCollected = ItemFinderModule.Instance()->GlamourDresserItemIds.Contains(row.RowId);

        ImGui.BeginGroup();
        ImGui.Dummy(ImGuiHelpers.ScaledVector2(IconSize));
        ImGui.SameLine(0, 0);
        ImGuiUtils.PushCursorX(-IconSize * ImGuiHelpers.GlobalScale);
        _textureService.DrawIcon(
            row.Set.Value.Icon,
            false,
            new(IconSize * ImGuiHelpers.GlobalScale)
            {
                TintColor = isSetCollected
                    ? Color.White
                    : ImGui.IsItemHovered() || ImGui.IsPopupOpen($"###Set_{row.RowId}_Icon_ItemContextMenu")
                        ? Color.White : Color.Grey3
            }
        );

        if (ImGui.IsItemHovered())
        {
            using var tooltip = ImRaii.Tooltip();
            if (_textureProvider.TryGetFromGameIcon(new(row.Set.Value.Icon), out var texture) && texture.TryGetWrap(out var textureWrap, out _))
            {
                ImGui.Image(textureWrap.ImGuiHandle, new(textureWrap.Width, textureWrap.Height));
                ImGui.SameLine();
                ImGuiUtils.PushCursorY(textureWrap.Height / 2f - ImGui.GetTextLineHeight() / 2f);
            }
            ImGui.TextUnformatted(ToName(row));
        }

        if (isSetCollected)
            OutfitsTable.DrawCollectedCheckmark(_textureProvider);

        ImGui.SameLine();
        ImGui.Selectable($"###SetName_{row.RowId}", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, IconSize * ImGuiHelpers.GlobalScale));

        ImGui.EndGroup();

        // TODO: preview whole set??
        _imGuiContextMenuService.Draw($"###Set_{row.RowId}_ItemContextMenu", builder =>
        {
            builder.AddTryOn(row.Set.RowId);
            builder.AddItemFinder(row.Set.RowId);
            builder.AddCopyItemName(row.Set.RowId);
            builder.AddItemSearch(row.Set.RowId);
            builder.AddOpenOnGarlandTools("item", row.Set.RowId);
        });

        ImGui.SameLine(IconSize * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.X, 0);
        ImGuiUtils.PushCursorY(IconSize * ImGuiHelpers.GlobalScale / 2f - ImGui.GetTextLineHeight() / 2f);
        ImGui.TextUnformatted(_textService.GetItemName(row.RowId).ExtractText().StripSoftHyphen());
    }
}
