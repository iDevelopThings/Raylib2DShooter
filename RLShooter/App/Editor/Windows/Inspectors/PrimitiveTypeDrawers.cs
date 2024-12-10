using System.Collections;
using System.Reflection;
using Hexa.NET.ImGui;
using RLShooter.App.Assets;
using RLShooter.App.Editor.ImGuiIntegration;
using RLShooter.App.Editor.PropertyEditor;
using RLShooter.Common.Utils;
using RLShooter.GameScene;
using RLShooter.Common.Utils.Reflection;

namespace RLShooter.App.Editor.Windows.Inspectors;

public abstract class BaseTypeDrawer {
    public virtual void Draw(ObjectMemberCache member, object container) { }

    public static Dictionary<Type, BaseTypeDrawer> Drawers            = new();
    public static Dictionary<Type, BaseTypeDrawer> DrawerByDrawerType = new();

    private static bool _didLoad = false;

    public static TypeDrawer GetDrawer(Type type) {
        if (!_didLoad) {
            LoadAllDrawers();
            _didLoad = true;
        }

        if (Drawers.TryGetValue(type, out var drawer)) {
            return (TypeDrawer) drawer;
        }

        if (type.IsEnum)
            return (TypeDrawer) Drawers[typeof(Enum)];

        if (type.IsArray || type.GetInterface("IEnumerable") != null)
            return (TypeDrawer) Drawers[typeof(IEnumerable)];

        return null;
    }

    public static T GetDrawerType<T>() where T : BaseTypeDrawer {
        return (T) DrawerByDrawerType[typeof(T)];
    }

    public static void LoadAllDrawers() {
        var all = ReflectionStore.AllTypesWithAttribute<InspectorAttribute>()
           .Where(type => !type.IsAbstract)
           .ToList();

        foreach (var type in all) {
            var attr   = type.GetCustomAttribute<InspectorAttribute>()!;
            var drawer = (BaseTypeDrawer) Activator.CreateInstance(type);
            Drawers[attr.Type]       = drawer;
            DrawerByDrawerType[type] = drawer;
        }
    }
}

public abstract class TypeDrawer : BaseTypeDrawer { }
public abstract class TypeDrawer<T> : TypeDrawer { }

[Inspector(typeof(bool))]
public class TypeDrawer_Bool : TypeDrawer<bool> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val = member.GetValue<bool>(container);
        if (ImGui.Checkbox($"##{member.Name}", ref val)) {
            member.SetValue(val, container);
        }
    }
}

[Inspector(typeof(int))]
public class TypeDrawer_Int : TypeDrawer<int> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val    = member.GetValue<int>(container);
        var minMax = member.GetMinMax();
        if (minMax != Vector2.Zero) {
            if (ImGui.SliderInt($"##{member.Name}", ref val, (int) minMax.X, (int) minMax.Y)) {
                member.SetValue(val, container);
            }
        } else {
            if (ImGui.InputInt($"##{member.Name}", ref val)) {
                member.SetValue(val, container);
            }
        }
    }
}

[Inspector(typeof(float))]
public class TypeDrawer_Float : TypeDrawer<float> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val    = member.GetValue<float>(container);
        var minMax = member.GetMinMax();

        if (minMax == Vector2.Zero) {
            if (ImGui.DragFloat($"##{member.Name}", ref val)) {
                member.SetValue(val, container);
            }
        } else {
            if (ImGui.SliderFloat($"##{member.Name}", ref val, minMax.X, minMax.Y)) {
                member.SetValue(val, container);
            }
        }
    }
}

[Inspector(typeof(string))]
public class TypeDrawer_String : TypeDrawer<string> {
    public override void Draw(ObjectMemberCache member, object container) {
        var flags = ImGuiInputTextFlags.None;
        if (member.Disabled) {
            flags |= ImGuiInputTextFlags.ReadOnly;
        }
        var val = member.GetValue<string>(container);
        if (ImGui.InputText($"##{member.Name}", ref val, 256, flags)) {
            member.SetValue(val, container);
        }

    }
}

[Inspector(typeof(Vector2))]
public class TypeDrawer_Vector2 : TypeDrawer<Vector2> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val    = member.GetValue<Vector2>(container);
        var minMax = member.GetMinMax();

        if (minMax != Vector2.Zero) {
            if (ImGui.SliderFloat2($"##{member.Name}", ref val, minMax.X, minMax.Y, "%.2f")) {
                member.SetValue(val, container);
            }
        } else {
            if (ImGui.DragFloat2($"##{member.Name}", ref val)) {
                member.SetValue(val, container);
            }
        }
    }
}

[Inspector(typeof(Vector3))]
public class TypeDrawer_Vector3 : TypeDrawer<Vector3> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val    = member.GetValue<Vector3>(container);
        var minMax = member.GetMinMax();
        if (minMax != Vector2.Zero) {
            if (ImGui.SliderFloat3($"##{member.Name}", ref val, minMax.X, minMax.Y, "%.2f")) {
                member.SetValue(val, container);
            }
        } else {
            if (ImGui.DragFloat3($"##{member.Name}", ref val)) {
                member.SetValue(val, container);
            }
        }
    }
}

