using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Box2D.NetStandard.Dynamics.Bodies;
using Hexa.NET.ImGui;
using RLShooter.App.Editor.ImGuiIntegration;
using RLShooter.Common.Mathematics;
using RLShooter.Gameplay.Components;
using RLShooter.Gameplay.Systems;
using RLShooter.GameScene;
using RLShooter.Utils;

namespace RLShooter.App.Editor.Windows;

public class HierarchyEditorWindow : EditorWindow {
    private bool   windowHovered = false;
    private string searchString  = string.Empty;

    private readonly List<bool> isLastInLevel = [];

    private enum HierarchyLevelColoring {
        Mono,
        Color,
        Multi
    }

    private HierarchyLevelColoring coloring = HierarchyLevelColoring.Color;

    private UnsafeList<char> labelBuffer    = new();
    private UnsafeList<byte> labelOutBuffer = new();

    protected override void OnPreDraw() {
        base.OnPreDraw();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        WindowFlags = ImGuiWindowFlags.NoCollapse;

        ImGui.SetNextWindowSizeConstraints(new Vector2(400, 400), new Vector2(GetScreenWidth(), GetScreenHeight()));
    }

    protected override void OnPostDraw() {
        base.OnPostDraw();
        ImGui.PopStyleVar(1);
    }

    private QueryDescription _entityListDescription = new QueryDescription()
       .WithAll<Named, EditorFlags>();

    protected override void OnDraw() {
        var scene = Scene.Current;

        ImGui.BeginGroup();
        ImGui.InputTextWithHint("##SearchBar", $"{UwU.MagnifyingGlass} Search...", ref searchString, 1024);
        // ImGui.SameLine();

        /*if (ImGui.BeginPopupContextItem(ImGuiPopupFlags.MouseButtonLeft)) {

            var instantiatableTypes = AssetRegistry.GetInstantiatableGameObjectTypes();
            foreach (var type in instantiatableTypes) {
                if (ImGui.MenuItem(type.Name)) {
                    scene.CreateGameObject(type, type.Name);
                }
            }

            ImGui.EndPopup();

        }*/

        ImGui.PushStyleColor(ImGuiCol.ChildBg, 0xff1c1c1c);

        ImGui.BeginChild("LayoutContent");

        windowHovered = ImGui.IsWindowHovered();

        var avail    = ImGui.GetContentRegionAvail();
        var drawList = ImGui.GetWindowDrawList();

        ImGui.BeginTable("Table", 1, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.PreciseWidths);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

        ImGui.PushStyleColor(ImGuiCol.TableRowBg, 0xff1c1c1c);
        ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, 0xff232323);

        var table = ImGuiP.GetCurrentTable();

        ImGui.Indent();
        ImGui.TableHeadersRow();
        ImGui.Unindent();

        /*if (ImGui.BeginDragDropTarget()) {
            unsafe {
                var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                if (!payload.IsNull) {
                    var id    = *(Guid*) payload.Data;
                    var child = scene.FindByGuid(id);
                    if (child != null) {
                        Console.WriteLine($"Accepted {child.Name}");
                        // scene.AddChild(child);
                    }
                }
            }

            ImGui.EndDragDropTarget();
        }*/

        /*foreach (var element in scene.GameObjects) {
            if (element.Parent != null)
                continue;
            var isLast = element == scene.GameObjects[^1];
            DisplayNode(element, true, !element.Name.Contains(searchString), drawList, table, avail, 0, isLast);
        }*/

        var entities = new List<Entity>();
        scene.World.GetEntities(_entityListDescription, entities);

        for (var i = 0; i < entities.Count; i++) {
            var entity = entities[i];
            var name   = entity.GetEntityName();

            var isLast = i == entities.Count - 1;

            using var _ = new ImguiIdScope(i.ToString());

            DisplayNode(
                ref entity,
                false,
                !name.Contains(searchString),
                drawList,
                table, avail, 0, isLast
            );
        }

        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.EndTable();

        var space = ImGui.GetContentRegionAvail();
        ImGui.Dummy(space);

        ImGui.EndChild();

        ImGui.PopStyleColor();

