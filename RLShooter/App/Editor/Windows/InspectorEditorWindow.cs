using Hexa.NET.ImGui;
using JetBrains.Annotations;
using RLShooter.App.Editor.PropertyEditor;
using RLShooter.Config;
using RLShooter.GameScene;

namespace RLShooter.App.Editor.Windows;

[UsedImplicitly]
public class InspectorEditorWindow : EditorWindow {

    private GameObject _selectedGameObject = null;

    /*private Action<ObjectProperties, ObjectMember> GetDrawer(Type type) {
        if (TypeDrawers.TryGetValue(type, out var drawer)) {
            return drawer;
        }

        /#1#/ handle enum types
        if (type.IsEnum) {
            return (properties, member) => {
                var val = member.GetValue();
                if (ImGui.BeginCombo($"##{member.Name}", val.ToString())) {
                    foreach (var enumValue in Enum.GetValues(type)) {
                        var enumVal = (int) enumValue;
                        if (ImGui.Selectable(enumValue.ToString(), enumVal == (int) val)) {
                            member.SetValue(enumVal);
                        }
                    }

                    ImGui.EndCombo();
                }
            };
        }

        // Handle lists/arrays
        if (type.IsArray || type.GetInterface("IEnumerable") != null) {
            return (properties, member) => {
                var val = member.GetValue();
                if (val == null) {
                    ImGui.Text("null");
                    return;
                }

                var innerType = val.GetType().GetElementType()
                             ?? val.GetType().GetGenericArguments().FirstOrDefault();

                var label = $"{innerType?.Name}[{((IEnumerable) val).Cast<object>().Count()}]";
                if (ImGui.TreeNode(label)) {
                    ImGui.Unindent();

                    var array = (IEnumerable) val;
                    var i     = 0;
                    foreach (var item in array) {
                        ImGui.PushID(i);

                        if (ImGui.TreeNode($"Element {i}")) {

                            var objProperties = new ObjectProperties(item.GetType(), item);
                            DrawObjectProperties(objProperties);

                            ImGui.TreePop();
                        }


                        ImGui.PopID();
                        i++;
                    }
                    ImGui.Indent();
                    ImGui.TreePop();

                }


            };
        }#1#

        // if subclass of `EntityObject` then draw all properties
        if (typeof(GameObject).IsAssignableFrom(type)) {
            return (properties, member) => {
                var obj = member.GetValue<GameObject>();
                if (obj == null) {
                    ImGui.Text("null");
                    return;
                }

                // add imgui indentation
                ImGui.Indent();

                var objProperties = new ObjectProperties(obj.GetType(), obj);
                DrawObjectProperties(objProperties);

                ImGui.Unindent();
            };
        }

        // Check if it can be converted to a string
        if (type.GetMethod("ToString", Type.EmptyTypes) != null) {
            return (properties, member) => {
                var val = member.GetValue();
                ImGui.Text(val.ToString());
                ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), "Unsupported type: " + type);

            };
        }

        return (properties, member) => ImGui.Text("Unsupported type: " + type);
    }*/

    private void DrawObjectPropertiesContainer(object container) {
        var type = container.GetType();

        if (ImGui.CollapsingHeader(type.Name, ImGuiTreeNodeFlags.DefaultOpen)) {
            DrawObjectProperties(container);
        }
    }

