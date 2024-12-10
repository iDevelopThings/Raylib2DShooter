using System.Reflection;
using Hexa.NET.ImGui;
using RLShooter.App.Editor.ImGuiIntegration;
using RLShooter.App.Editor.Windows.Inspectors;
using RLShooter.GameScene;
using RLShooter.Common.Utils.Reflection;

namespace RLShooter.App.Editor.PropertyEditor;

public class ObjectTypeCache {
    public Type Type { get; set; }

    public List<Attribute> ClassAttributes { get; set; } = new();

    public List<ObjectMemberCache> AllFields { get; set; } = new();

    public Dictionary<string, ObjectMemberCache> Fields { get; set; } = new();

    public PropertyNode RootCategory { get; set; }

    public ObjectTypeCache(Type type) {
        Type         = type;
        RootCategory = new("Root", true, this);

        ClassAttributes.AddRange(type.GetCustomAttributes());

        AllFields.AddRange(
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
               .Select(property => new ObjectMemberCache(property, this))
        );
        AllFields.AddRange(
            type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
               .Select(field => new ObjectMemberCache(field, this))
        );

        var eType = typeof(GameObject);
        var cType = typeof(Component);
        // Sort all fields so that any which are defined on `Entity` or `Component` are at the top
        AllFields.Sort((a, b) => {
            var aType = a.MemberInfo.DeclaringType;
            var bType = b.MemberInfo.DeclaringType;

            if (aType == eType && bType != eType)
                return -1;
            if (aType != eType && bType == eType)
                return 1;

            if (aType == cType && bType != cType)
                return -1;
            if (aType != cType && bType == cType)
                return 1;

            return 0;
        });

        for (var i = 0; i < AllFields.Count; i++) {
            var objectMember = AllFields[i];
            objectMember.SortOrder = objectMember.GetAttribute<InspectorOrder>()?.Order ?? i;
            AllFields[i]           = objectMember;
        }

        // Sort by order
        AllFields.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));

        Fields = AllFields.ToDictionary(member => member.MemberInfo.Name);


        // Build category tree
        foreach (var member in AllFields) {
            if (!member.CanDrawInInspector)
                continue;

            var categoryPath = (member.Category ?? "").Split('/');
            RootCategory.AddMember(categoryPath, member);
        }
    }

    public IEnumerator<ObjectMemberCache> GetEnumerator() => AllFields.GetEnumerator();

    // Casted T Attribute iterator
    public IEnumerable<T> GetClassAttributes<T>() where T : Attribute {
        return ClassAttributes.OfType<T>();
    }

    public ObjectMemberCache Member(string name) {
        return Fields.GetValueOrDefault(name);
    }

    public void Render(object container) {
        using var _ = new ImguiStyleVarScope(ImGuiStyleVar.CellPadding, new Vector2(5, 5));

        RootCategory.Render(container);
    }
}