[Inspector(typeof(Texture))]
public class TypeDrawer_Texture2D : TypeDrawer<Texture> {
    public override void Draw(ObjectMemberCache member, object container) {
        ImGuiRaylibPlatform.Image(member.GetValue<Texture>(container));
    }
}

[Inspector(typeof(AssetHandle<Texture>))]
public class TypeDrawer_AssetHandleTexture2D : TypeDrawer<AssetHandle<Texture>> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val = member.GetValue<AssetHandle<Texture>>(container);
        var tex = val.Asset;

        ImGuiRaylibPlatform.Image(tex);
    }
}

[Inspector(typeof(Color))]
public class TypeDrawer_Color : TypeDrawer<Color> {
    public override void Draw(ObjectMemberCache member, object container) {
        var col = member.GetValue<Color>(container);
        var val = col.Normalize();

        if (ImGui.ColorEdit4($"##{member.Name}", ref val)) {
            member.SetValue(val.ToColor(), container);
        }
    }
}

[Inspector(typeof(Common.Mathematics.Rectangle))]
public class TypeDrawer_Rectangle : TypeDrawer<Common.Mathematics.Rectangle> {
    public override void Draw(ObjectMemberCache member, object container) {
        var rect = member.GetValue<Common.Mathematics.Rectangle>(container);

        var rectCache = PropertyEditorCache.Get(member.Type);

        ImGui.BeginTable("Rectangle", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("");
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Position");
        ImGui.TableSetColumnIndex(1);
        GetDrawerType<TypeDrawer_Vector2>()
           .Draw(rectCache.Member("Position"), rect);

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text("Size");
        ImGui.TableSetColumnIndex(1);
        GetDrawerType<TypeDrawer_Vector2>()
           .Draw(rectCache.Member("SizeV"), rect);
        
        ImGui.EndTable();

    }
}

[Inspector(typeof(Transform2D))]
public class TypeDrawer_Transform2D : TypeDrawer<Transform2D> {
    public override void Draw(ObjectMemberCache member, object container) {
        var transform = member.GetValue<Transform2D>(container);
        if (transform == null) {
            ImGui.Text("null");
            return;
        }

        var propCache = PropertyEditorCache.Get(transform.GetType());

        ImGui.Text($"Translation: ");
        ImGui.SameLine();
        ImGui.TextColored(
            new Vector4(0.5f, 0.5f, 0.5f, 1),
            $"Global: {transform.GlobalPosition}"
        );

        GetDrawerType<TypeDrawer_Vector2>()
           .Draw(propCache.Member("Position"), transform);

        // var pos = transform.Position;
        // if (ImGui.DragFloat2("##translation", ref pos, "%.2f")) {
        //     transform.Position = pos;
        // }

        ImGui.Text($"Rotation: ");
        ImGui.SameLine();
        ImGui.TextColored(
            new Vector4(0.5f, 0.5f, 0.5f, 1),
            $"Global: {transform.GlobalRotation}"
        );

        GetDrawerType<TypeDrawer_Float>()
           .Draw(propCache.Member("Rotation"), transform);

        ImGui.Text($"Scale: ");
        ImGui.SameLine();
        ImGui.TextColored(
            new Vector4(0.5f, 0.5f, 0.5f, 1),
            $"Global: {transform.GlobalScale}"
        );

        GetDrawerType<TypeDrawer_Vector2>()
           .Draw(propCache.Member("Scale"), transform);

        //var scale = transform.Scale;
        //if (ImGui.DragFloat2("##scale", ref scale, 0.01f, "%.2f")) {
        //    transform.Scale = scale;
        //}

        // ImGui.TextUnformatted($"Transform2D Parent:{transform.Owner?.Transform?.Owner?.DebugName ?? "null"}");

        ImGui.Text("Pivot: ");

        GetDrawerType<TypeDrawer_Vector2>()
           .Draw(propCache.Member("Pivot"), transform);

        ImGui.Text("Dirty State: ");
        GetDrawerType<TypeDrawer_Bool>()
           .Draw(propCache.Member("IsDirty"), transform);

        ImGui.Text("Bounds: ");
        GetDrawerType<TypeDrawer_Rectangle>()
           .Draw(propCache.Member("Bounds"), transform);
    }
}

[Inspector(typeof(Enum))]
public class TypeDrawer_Enum : TypeDrawer<Enum> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val = member.GetValue(container);
        if (ImGui.BeginCombo($"##{member.Name}", val.ToString())) {
            foreach (var enumValue in Enum.GetValues(member.Type)) {
                var enumVal = (int) enumValue;
                if (ImGui.Selectable(enumValue.ToString(), enumVal == (int) val)) {
                    member.SetValue(enumVal, container);
                }
            }

            ImGui.EndCombo();
        }
    }
}

[Inspector(typeof(IEnumerable))]
public class TypeDrawer_IEnumerable : TypeDrawer<IEnumerable> {
    public override void Draw(ObjectMemberCache member, object container) {
        var val = member.GetValue(container);
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

                    var objProperties = PropertyEditorCache.Get(item.GetType());
                    objProperties.Render(item);
                    
                    // var objProperties = new ObjectProperties(item.GetType(), item);
                    // objProperties.RootCategory.Render();

                    ImGui.TreePop();
                }


                ImGui.PopID();
                i++;
            }
            ImGui.Indent();
            ImGui.TreePop();

        }

    }
}