    private void DrawObjectProperties(object container) {

        var propCache = PropertyEditorCache.Get(container.GetType());
        if (propCache != null) {
            propCache.Render(container);
        }

        // objectProperties.RootCategory.Render();


        /*foreach (var (category, properties) in fields) {
            var drawProperties = true;
            if (!string.IsNullOrEmpty(category)) {
                drawProperties = ImGui.CollapsingHeader(category);
            }

            if (drawProperties) {
                if (ImGui.BeginTable("##properties", 2, TableFlags)) {
                    ImGui.TableSetupColumn("##propertyname");
                    ImGui.TableSetupColumn("##propertyvalue");


                    foreach (var member in properties) {
                        if (!member.CanDrawInInspector)
                            continue;
                        // if advanced is true, we only want to draw the advanced properties, otherwise exclude them
                        if (!advanced && member.AdvancedSection) {
                            advancedCount++;
                            continue;
                        }

                        if (advanced && !member.AdvancedSection) {
                            continue;
                        }


                        var disabled = member.Disabled;

                        ImGui.TableNextRow();
                        ImGui.PushID(member.Name);
                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();
                        ImGui.Dummy(new Vector2(5, 0)); // Manual horizontal padding
                        ImGui.SameLine();
                        if (disabled) {
                            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                        }

                        ImGui.TextUnformatted(member.Name);
                        if (disabled) {
                            ImGui.PopStyleVar();
                        }

                        ImGui.TableNextColumn();

                        if (disabled) {
                            ImGui.BeginDisabled();
                        }

                        if (member.ShowNameOnly) {
                            ImGui.TextUnformatted(member.GetValue().ToString());
                        } else {
                            var drawer = GetDrawer(member.Type);

                            ImGui.PushItemWidth(-1);
                            drawer(objectProperties, member);
                        }

                        if (disabled) {
                            ImGui.EndDisabled();
                        }

                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }
            }

        }*/

        /*
        if (ImGui.BeginTable("##properties", 2, TableFlags)) {
            ImGui.TableSetupColumn("##propertyname");
            ImGui.TableSetupColumn("##propertyvalue");


            ImGui.EndTable();
        }
        */


        /*if (!advanced && advancedCount > 0) {
            ImGui.Indent();

            if (ImGui.CollapsingHeader("Advanced")) {
                DrawObjectProperties(objectProperties, true);
            }

            ImGui.Unindent();
        }*/

    }

    public override void Setup(bool open) {
        base.Setup(open);

        SelectionCollection.Global.SelectionChanged += (sender, collection) => {
            var first = collection.First<GameObject>();
            if (collection.Count == 1 && (_selectedGameObject == null || _selectedGameObject != first)) {
                SetSelected(first);
            }
        };

        /*if (
            _selectedGameObject == null &&
            !SelectionCollection.Global.HasAny<GameObject>() &&
            (Application.CurrentScene.Root?.Children.Count ?? 0) > 0
        ) {
            if (AppConfig.LastSelectedObjectId != Guid.Empty) {
                // var prevSelected = Application.CurrentScene.FindByGuid(AppConfig.LastSelectedObjectId);
                // if (prevSelected != null)
                // SetSelected(prevSelected);
            } else {
                SetSelected(Application.CurrentScene.Root.Children[0]);
            }

            return;
        }*/

        if (SelectionCollection.Global.HasAny<GameObject>()) {
            var first = SelectionCollection.Global.First<GameObject>();
            if (_selectedGameObject == null || _selectedGameObject != first) {
                SetSelected(first);
            }
        }
    }



    private void SetSelected(GameObject gameObject) {
        _selectedGameObject            = gameObject;
        AppConfig.LastSelectedObjectId = gameObject?.AssetId ?? Guid.Empty;
    }

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

    protected override void OnDraw() {
        if (_selectedGameObject == null) {
            ImGui.Text("No selection");
            return;
        }

        if (ImGui.BeginChild("##inspector_header", new Vector2(0, 40), ImGuiWindowFlags.NoScrollbar)) {
            // ImGui.SetCursorPosX(ImGui.GetWindowWidth() / 2 - ImGui.CalcTextSize(_selectedGameObject.DebugName).X / 2);
            ImGui.SetCursorPosX(5);
            ImGui.SetCursorPosY(ImGui.GetWindowHeight() / 2 - ImGui.CalcTextSize(_selectedGameObject.DebugName).Y / 2);
            ImGui.Text(_selectedGameObject.DebugName);
            ImGui.SameLine();

            var btnText = "Add Component";
            ImGui.SetCursorPosY(8);
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize(btnText).X - ImGui.GetStyle().FramePadding.X * 2 - ImGui.GetStyle().ItemSpacing.X);
            ImGui.Button(btnText);
            if (ImGui.BeginPopupContextItem(ImGuiPopupFlags.MouseButtonLeft)) {

                /*var instantiatableTypes = AssetRegistry.GetInstantiatableComponentTypes();
                foreach (var type in instantiatableTypes) {
                    if (ImGui.MenuItem(type.Name)) {
                        var comp = _selectedGameObject.AddComponent(type);
                    }
                }*/

                ImGui.EndPopup();

            }

            ImGui.EndChild();
        }

        ImGui.Separator();

        if (ImGui.BeginChild("##inspector", ImGuiWindowFlags.AlwaysVerticalScrollbar)) {
            DrawObjectProperties(_selectedGameObject);

            ImGui.Separator();

            foreach (var component in _selectedGameObject.Components) {
                if (component == null)
                    continue;

                DrawObjectPropertiesContainer(component);
            }

            ImGui.EndChild();
        }

    }


    public override void Shutdown() { }
}