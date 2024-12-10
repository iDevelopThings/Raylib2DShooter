using Hexa.NET.ImGui;
using RLShooter.App.Editor.ImGuiIntegration;
using RLShooter.App.Editor.Windows.Inspectors;

namespace RLShooter.App.Editor.PropertyEditor;

public class PropertyNode {

    public string Name   { get; }
    public bool   IsRoot { get; }

    public ObjectTypeCache Container { get; set; }

    public Dictionary<string, PropertyNode> Subcategories { get; } = new();
    public List<ObjectMemberCache>          Members       { get; } = new();


    public PropertyNode(
        string          name,
        bool            isRoot,
        ObjectTypeCache container
    ) {
        Name      = name;
        IsRoot    = isRoot;
        Container = container;
    }

    public void AddMember(string[] categoryPath, ObjectMemberCache member, int index = 0) {
        if (index >= categoryPath.Length) {
            Members.Add(member);
        } else {
            var subcategoryName = categoryPath[index];
            if (!Subcategories.TryGetValue(subcategoryName, out var subcategory)) {
                subcategory = new PropertyNode(
                    subcategoryName,
                    false,
                    Container
                );
                Subcategories[subcategoryName] = subcategory;
            }

            subcategory.AddMember(categoryPath, member, index + 1);
        }
    }

    public void Render(object container) {
        var draw = (IsRoot || string.IsNullOrEmpty(Name)) || ImGui.CollapsingHeader(Name);
        if (draw) {
            RenderMembers(container);

            using var _ = new ImguiIndentScope(!IsRoot);

            foreach (var subcategory in Subcategories.Values) {
                subcategory.Render(container);
            }

        }
    }

    private void RenderMembers(object container) {
        // Render members inside a table for better alignment
        if (Members.Count == 0)
            return;

        if (ImGui.BeginTable($"##{Name}_table", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerH)) {
            ImGui.TableSetupColumn("Property Name", ImGuiTableColumnFlags.WidthFixed, 150.0f);
            ImGui.TableSetupColumn("Value");

            foreach (var member in Members) {
                ImGui.TableNextRow();

                using var _ = new ImguiIdScope(member.Name);
                ImGui.TableNextColumn();

                ImGui.AlignTextToFramePadding();
                ImGui.Dummy(new Vector2(5, 0)); // Manual horizontal padding
                ImGui.SameLine();

                {
                    using var __ = new ImguiDisabledScope(member.Disabled);
                    ImGui.TextUnformatted(member.Name);
                }

                ImGui.TableNextColumn();

                {
                    using var __ = new ImguiItemDisabledScope(member.Disabled);

                    ImGui.PushItemWidth(-1);

                    if (member.ShowNameOnly) {
                        ImGui.TextUnformatted(member.GetValue(container).ToString());
                    } else {
                        var drawer = BaseTypeDrawer.GetDrawer(member.Type);
                        if (drawer != null) {
                            drawer.Draw(member, container);
                        } else {

                            // if it's a struct, just try to draw it
                            if (member.Type.IsValueType) {
                                PropertyEditorCache.Get(member.Type).Render(member.GetValue(container));
                            } else {
                                ImGui.TextUnformatted("No drawer for type: \n" + member.Type);
                            }
                        }
                    }

                }

            }

            ImGui.EndTable();
        }
    }
}