        ImGui.EndGroup();

    }

    private static Dictionary<int, bool> _expanded = new();

    private void DisplayNode(
        ref Entity    element,
        bool          isRoot,
        bool          searchHidden,
        ImDrawListPtr drawList,
        ImGuiTablePtr table,
        Vector2       avail,
        int           level,
        bool          isLast
    ) {
        SetLevel(level, isLast);

        ref var state = ref element.Get<EditorFlags>();

        // if (!element.Active) {
        //     ImGui.BeginDisabled();
        // }

        var colHovered  = ImGui.GetColorU32(ImGuiCol.HeaderHovered);
        var colSelected = ImGui.GetColorU32(ImGuiCol.Header);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow;

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        var rect = ImGuiP.TableGetCellBgRect(table, 0);
        rect.Max.X =  avail.X + rect.Min.X;
        rect.Max.Y += ImGui.GetTextLineHeight();

        var hovered = ImGui.IsMouseHoveringRect(rect.Min, rect.Max) && windowHovered;
        if (hovered) {
            drawList.AddRectFilled(rect.Min, rect.Max, colHovered);
        }

        if (element.HasRelationship<ParentOf>()) {
            flags |= ImGuiTreeNodeFlags.Leaf;
        }

        if (isRoot) {
            flags = ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.DefaultOpen;
        }

        if (level > 0) {
            if (!string.IsNullOrEmpty(searchString)) {
                var before = coloring;
                coloring = HierarchyLevelColoring.Mono;
                DrawTreeLine(drawList, rect, level, isLast);
                coloring = before;
            } else {
                DrawTreeLine(drawList, rect, level, isLast);
            }
        }

        if (state.IsSelectedInEditor) {
            drawList.AddRectFilled(rect.Min, rect.Max, colSelected);

            var lineMin = rect.Min;
            var lineMax = new Vector2(lineMin.X + 4, rect.Max.Y);
            drawList.AddRectFilled(lineMin, lineMax, levelColor);
        }

        var colorText = !searchHidden && !string.IsNullOrEmpty(searchString);

        if (colorText) {
            ImGui.PushStyleColor(ImGuiCol.Text, 0xff0099ff);
        }

        uint col = 0x0;
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, col);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, col);

        var icon = GetIcon(ref element);

        var isOpen = ImGui.TreeNodeEx($"{icon} {element.GetEntityName()}", flags);
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();

        if (colorText) {
            ImGui.PopStyleColor();
        }

        state.OpenInEditor      = isOpen;
        state.DisplayedInEditor = true;

        HandleInput(ref element, hovered);
        DisplayNodeContextMenu(ref element);
        HandleDragDrop(ref element);
        DrawObjectLabels(avail, ref element);

        if (isOpen) {
            if (element.HasRelationship<ParentOf>()) {
                ref var parentOfRelation = ref element.GetRelationships<ParentOf>();

                var idx = 0;
                foreach (var child in parentOfRelation) {
                    var childEntity = child.Key;
                    var relation    = child.Value;
                    var name        = childEntity.Get<Named>().Name;

                    DisplayNode(
                        ref childEntity,
                        false,
                        !name.Contains(searchString!),
                        drawList, table, avail, level + 1,
                        false
                        // isLastElement
                    );
                    idx++;
                }
            }

            ImGui.TreePop();
        } else {
            if (element.HasRelationship<ParentOf>()) {
                ref var parentOfRelation = ref element.GetRelationships<ParentOf>();
                foreach (var child in parentOfRelation) {
                    var childEntity = child.Key;

                    ref var childState = ref childEntity.Get<EditorFlags>();
                    childState.DisplayedInEditor = false;
                }
            }
        }

        // if (!element.Active) {
        // ImGui.EndDisabled();
        // }

    }

    private void HandleInput(ref Entity element, bool hovered) {
        var isSelected = element.IsSelectedInEditor();
        
        if (ImGuiP.IsMouseClicked(ImGuiMouseButton.Left)) {
            if (hovered) {
                if (ImGui.GetIO().KeyCtrl) {
                    SelectionCollection.Global.AddSelection(element.Reference());
                } else if (ImGui.GetIO().KeyShift) {
                    if (SelectionCollection.Global.TryGetLast<EntityReference>(out var last)) {
                        SelectionCollection.Global.AddMultipleSelection(Scene.Current.GetRange(last, element));
                    }
                } else if (!isSelected) {
                    SelectionCollection.Global.AddOverwriteSelection(element.Reference());
                }
            }
        }

        if (hovered && ImGuiP.IsMouseClicked(ImGuiMouseButton.Right)) {
            ImGui.OpenPopup(element.GetEntityName());
            if (!isSelected && !ImGui.GetIO().KeyCtrl) {
                SelectionCollection.Global.AddSelection(element.Reference());
            }
        }

        if (Focused) {
            // if (element.IsSelectedInEditor && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && EditorCameraController.Center != element.Transform.GlobalPosition) {
            //     EditorCameraController.Center = element.Transform.GlobalPosition;
            // } else if (element.IsSelectedInEditor && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && EditorCameraController.Center == element.Transform.GlobalPosition) {
            //     EditorCameraController.Center = Vector3.Zero;
            // }

            // if (element.IsSelectedInEditor && ImGuiP.IsKeyReleased(ImGuiKey.Delete)) {
            //     foreach (var o in SelectionCollection.Global) {
            //         if (o is GameObject go) {
            //             go.Destroy();
            //         }
            //     }
            //
            //     SelectionCollection.Global.ClearSelection();
            // }
            //
            // if (element.IsSelectedInEditor && ImGuiP.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGuiP.IsKeyReleased(ImGuiKey.U)) {
            //     SelectionCollection.Global.ClearSelection();
            // }
        }
    }

    private static void DisplayNodeContextMenu(ref Entity element) {
        /*if (ImGui.BeginPopupContextItem(element.DebugName, ImGuiPopupFlags.MouseButtonRight)) {
            if (ImGui.MenuItem($"{UwU.MagnifyingGlassPlus} Focus")) {
                // EngineTickContext.EditorCamera.Target = element.Transform.GlobalPosition;
                // EngineTickContext.EditorCamera.Transform.Recalculate(true);
            }

            if (ImGui.MenuItem($"{UwU.MagnifyingGlassMinus} Defocus")) {
                // EngineTickContext.EditorCamera.Target = Vector3.Zero;
                // EngineTickContext.EditorCamera.Transform.Recalculate(true);
            }

            ImGui.Separator();

            if (ImGui.MenuItem($"{UwU.Minus} Unselect")) {
                SelectionCollection.Global.ClearSelection();
            }

            /*ImGui.Separator();

            if (ImGui.MenuItem($"{UwU.Clone} Clone")) {
                foreach (var item in SelectionCollection.Global) {
                    if (item is GameObject gameObject) {
                        element.Scene.AddChild(Instantiator.Instantiate(gameObject));
                    }
                }
            }#1#

            ImGui.Separator();

            if (ImGui.MenuItem($"{UwU.Trash} Delete")) {
                foreach (var o in SelectionCollection.Global) {
                    if (o is GameObject go) {
                        go.Destroy();
                    }
                }

                SelectionCollection.Global.ClearSelection();
            }

            ImGui.EndPopup();
        }*/
    }

    private static void HandleDragDrop(ref Entity element) {
        /*if (ImGui.BeginDragDropTarget()) {
            unsafe {
                var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                if (!payload.IsNull) {
                    //Guid id = *(Guid*)payload.Data;
                    //var gameObject = SceneManager.Current.FindByGuid(id);
                    foreach (var o in SelectionCollection.Global) {
                        if (o is GameObject go) {
                            element.AddChild(go);
                        }
                    }
                }
            }

            ImGui.EndDragDropTarget();
        }

        if (ImGui.BeginDragDropSource()) {
            unsafe {
                var id = element.AssetId;
                ImGui.SetDragDropPayload(nameof(GameObject), &id, (uint) sizeof(Guid));
            }

            ImGui.Text(element.Name);
            ImGui.EndDragDropSource();
        }*/
    }

    private unsafe void DrawObjectLabels(Vector2 avail, ref Entity element) {
        labelBuffer.Clear();
        labelOutBuffer.Clear();

        if (element.Has<SpriteRenderable>()) {
            labelBuffer.PushBack(UwU.DrawPolygon);
        }

        if (element.Has<Body>()) {
            labelBuffer.PushBack(UwU.Shapes);
        }

        ToUTF8(ref labelBuffer, ref labelOutBuffer);

        var padding = ImGui.CalcTextSize($"{UwU.Eye}").X + 7;

        var width = ImGui.CalcTextSize(labelOutBuffer.Data).X + padding;
        ImGui.SameLine();
        ImGui.SetCursorPosX(avail.X - width);
        ImGui.Text(labelOutBuffer.Data);

        // ImGui.SameLine();
        // var visible = element.Active;
        // if (visible && ImGui.MenuItem($"{UwU.Eye}")) {
        //     element.Active = false;
        // }

        // if (!visible && ImGui.MenuItem($"{UwU.EyeSlash}")) {
        //     element.Active = true;
        // }
    }

    private static unsafe void ToUTF8(ref UnsafeList<char> str, ref UnsafeList<byte> pOutStr) {
        var byteSize = Encoding.UTF8.GetByteCount(str.Data, (int) str.Size);
        pOutStr.Reserve(byteSize + 1);
        Encoding.UTF8.GetBytes(str.Data, (int) str.Size, pOutStr.Data, byteSize);
        pOutStr.Resize(byteSize);
        pOutStr[pOutStr.Size] = (byte) '\0';
    }

    private static char GetIcon(ref Entity element) {
        var icon = UwU.Cube;

        // if (element is Light) icon = UwU.Lightbulb;
        // if (element is Camera or EditorCamera) icon = UwU.Camera;

        return icon;
    }
    private void SetLevel(int level, bool isLast) {
        if (isLastInLevel.Count <= level) {
            isLastInLevel.Add(isLast);
        } else {
            isLastInLevel[level] = isLast;
        }
    }
    private void DrawTreeLine(ImDrawListPtr drawList, ImRect rect, int level, bool isLast, bool isLevelLower = false) {
        for (var i = 1; i < level; i++) {
            var lowerLevel = level - i;
            if (isLastInLevel[lowerLevel]) {
                continue;
            }

            DrawTreeLine(drawList, rect, lowerLevel, false, true);
        }

        const float lineThickness = 2;
        const float lineWidth     = 10;
        var         indentSpacing = ImGui.GetStyle().IndentSpacing * (level - 1) + ImGui.GetTreeNodeToLabelSpacing() * 0.5f - lineThickness * 0.5f;
        Vector2     lineMin       = new(rect.Min.X + indentSpacing, rect.Min.Y);
        Vector2     lineMax       = new(lineMin.X + lineThickness, rect.Max.Y);
        var         lineMidpoint  = lineMin + (lineMax - lineMin) * 0.5f;
        Vector2     lineTMin      = new(lineMax.X, lineMidpoint.Y - lineThickness * 0.5f);
        Vector2     lineTMax      = new(lineMax.X + lineWidth, lineMidpoint.Y + lineThickness * 0.5f);
        if (isLast) {
            lineMax.Y = lineTMax.Y; // set vertical line y to horizontal line y to create a L shape
        }

        var color = GetColorForLevel(level);
        drawList.AddRectFilled(lineMin, lineMax, color);
        if (!isLevelLower) {
            drawList.AddRectFilled(lineTMin, lineTMax, color);
        }
    }

    private uint levelColor           = 0xffcf7334;
    private byte levelAlpha           = 0xFF;
    private byte monochromeBrightness = 0xac;
    private bool reverseColoring      = false;

    private readonly uint[] levelColorPalette = [ // 0xAABBGGRR
        0x8F0000FF,
        0x8F00FF00,
        0x8FFF0000,
        0x8FFFFF00,
        0x8FFF00FF,
        0x8F00FFFF,
        0x8F800080,
        0x8F008080,
    ];

    private uint GetColorForLevel(int level) {
        var levelNormalized = (level - 1) % levelColorPalette.Length;

        if (reverseColoring) {
            levelNormalized = levelColorPalette.Length - levelNormalized - 1;
        }

        if (coloring == HierarchyLevelColoring.Mono) {
            var brightness = (uint) (monochromeBrightness * (1 - (levelNormalized / (float) levelColorPalette.Length)));

            return (uint) levelAlpha << 24 | brightness << 16 | brightness << 8 | brightness; // 0xAABBGGRR
        }

        if (coloring == HierarchyLevelColoring.Color) {
            var value    = levelNormalized / (float) levelColorPalette.Length;
            var hueShift = value * 0.1f;

            var hsv = Common.Mathematics.Color.FromABGR(levelColor).ToHSVA();

            hsv.H += hueShift;
            hsv.S *= 1 - value;
            hsv.V /= MathF.Exp(value);

            var color = hsv.ToRGBA().ToUIntABGR();

            return (uint) levelAlpha << 24 | color; // 0xAABBGGRR
        }

        // HierarchyLevelColoring.Multi just return here as fallback.

        return levelColorPalette[levelNormalized];
    }

    public override void Shutdown() { }
}