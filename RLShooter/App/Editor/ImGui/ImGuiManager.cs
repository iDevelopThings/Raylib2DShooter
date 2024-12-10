using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using ImGuiConfigFlags = Hexa.NET.ImGui.ImGuiConfigFlags;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace RLShooter.App.Editor.ImGuiIntegration;

public class ImGuiManager {
    private static readonly Dictionary<string, ImFontPtr> aliasToFont = new();

    private ImGuiContextPtr   guiContext;
    private ImNodesContextPtr nodesContext;
    private ImPlotContextPtr  plotContext;

    public unsafe ImGuiManager() {
        // Create ImGui context
        guiContext = ImGui.CreateContext(null);

        // Set ImGui context
        ImGui.SetCurrentContext(guiContext);

        // Set ImGui context for ImGuizmo
        ImGuizmo.SetImGuiContext(guiContext);

        // Set ImGui context for ImPlot
        ImPlot.SetImGuiContext(guiContext);

        // Set ImGui context for ImNodes
        ImNodes.SetImGuiContext(guiContext);

        // Create and set ImNodes context and set style
        nodesContext = ImNodes.CreateContext();
        ImNodes.SetCurrentContext(nodesContext);
        ImNodes.StyleColorsDark(ImNodes.GetStyle());

        // Create and set ImPlot context and set style
        plotContext = ImPlot.CreateContext();
        ImPlot.SetCurrentContext(plotContext);
        ImPlot.StyleColorsDark(ImPlot.GetStyle());

        // Setup ImGui config.
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard; // Enable Keyboard Controls
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;  // Enable Gamepad Controls
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;     // Enable Docking
        //io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;       // Enable Multi-Viewport / Platform Windows
        io.ConfigViewportsNoAutoMerge   = false;
        io.ConfigViewportsNoTaskBarIcon = false;

        SetupFont();
        SetupStyle();

        // setup fonts.
        // var config = ImGui.ImFontConfig();
        // io.Fonts.AddFontDefault(/*config*/);

        // setup ImGui style
        /*
        var style  = ImGui.GetStyle();
        var colors = style.Colors;

        colors[(int) ImGuiCol.Text]                  = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int) ImGuiCol.TextDisabled]          = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int) ImGuiCol.WindowBg]              = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        colors[(int) ImGuiCol.ChildBg]               = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.PopupBg]               = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int) ImGuiCol.Border]                = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int) ImGuiCol.BorderShadow]          = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int) ImGuiCol.FrameBg]               = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.FrameBgHovered]        = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.FrameBgActive]         = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.TitleBg]               = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TitleBgActive]         = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int) ImGuiCol.TitleBgCollapsed]      = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.MenuBarBg]             = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.ScrollbarBg]           = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrab]         = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabHovered]  = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabActive]   = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.CheckMark]             = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.SliderGrab]            = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.SliderGrabActive]      = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.Button]                = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ButtonHovered]         = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.ButtonActive]          = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.Header]                = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.HeaderHovered]         = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int) ImGuiCol.HeaderActive]          = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int) ImGuiCol.Separator]             = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.SeparatorHovered]      = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.SeparatorActive]       = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.ResizeGrip]            = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripHovered]     = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripActive]      = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.Tab]                   = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabHovered]            = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.TabSelected]           = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int) ImGuiCol.TabDimmed]             = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabDimmedSelected]     = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.DockingPreview]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.DockingEmptyBg]        = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLines]             = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLinesHovered]      = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogram]         = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogramHovered]  = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TableHeaderBg]         = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderStrong]     = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderLight]      = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.TableRowBg]            = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.TableRowBgAlt]         = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int) ImGuiCol.TextSelectedBg]        = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.DragDropTarget]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.NavCursor]             = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int) ImGuiCol.NavWindowingDimBg]     = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int) ImGuiCol.ModalWindowDimBg]      = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

        style.WindowPadding     = new Vector2(8.00f, 8.00f);
        style.FramePadding      = new Vector2(5.00f, 2.00f);
        style.CellPadding       = new Vector2(6.00f, 6.00f);
        style.ItemSpacing       = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing  = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing     = 25;
        style.ScrollbarSize     = 15;
        style.GrabMinSize       = 10;
        style.WindowBorderSize  = 1;
        style.ChildBorderSize   = 1;
        style.PopupBorderSize   = 1;
        style.FrameBorderSize   = 1;
        style.TabBorderSize     = 1;
        style.WindowRounding    = 7;
        style.ChildRounding     = 4;
        style.FrameRounding     = 3;
        style.PopupRounding     = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding      = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding       = 4;

        // When viewports are enabled we tweak WindowRounding/WindowBg so platform windows can look identical to regular ones.
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
            style.WindowRounding                    = 0.0f;
            style.Colors[(int) ImGuiCol.WindowBg].W = 1.0f;
        }
        */

        ImGuiRaylibPlatform.Init();

    }
    private unsafe void SetupFont() {
        var io = ImGui.GetIO();

        var fonts = io.Fonts;
        fonts.FontBuilderFlags = (uint) ImFontAtlasFlags.NoPowerOfTwoHeight;
        fonts.TexDesiredWidth  = 2048;

        uint* glyphRanges = stackalloc uint[] {
            (char) 0xe005, (char) 0xe684,
            (char) 0xF000, (char) 0xF8FF,
            (char) 0 // null terminator
        };

        ImGuiFontBuilder defaultBuilder = new(fonts);
        defaultBuilder.AddFontFromFileTTF("Resources/Fonts/arial.ttf", 15)
           .SetOption(conf => conf.GlyphMinAdvanceX = 16)
           .AddFontFromFileTTF("Resources/Fonts/fa-solid-900.ttf", 14, glyphRanges)
           .AddFontFromFileTTF("Resources/Fonts/fa-brands-400.ttf", 14, glyphRanges);
        defaultBuilder.Destroy();

        ImGuiFontBuilder iconsRegularBuilder = new(fonts);
        iconsRegularBuilder.AddFontFromFileTTF("Resources/Fonts/arial.ttf", 15)
           .SetOption(conf => conf.GlyphMinAdvanceX = 16)
           .AddFontFromFileTTF("Resources/Fonts/fa-regular-400.ttf", 14, glyphRanges);
        aliasToFont.Add("Icons-Regular", iconsRegularBuilder.Font);
        iconsRegularBuilder.Destroy();

        uint* glyphMaterialRanges = stackalloc uint[] {
            (char) 0xe003, (char) 0xF8FF,
            (char) 0 // null terminator
        };

        ImGuiFontBuilder textEditorFontBuilder = new(fonts);
        textEditorFontBuilder.AddFontFromFileTTF("Resources/Fonts/CascadiaMono.ttf", size: 16)
           .SetOption(conf => conf.GlyphMinAdvanceX = 16)
           .AddFontFromFileTTF("Resources/Fonts/MaterialSymbolsRounded.ttf", 20, glyphMaterialRanges)
            ;
        aliasToFont.Add("TextEditorFont", textEditorFontBuilder.Font);
        textEditorFontBuilder.Destroy();

        fonts.Build();

    }

    private void SetupStyle() {
        var io     = ImGui.GetIO();
        var style  = ImGui.GetStyle();
        var colors = style.Colors;

        colors[(int) ImGuiCol.Text]                  = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int) ImGuiCol.TextDisabled]          = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int) ImGuiCol.WindowBg]              = new Vector4(0.13f, 0.13f, 0.13f, 1.00f);
        colors[(int) ImGuiCol.ChildBg]               = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.PopupBg]               = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int) ImGuiCol.Border]                = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int) ImGuiCol.BorderShadow]          = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int) ImGuiCol.FrameBg]               = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.FrameBgHovered]        = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.FrameBgActive]         = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.TitleBg]               = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TitleBgActive]         = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int) ImGuiCol.TitleBgCollapsed]      = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.MenuBarBg]             = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.ScrollbarBg]           = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrab]         = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabHovered]  = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabActive]   = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.CheckMark]             = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.SliderGrab]            = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.SliderGrabActive]      = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.Button]                = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ButtonHovered]         = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.ButtonActive]          = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.Header]                = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.HeaderHovered]         = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int) ImGuiCol.HeaderActive]          = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int) ImGuiCol.Separator]             = new Vector4(0.48f, 0.48f, 0.48f, 0.39f);
        colors[(int) ImGuiCol.SeparatorHovered]      = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.SeparatorActive]       = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.ResizeGrip]            = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripHovered]     = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripActive]      = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.Tab]                   = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabHovered]            = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.TabSelected]           = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int) ImGuiCol.TabDimmed]             = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabDimmedSelected]     = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.DockingPreview]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.DockingEmptyBg]        = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLines]             = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLinesHovered]      = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogram]         = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogramHovered]  = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TableHeaderBg]         = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderStrong]     = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderLight]      = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.TableRowBg]            = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.TableRowBgAlt]         = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int) ImGuiCol.TextSelectedBg]        = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.DragDropTarget]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        // colors[(int) ImGuiCol.NavHighlight]          = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int) ImGuiCol.NavWindowingDimBg]     = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int) ImGuiCol.ModalWindowDimBg]      = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

        style.WindowPadding     = new Vector2(8.00f, 8.00f);
        style.FramePadding      = new Vector2(8.00f, 6.00f);
        style.CellPadding       = new Vector2(6.00f, 6.00f);
        style.ItemSpacing       = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing  = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing     = 25;
        style.ScrollbarSize     = 15;
        style.GrabMinSize       = 12;
        style.WindowBorderSize  = 1;
        style.ChildBorderSize   = 1;
        style.PopupBorderSize   = 1;
        style.FrameBorderSize   = 1;
        style.TabBorderSize     = 1;
        style.WindowRounding    = 7;
        style.ChildRounding     = 4;
        style.FrameRounding     = 4;
        style.PopupRounding     = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding      = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding       = 4;

        // When viewports are enabled we tweak WindowRounding/WindowBg so platform windows can look identical to regular ones.
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
            style.WindowRounding                    = 0.0f;
            style.Colors[(int) ImGuiCol.WindowBg].W = 1.0f;
        }

    }

    public unsafe void NewFrame() {
        // Set ImGui context
        ImGui.SetCurrentContext(guiContext);
        // Set ImGui context for ImGuizmo
        ImGuizmo.SetImGuiContext(guiContext);
        // Set ImGui context for ImPlot
        ImPlot.SetImGuiContext(guiContext);
        // Set ImGui context for ImNodes
        ImNodes.SetImGuiContext(guiContext);

        // Set ImNodes context
        ImNodes.SetCurrentContext(nodesContext);
        // Set ImPlot context
        ImPlot.SetCurrentContext(plotContext);

        // Start new frame, call order matters.
        ImGuiRaylibPlatform.NewFrame();
        ImGui.NewFrame();
        ImGuizmo.BeginFrame(); // mandatory for ImGuizmo

        // Example for getting the central dockspace id of a window/viewport.
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
        DockSpaceId = ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode, null); // passing null as first argument will use the main viewport
        ImGui.PopStyleColor(1);
    }

    public static uint DockSpaceId { get; private set; }

    public unsafe void EndFrame() {
        // Renders ImGui Data
        var io = ImGui.GetIO();
        ImGui.Render();
        ImGui.EndFrame();

        ImGuiRaylibPlatform.RenderDrawData(ImGui.GetDrawData());

        // Update and Render additional Platform Windows
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }
    }

    public void Dispose() {
        ImGuiRaylibPlatform.Shutdown();
    }
}

/*public class ImGuiManager {
    private ImGuiContextPtr   guiContext;
    private ImNodesContextPtr nodesContext;
    private ImPlotContextPtr  plotContext;

    private static ImGuiMouseCursor                          CurrentMouseCursor = ImGuiMouseCursor.Count;
    private static Dictionary<ImGuiMouseCursor, MouseCursor> MouseCursorMap     = new();
    private static Texture                                   FontTexture;

    private static readonly Dictionary<string, ImFontPtr> aliasToFont = new();
    private static          int                           fontPushes  = 0;

    public static Dictionary<KeyboardKey, ImGuiKey> RaylibKeyMap = new();

    private static bool LastFrameFocused   = false;
    private static bool LastControlPressed = false;
    private static bool LastShiftPressed   = false;
    private static bool LastAltPressed     = false;
    private static bool LastSuperPressed   = false;

    private static bool IsControlDown() => IsKeyDown((int) KeyboardKey.RightControl) || IsKeyDown((int) KeyboardKey.LeftControl);
    private static bool IsShiftDown()   => IsKeyDown((int) KeyboardKey.RightShift) || IsKeyDown((int) KeyboardKey.LeftShift);
    private static bool IsAltDown()     => IsKeyDown((int) KeyboardKey.RightAlt) || IsKeyDown((int) KeyboardKey.LeftAlt);
    private static bool IsSuperDown()   => IsKeyDown((int) KeyboardKey.RightSuper) || IsKeyDown((int) KeyboardKey.LeftSuper);

    public event Action OnRenderDrawData;

    public uint DockSpaceId { get; set; }

    public unsafe ImGuiManager() {
        guiContext = ImGui.CreateContext(null);
        ImGui.SetCurrentContext(guiContext);
        ImGuizmo.SetImGuiContext(guiContext);
        ImPlot.SetImGuiContext(guiContext);
        ImNodes.SetImGuiContext(guiContext);

        nodesContext = ImNodes.CreateContext();
        ImNodes.SetCurrentContext(nodesContext);
        ImNodes.StyleColorsDark(ImNodes.GetStyle());

        plotContext = ImPlot.CreateContext();
        ImPlot.SetCurrentContext(plotContext);
        ImPlot.StyleColorsDark(ImPlot.GetStyle());


        var io = ImGui.GetIO();
        io.ConfigFlags                  |= ImGuiConfigFlags.NavEnableKeyboard; // Enable Keyboard Controls
        io.ConfigFlags                  |= ImGuiConfigFlags.NavEnableGamepad;  // Enable Gamepad Controls
        io.ConfigFlags                  |= ImGuiConfigFlags.DockingEnable;     // Enable Docking
        io.ConfigFlags                  |= ImGuiConfigFlags.ViewportsEnable;   // Enable Multi-Viewport / Platform Windows
        io.ConfigViewportsNoAutoMerge   =  false;
        io.ConfigViewportsNoTaskBarIcon =  false;
        io.ConfigDragClickToInputText   =  true;

        MouseCursorMap     = new Dictionary<ImGuiMouseCursor, MouseCursor>();
        LastFrameFocused   = IsWindowFocused();
        LastControlPressed = false;
        LastShiftPressed   = false;
        LastAltPressed     = false;
        LastSuperPressed   = false;
        FontTexture.Id     = 0;

        SetupFont();
        SetupStyle();
        SetupKeymap();
        SetupMouseCursors();
        ReloadFonts();

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos | ImGuiBackendFlags.HasGamepad;

        io.MousePos.X = 0;
        io.MousePos.Y = 0;

        io.SetClipboardTextFn = (void*) Marshal.GetFunctionPointerForDelegate<SetClipboardTextFn>(SetClipboardText);
        io.GetClipboardTextFn = (void*) Marshal.GetFunctionPointerForDelegate<GetClipboardTextFn>(GetClipboardText);
        io.ClipboardUserData  = null;
    }

    private void SetupStyle() {
        var io    = ImGui.GetIO();
        var style = ImGui.GetStyle();

        var colors = style.Colors;

        /*
        colors[(int) ImGuiCol.Text]                  = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int) ImGuiCol.TextDisabled]          = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int) ImGuiCol.WindowBg]              = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        colors[(int) ImGuiCol.ChildBg]               = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.PopupBg]               = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int) ImGuiCol.Border]                = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int) ImGuiCol.BorderShadow]          = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int) ImGuiCol.FrameBg]               = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.FrameBgHovered]        = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.FrameBgActive]         = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.TitleBg]               = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TitleBgActive]         = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int) ImGuiCol.TitleBgCollapsed]      = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.MenuBarBg]             = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.ScrollbarBg]           = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrab]         = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabHovered]  = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabActive]   = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.CheckMark]             = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.SliderGrab]            = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.SliderGrabActive]      = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.Button]                = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ButtonHovered]         = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.ButtonActive]          = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.Header]                = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.HeaderHovered]         = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int) ImGuiCol.HeaderActive]          = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int) ImGuiCol.Separator]             = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.SeparatorHovered]      = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.SeparatorActive]       = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.ResizeGrip]            = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripHovered]     = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripActive]      = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.Tab]                   = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabHovered]            = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.TabSelected]           = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int) ImGuiCol.TabDimmed]             = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabDimmedSelected]     = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.DockingPreview]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.DockingEmptyBg]        = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLines]             = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLinesHovered]      = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogram]         = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogramHovered]  = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TableHeaderBg]         = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderStrong]     = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderLight]      = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.TableRowBg]            = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.TableRowBgAlt]         = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int) ImGuiCol.TextSelectedBg]        = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.DragDropTarget]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.NavHighlight]          = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int) ImGuiCol.NavWindowingDimBg]     = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int) ImGuiCol.ModalWindowDimBg]      = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

        style.WindowPadding     = new Vector2(8.00f, 8.00f);
        style.FramePadding      = new Vector2(5.00f, 2.00f);
        style.CellPadding       = new Vector2(6.00f, 6.00f);
        style.ItemSpacing       = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing  = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing     = 25;
        style.ScrollbarSize     = 15;
        style.GrabMinSize       = 10;
        style.WindowBorderSize  = 1;
        style.ChildBorderSize   = 1;
        style.PopupBorderSize   = 1;
        style.FrameBorderSize   = 1;
        style.TabBorderSize     = 1;
        style.WindowRounding    = 7;
        style.ChildRounding     = 4;
        style.FrameRounding     = 3;
        style.PopupRounding     = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding      = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding       = 4;

        // When viewports are enabled we tweak WindowRounding/WindowBg so platform windows can look identical to regular ones.
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
            style.WindowRounding                    = 0.0f;
            style.Colors[(int) ImGuiCol.WindowBg].W = 1.0f;
        }#1#


        colors[(int) ImGuiCol.Text]                  = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int) ImGuiCol.TextDisabled]          = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int) ImGuiCol.WindowBg]              = new Vector4(0.13f, 0.13f, 0.13f, 1.00f);
        colors[(int) ImGuiCol.ChildBg]               = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.PopupBg]               = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int) ImGuiCol.Border]                = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int) ImGuiCol.BorderShadow]          = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int) ImGuiCol.FrameBg]               = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.FrameBgHovered]        = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.FrameBgActive]         = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.TitleBg]               = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TitleBgActive]         = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int) ImGuiCol.TitleBgCollapsed]      = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.MenuBarBg]             = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.ScrollbarBg]           = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrab]         = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabHovered]  = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabActive]   = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.CheckMark]             = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.SliderGrab]            = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.SliderGrabActive]      = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.Button]                = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ButtonHovered]         = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.ButtonActive]          = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.Header]                = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.HeaderHovered]         = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int) ImGuiCol.HeaderActive]          = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int) ImGuiCol.Separator]             = new Vector4(0.48f, 0.48f, 0.48f, 0.39f);
        colors[(int) ImGuiCol.SeparatorHovered]      = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.SeparatorActive]       = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.ResizeGrip]            = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripHovered]     = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripActive]      = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.Tab]                   = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabHovered]            = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.TabSelected]           = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int) ImGuiCol.TabDimmed]             = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabDimmedSelected]     = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.DockingPreview]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.DockingEmptyBg]        = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLines]             = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotLinesHovered]      = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogram]         = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogramHovered]  = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TableHeaderBg]         = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderStrong]     = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderLight]      = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.TableRowBg]            = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.TableRowBgAlt]         = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int) ImGuiCol.TextSelectedBg]        = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.DragDropTarget]        = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int) ImGuiCol.NavHighlight]          = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int) ImGuiCol.NavWindowingDimBg]     = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int) ImGuiCol.ModalWindowDimBg]      = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

        style.WindowPadding     = new Vector2(8.00f, 8.00f);
        style.FramePadding      = new Vector2(8.00f, 6.00f);
        style.CellPadding       = new Vector2(6.00f, 6.00f);
        style.ItemSpacing       = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing  = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing     = 25;
        style.ScrollbarSize     = 15;
        style.GrabMinSize       = 12;
        style.WindowBorderSize  = 1;
        style.ChildBorderSize   = 1;
        style.PopupBorderSize   = 1;
        style.FrameBorderSize   = 1;
        style.TabBorderSize     = 1;
        style.WindowRounding    = 7;
        style.ChildRounding     = 4;
        style.FrameRounding     = 4;
        style.PopupRounding     = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding      = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding       = 4;

        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
            style.WindowRounding                    = 0.0f;
            style.Colors[(int) ImGuiCol.WindowBg].W = 1.0f;
        }

    }

    private unsafe void SetupFont() {
        var io     = ImGui.GetIO();
        var config = ImGui.ImFontConfig();

        // io.Fonts.AddFontDefault(config);


        var fonts = io.Fonts;
        fonts.FontBuilderFlags = (uint) ImFontAtlasFlags.NoPowerOfTwoHeight;
        fonts.TexDesiredWidth  = 2048;

        char* glyphRanges = stackalloc char[] {
            (char) 0xe005, (char) 0xe684,
            (char) 0xF000, (char) 0xF8FF,
            (char) 0 // null terminator
        };

        ImGuiFontBuilder defaultBuilder = new(fonts);
        defaultBuilder.AddFontFromFileTTF("Resources/Fonts/ARIAL.TTF", 15)
           .SetOption(conf => conf.GlyphMinAdvanceX = 16)
           .AddFontFromFileTTF("Resources/Fonts/fa-solid-900.ttf", 14, glyphRanges)
           .AddFontFromFileTTF("Resources/Fonts/fa-brands-400.ttf", 14, glyphRanges);
        defaultBuilder.Destroy();

        ImGuiFontBuilder iconsRegularBuilder = new(fonts);
        iconsRegularBuilder.AddFontFromFileTTF("Resources/Fonts/ARIAL.TTF", 15)
           .SetOption(conf => conf.GlyphMinAdvanceX = 16)
           .AddFontFromFileTTF("Resources/Fonts/fa-regular-400.ttf", 14, glyphRanges);
        aliasToFont.Add("Icons-Regular", iconsRegularBuilder.Font);
        iconsRegularBuilder.Destroy();

        char* glyphMaterialRanges = stackalloc char[] {
            (char) 0xe003, (char) 0xF8FF,
            (char) 0 // null terminator
        };

        ImGuiFontBuilder textEditorFontBuilder = new(fonts);
        textEditorFontBuilder.AddFontFromFileTTF("Resources/Fonts/CascadiaMono.ttf", size: 16)
           .SetOption(conf => conf.GlyphMinAdvanceX = 16)
           .AddFontFromFileTTF("Resources/Fonts/MaterialSymbolsRounded.ttf", 20, glyphMaterialRanges)
            ;
        aliasToFont.Add("TextEditorFont", textEditorFontBuilder.Font);
        textEditorFontBuilder.Destroy();

        fonts.Build();


        // load custom font
        /*config.FontDataOwnedByAtlas = false; // Set this option to false to avoid ImGui to delete the data, used with fixed statement.
        config.MergeMode            = true;
        config.GlyphMinAdvanceX     = 18;
        config.GlyphOffset          = new(0, 4);
        var range = new char[] { (char)0xE700, (char)0xF800, (char)0 };
        fixed (char* buffer = range)
        {
            var bytes = File.ReadAllBytes("assets/fonts/SEGMDL2.TTF");
            fixed (byte* buffer2 = bytes)
            {
                // IMPORTANT: AddFontFromMemoryTTF() by default transfer ownership of the data buffer to the font atlas, which will attempt to free it on destruction.
                // This was to avoid an unnecessary copy, and is perhaps not a good API (a future version will redesign it).
                // Set config.FontDataOwnedByAtlas to false to keep ownership of the data (so you need to free the data yourself).
                io.Fonts.AddFontFromMemoryTTF(buffer2, bytes.Length, 14, config, buffer);
            }
        }#1#
    }

    private void SetupKeymap() {
        if (RaylibKeyMap.Count > 0)
            return;

        // build up a map of raylib keys to ImGuiKeys
        RaylibKeyMap[KeyboardKey.Apostrophe]   = ImGuiKey.Apostrophe;
        RaylibKeyMap[KeyboardKey.Comma]        = ImGuiKey.Comma;
        RaylibKeyMap[KeyboardKey.Minus]        = ImGuiKey.Minus;
        RaylibKeyMap[KeyboardKey.Period]       = ImGuiKey.Period;
        RaylibKeyMap[KeyboardKey.Slash]        = ImGuiKey.Slash;
        RaylibKeyMap[KeyboardKey.Zero]         = ImGuiKey.Key0;
        RaylibKeyMap[KeyboardKey.One]          = ImGuiKey.Key1;
        RaylibKeyMap[KeyboardKey.Two]          = ImGuiKey.Key2;
        RaylibKeyMap[KeyboardKey.Three]        = ImGuiKey.Key3;
        RaylibKeyMap[KeyboardKey.Four]         = ImGuiKey.Key4;
        RaylibKeyMap[KeyboardKey.Five]         = ImGuiKey.Key5;
        RaylibKeyMap[KeyboardKey.Six]          = ImGuiKey.Key6;
        RaylibKeyMap[KeyboardKey.Seven]        = ImGuiKey.Key7;
        RaylibKeyMap[KeyboardKey.Eight]        = ImGuiKey.Key8;
        RaylibKeyMap[KeyboardKey.Nine]         = ImGuiKey.Key9;
        RaylibKeyMap[KeyboardKey.Semicolon]    = ImGuiKey.Semicolon;
        RaylibKeyMap[KeyboardKey.Equal]        = ImGuiKey.Equal;
        RaylibKeyMap[KeyboardKey.A]            = ImGuiKey.A;
        RaylibKeyMap[KeyboardKey.B]            = ImGuiKey.B;
        RaylibKeyMap[KeyboardKey.C]            = ImGuiKey.C;
        RaylibKeyMap[KeyboardKey.D]            = ImGuiKey.D;
        RaylibKeyMap[KeyboardKey.E]            = ImGuiKey.E;
        RaylibKeyMap[KeyboardKey.F]            = ImGuiKey.F;
        RaylibKeyMap[KeyboardKey.G]            = ImGuiKey.G;
        RaylibKeyMap[KeyboardKey.H]            = ImGuiKey.H;
        RaylibKeyMap[KeyboardKey.I]            = ImGuiKey.I;
        RaylibKeyMap[KeyboardKey.J]            = ImGuiKey.J;
        RaylibKeyMap[KeyboardKey.K]            = ImGuiKey.K;
        RaylibKeyMap[KeyboardKey.L]            = ImGuiKey.L;
        RaylibKeyMap[KeyboardKey.M]            = ImGuiKey.M;
        RaylibKeyMap[KeyboardKey.N]            = ImGuiKey.N;
        RaylibKeyMap[KeyboardKey.O]            = ImGuiKey.O;
        RaylibKeyMap[KeyboardKey.P]            = ImGuiKey.P;
        RaylibKeyMap[KeyboardKey.Q]            = ImGuiKey.Q;
        RaylibKeyMap[KeyboardKey.R]            = ImGuiKey.R;
        RaylibKeyMap[KeyboardKey.S]            = ImGuiKey.S;
        RaylibKeyMap[KeyboardKey.T]            = ImGuiKey.T;
        RaylibKeyMap[KeyboardKey.U]            = ImGuiKey.U;
        RaylibKeyMap[KeyboardKey.V]            = ImGuiKey.V;
        RaylibKeyMap[KeyboardKey.W]            = ImGuiKey.W;
        RaylibKeyMap[KeyboardKey.X]            = ImGuiKey.X;
        RaylibKeyMap[KeyboardKey.Y]            = ImGuiKey.Y;
        RaylibKeyMap[KeyboardKey.Z]            = ImGuiKey.Z;
        RaylibKeyMap[KeyboardKey.Space]        = ImGuiKey.Space;
        RaylibKeyMap[KeyboardKey.Escape]       = ImGuiKey.Escape;
        RaylibKeyMap[KeyboardKey.Enter]        = ImGuiKey.Enter;
        RaylibKeyMap[KeyboardKey.Tab]          = ImGuiKey.Tab;
        RaylibKeyMap[KeyboardKey.Backspace]    = ImGuiKey.Backspace;
        RaylibKeyMap[KeyboardKey.Insert]       = ImGuiKey.Insert;
        RaylibKeyMap[KeyboardKey.Delete]       = ImGuiKey.Delete;
        RaylibKeyMap[KeyboardKey.Right]        = ImGuiKey.RightArrow;
        RaylibKeyMap[KeyboardKey.Left]         = ImGuiKey.LeftArrow;
        RaylibKeyMap[KeyboardKey.Down]         = ImGuiKey.DownArrow;
        RaylibKeyMap[KeyboardKey.Up]           = ImGuiKey.UpArrow;
        RaylibKeyMap[KeyboardKey.PageUp]       = ImGuiKey.PageUp;
        RaylibKeyMap[KeyboardKey.PageDown]     = ImGuiKey.PageDown;
        RaylibKeyMap[KeyboardKey.Home]         = ImGuiKey.Home;
        RaylibKeyMap[KeyboardKey.End]          = ImGuiKey.End;
        RaylibKeyMap[KeyboardKey.CapsLock]     = ImGuiKey.CapsLock;
        RaylibKeyMap[KeyboardKey.ScrollLock]   = ImGuiKey.ScrollLock;
        RaylibKeyMap[KeyboardKey.NumLock]      = ImGuiKey.NumLock;
        RaylibKeyMap[KeyboardKey.PrintScreen]  = ImGuiKey.PrintScreen;
        RaylibKeyMap[KeyboardKey.Pause]        = ImGuiKey.Pause;
        RaylibKeyMap[KeyboardKey.F1]           = ImGuiKey.F1;
        RaylibKeyMap[KeyboardKey.F2]           = ImGuiKey.F2;
        RaylibKeyMap[KeyboardKey.F3]           = ImGuiKey.F3;
        RaylibKeyMap[KeyboardKey.F4]           = ImGuiKey.F4;
        RaylibKeyMap[KeyboardKey.F5]           = ImGuiKey.F5;
        RaylibKeyMap[KeyboardKey.F6]           = ImGuiKey.F6;
        RaylibKeyMap[KeyboardKey.F7]           = ImGuiKey.F7;
        RaylibKeyMap[KeyboardKey.F8]           = ImGuiKey.F8;
        RaylibKeyMap[KeyboardKey.F9]           = ImGuiKey.F9;
        RaylibKeyMap[KeyboardKey.F10]          = ImGuiKey.F10;
        RaylibKeyMap[KeyboardKey.F11]          = ImGuiKey.F11;
        RaylibKeyMap[KeyboardKey.F12]          = ImGuiKey.F12;
        RaylibKeyMap[KeyboardKey.LeftShift]    = ImGuiKey.LeftShift;
        RaylibKeyMap[KeyboardKey.LeftControl]  = ImGuiKey.LeftCtrl;
        RaylibKeyMap[KeyboardKey.LeftAlt]      = ImGuiKey.LeftAlt;
        RaylibKeyMap[KeyboardKey.LeftSuper]    = ImGuiKey.LeftSuper;
        RaylibKeyMap[KeyboardKey.RightShift]   = ImGuiKey.RightShift;
        RaylibKeyMap[KeyboardKey.RightControl] = ImGuiKey.RightCtrl;
        RaylibKeyMap[KeyboardKey.RightAlt]     = ImGuiKey.RightAlt;
        RaylibKeyMap[KeyboardKey.RightSuper]   = ImGuiKey.RightSuper;
        RaylibKeyMap[KeyboardKey.KbMenu]       = ImGuiKey.Menu;
        RaylibKeyMap[KeyboardKey.LeftBracket]  = ImGuiKey.LeftBracket;
        RaylibKeyMap[KeyboardKey.Backslash]    = ImGuiKey.Backslash;
        RaylibKeyMap[KeyboardKey.RightBracket] = ImGuiKey.RightBracket;
        RaylibKeyMap[KeyboardKey.Grave]        = ImGuiKey.GraveAccent;
        RaylibKeyMap[KeyboardKey.Kp0]          = ImGuiKey.Keypad0;
        RaylibKeyMap[KeyboardKey.Kp1]          = ImGuiKey.Keypad1;
        RaylibKeyMap[KeyboardKey.Kp2]          = ImGuiKey.Keypad2;
        RaylibKeyMap[KeyboardKey.Kp3]          = ImGuiKey.Keypad3;
        RaylibKeyMap[KeyboardKey.Kp4]          = ImGuiKey.Keypad4;
        RaylibKeyMap[KeyboardKey.Kp5]          = ImGuiKey.Keypad5;
        RaylibKeyMap[KeyboardKey.Kp6]          = ImGuiKey.Keypad6;
        RaylibKeyMap[KeyboardKey.Kp7]          = ImGuiKey.Keypad7;
        RaylibKeyMap[KeyboardKey.Kp8]          = ImGuiKey.Keypad8;
        RaylibKeyMap[KeyboardKey.Kp9]          = ImGuiKey.Keypad9;
        RaylibKeyMap[KeyboardKey.KpDecimal]    = ImGuiKey.KeypadDecimal;
        RaylibKeyMap[KeyboardKey.KpDivide]     = ImGuiKey.KeypadDivide;
        RaylibKeyMap[KeyboardKey.KpMultiply]   = ImGuiKey.KeypadMultiply;
        RaylibKeyMap[KeyboardKey.KpSubtract]   = ImGuiKey.KeypadSubtract;
        RaylibKeyMap[KeyboardKey.KpAdd]        = ImGuiKey.KeypadAdd;
        RaylibKeyMap[KeyboardKey.KpEnter]      = ImGuiKey.KeypadEnter;
        RaylibKeyMap[KeyboardKey.KpEqual]      = ImGuiKey.KeypadEqual;
    }

    private void SetupMouseCursors() {
        MouseCursorMap.Clear();
        MouseCursorMap[ImGuiMouseCursor.Arrow]      = MouseCursor.Arrow;
        MouseCursorMap[ImGuiMouseCursor.TextInput]  = MouseCursor.Ibeam;
        MouseCursorMap[ImGuiMouseCursor.Hand]       = MouseCursor.PointingHand;
        MouseCursorMap[ImGuiMouseCursor.ResizeAll]  = MouseCursor.ResizeAll;
        MouseCursorMap[ImGuiMouseCursor.ResizeEw]   = MouseCursor.ResizeEw;
        MouseCursorMap[ImGuiMouseCursor.ResizeNesw] = MouseCursor.ResizeNesw;
        MouseCursorMap[ImGuiMouseCursor.ResizeNs]   = MouseCursor.ResizeNs;
        MouseCursorMap[ImGuiMouseCursor.ResizeNwse] = MouseCursor.ResizeNwse;
        MouseCursorMap[ImGuiMouseCursor.NotAllowed] = MouseCursor.NotAllowed;
    }

    private unsafe void ReloadFonts() {
        ImGui.SetCurrentContext(guiContext);
        var io = ImGui.GetIO();

        byte* pixels        = null;
        var   width         = 0;
        var   height        = 0;
        var   bytesPerPixel = 0;

        io.Fonts.GetTexDataAsRGBA32(ref pixels, ref width, ref height, ref bytesPerPixel);

        var image = new Image {
            Data    = new IntPtr(pixels).ToPointer(),
            Width   = width,
            Height  = height,
            Mipmaps = 1,
            Format  = (int) PixelFormat.UncompressedR8G8B8A8,
        };

        if (IsTextureValid(FontTexture))
            UnloadTexture(FontTexture);

        FontTexture = LoadTextureFromImage(image);

        io.Fonts.SetTexID(new IntPtr(FontTexture.Id));
    }

    private unsafe byte* GetClipboardText(void* userData) {
        var txt = Raylib.GetClipboardText();
        return txt;
        // return (byte*) Marshal.StringToHGlobalAnsi(txt).ToPointer();
    }
    private unsafe void SetClipboardText(void* userData, byte* text) {
        Raylib.SetClipboardText(text);
        //Marshal.PtrToStringAnsi(new IntPtr(text))!);
    }


    public unsafe void BeginFrame() {
        ImGui.SetCurrentContext(guiContext);
        ImGuizmo.SetImGuiContext(guiContext);
        ImPlot.SetImGuiContext(guiContext);
        ImNodes.SetImGuiContext(guiContext);
        ImNodes.SetCurrentContext(nodesContext);
        ImPlot.SetCurrentContext(plotContext);

        // Start new frame, call order matters.
        BackendNewFrame();
        FrameEvents();

        ImGui.NewFrame();
        ImGuizmo.BeginFrame(); // mandatory for ImGuizmo

        // Example for getting the central dockspace id of a window/viewport.
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
        DockSpaceId = ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode, null); // passing null as first argument will use the main viewport
        ImGui.PopStyleColor(1);
    }
    private void BackendNewFrame() {
        var io = ImGui.GetIO();

        if (IsWindowFullscreen()) {
            var monitor = GetCurrentMonitor();
            io.DisplaySize = new Vector2(GetMonitorWidth(monitor), GetMonitorHeight(monitor));
        } else {
            io.DisplaySize = new Vector2(GetScreenWidth(), GetScreenHeight());
        }

        io.DisplayFramebufferScale = new Vector2(1, 1);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || IsWindowState((int) ConfigFlags.FlagWindowHighdpi))
            io.DisplayFramebufferScale = GetWindowScaleDPI();

        io.DeltaTime = GetFrameTime();

        if (io.WantSetMousePos) {
            SetMousePosition((int) io.MousePos.X, (int) io.MousePos.Y);
        } else {
            io.AddMousePosEvent(GetMouseX(), GetMouseY());
        }

        SetMouseEvent(io, MouseButton.Left, ImGuiMouseButton.Left);
        SetMouseEvent(io, MouseButton.Right, ImGuiMouseButton.Right);
        SetMouseEvent(io, MouseButton.Middle, ImGuiMouseButton.Middle);
        SetMouseEvent(io, MouseButton.Forward, ImGuiMouseButton.Middle + 1);
        SetMouseEvent(io, MouseButton.Back, ImGuiMouseButton.Middle + 2);

        var wheelMove = GetMouseWheelMoveV();
        io.AddMouseWheelEvent(wheelMove.X, wheelMove.Y);

        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
            return;

        var imgui_cursor = ImGui.GetMouseCursor();
        if (imgui_cursor == CurrentMouseCursor && !io.MouseDrawCursor)
            return;
        CurrentMouseCursor = imgui_cursor;

        if (io.MouseDrawCursor || imgui_cursor == ImGuiMouseCursor.None) {
            HideCursor();
        } else {
            ShowCursor();

            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
                return;
            SetMouseCursor((int) MouseCursorMap.GetValueOrDefault(imgui_cursor, MouseCursor.Default));
        }
    }
    private void FrameEvents() {
        var io = ImGui.GetIO();

        bool focused = IsWindowFocused();
        if (focused != LastFrameFocused)
            io.AddFocusEvent(focused);
        LastFrameFocused = focused;


        // handle the modifyer key events so that shortcuts work
        var ctrlDown = IsControlDown();
        if (ctrlDown != LastControlPressed)
            io.AddKeyEvent(ImGuiKey.ModCtrl, ctrlDown);
        LastControlPressed = ctrlDown;

        var shiftDown = IsShiftDown();
        if (shiftDown != LastShiftPressed)
            io.AddKeyEvent(ImGuiKey.ModShift, shiftDown);
        LastShiftPressed = shiftDown;

        var altDown = IsAltDown();
        if (altDown != LastAltPressed)
            io.AddKeyEvent(ImGuiKey.ModAlt, altDown);
        LastAltPressed = altDown;

        var superDown = IsSuperDown();
        if (superDown != LastSuperPressed)
            io.AddKeyEvent(ImGuiKey.ModSuper, superDown);
        LastSuperPressed = superDown;

        // get the pressed keys, they are in event order
        var keyId = GetKeyPressed();
        while (keyId != 0) {
            var key = (KeyboardKey) keyId;
            if (RaylibKeyMap.TryGetValue(key, out var value))
                io.AddKeyEvent(value, true);
            keyId = GetKeyPressed();
        }

        // look for any keys that were down last frame and see if they were down and are released
        foreach (var keyItr in RaylibKeyMap) {
            if (IsKeyReleased((int) keyItr.Key))
                io.AddKeyEvent(keyItr.Value, false);
        }

        // add the text input in order
        var pressed = GetCharPressed();
        while (pressed != 0) {
            io.AddInputCharacter((uint) pressed);
            pressed = GetCharPressed();
        }

        // gamepads
        if ((io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) != 0 && IsGamepadAvailable(0)) {
            HandleGamepadButtonEvent(io, GamepadButton.LeftFaceUp, ImGuiKey.GamepadDpadUp);
            HandleGamepadButtonEvent(io, GamepadButton.LeftFaceRight, ImGuiKey.GamepadDpadRight);
            HandleGamepadButtonEvent(io, GamepadButton.LeftFaceDown, ImGuiKey.GamepadDpadDown);
            HandleGamepadButtonEvent(io, GamepadButton.LeftFaceLeft, ImGuiKey.GamepadDpadLeft);

            HandleGamepadButtonEvent(io, GamepadButton.RightFaceUp, ImGuiKey.GamepadFaceUp);
            HandleGamepadButtonEvent(io, GamepadButton.RightFaceRight, ImGuiKey.GamepadFaceLeft);
            HandleGamepadButtonEvent(io, GamepadButton.RightFaceDown, ImGuiKey.GamepadFaceDown);
            HandleGamepadButtonEvent(io, GamepadButton.RightFaceLeft, ImGuiKey.GamepadFaceRight);

            HandleGamepadButtonEvent(io, GamepadButton.LeftTrigger1, ImGuiKey.GamepadL1);
            HandleGamepadButtonEvent(io, GamepadButton.LeftTrigger2, ImGuiKey.GamepadL2);
            HandleGamepadButtonEvent(io, GamepadButton.RightTrigger1, ImGuiKey.GamepadR1);
            HandleGamepadButtonEvent(io, GamepadButton.RightTrigger2, ImGuiKey.GamepadR2);
            HandleGamepadButtonEvent(io, GamepadButton.LeftThumb, ImGuiKey.GamepadL3);
            HandleGamepadButtonEvent(io, GamepadButton.RightThumb, ImGuiKey.GamepadR3);

            HandleGamepadButtonEvent(io, GamepadButton.MiddleLeft, ImGuiKey.GamepadStart);
            HandleGamepadButtonEvent(io, GamepadButton.MiddleRight, ImGuiKey.GamepadBack);

            // left stick
            HandleGamepadStickEvent(io, GamepadAxis.LeftX, ImGuiKey.GamepadLStickLeft, ImGuiKey.GamepadLStickRight);
            HandleGamepadStickEvent(io, GamepadAxis.LeftY, ImGuiKey.GamepadLStickUp, ImGuiKey.GamepadLStickDown);

            // right stick
            HandleGamepadStickEvent(io, GamepadAxis.RightX, ImGuiKey.GamepadRStickLeft, ImGuiKey.GamepadRStickRight);
            HandleGamepadStickEvent(io, GamepadAxis.RightY, ImGuiKey.GamepadRStickUp, ImGuiKey.GamepadRStickDown);
        }
    }

    private void SetMouseEvent(ImGuiIOPtr io, MouseButton rayMouse, ImGuiMouseButton imGuiMouse) {
        if (IsMouseButtonPressed((int) rayMouse))
            io.AddMouseButtonEvent((int) imGuiMouse, true);
        else if (IsMouseButtonReleased((int) rayMouse))
            io.AddMouseButtonEvent((int) imGuiMouse, false);
    }
    private void HandleGamepadButtonEvent(ImGuiIOPtr io, GamepadButton button, ImGuiKey key) {
        if (IsGamepadButtonPressed(0, (int) button))
            io.AddKeyEvent(key, true);
        else if (IsGamepadButtonReleased(0, (int) button))
            io.AddKeyEvent(key, false);
    }
    private void HandleGamepadStickEvent(ImGuiIOPtr io, GamepadAxis axis, ImGuiKey negKey, ImGuiKey posKey) {
        const float deadZone = 0.20f;

        var axisValue = GetGamepadAxisMovement(0, (int) axis);

        io.AddKeyAnalogEvent(negKey, axisValue < -deadZone, axisValue < -deadZone ? -axisValue : 0);
        io.AddKeyAnalogEvent(posKey, axisValue > deadZone, axisValue > deadZone ? axisValue : 0);
    }


    private void EnableScissor(float x, float y, float width, float height) {
        RlEnableScissorTest();
        var io = ImGui.GetIO();

        var scale = new Vector2(1.0f, 1.0f);
        if (IsWindowState((int) ConfigFlags.FlagWindowHighdpi) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            scale = io.DisplayFramebufferScale;

        RlScissor((int) (x * scale.X),
                  (int) ((io.DisplaySize.Y - (int) (y + height)) * scale.Y),
                  (int) (width * scale.X),
                  (int) (height * scale.Y));
    }
    private void TriangleVert(ImDrawVert idx_vert) {
        var color = ImGui.ColorConvertU32ToFloat4(idx_vert.Col);

        RlColor4F(color.X, color.Y, color.Z, color.W);
        RlTexCoord2F(idx_vert.Uv.X, idx_vert.Uv.Y);
        RlVertex2F(idx_vert.Pos.X, idx_vert.Pos.Y);
    }
    private void RenderTriangles(
        uint                       count,
        uint                       indexStart,
        ImVector<ushort>           indexBuffer,
        ImVector<ImDrawVert>       vertBuffer,
        Hexa.NET.ImGui.ImTextureID texturePtr
    ) {
        if (count < 3)
            return;

        uint textureId = 0;
        if (texturePtr != IntPtr.Zero)
            textureId = (uint) texturePtr.Handle.ToInt32();

        RlBegin(RL_TRIANGLES);
        RlSetTexture(textureId);

        for (var i = 0; i <= (count - 3); i += 3) {
            // if (RlCheckRenderBatchLimit(3)) {
            //     RlBegin(DrawMode.Triangles);
            //     RlSetTexture(textureId);
            // }

            var indexA = indexBuffer[(int) indexStart + i];
            var indexB = indexBuffer[(int) indexStart + i + 1];
            var indexC = indexBuffer[(int) indexStart + i + 2];

            var vertexA = vertBuffer[indexA];
            var vertexB = vertBuffer[indexB];
            var vertexC = vertBuffer[indexC];

            TriangleVert(vertexA);
            TriangleVert(vertexB);
            TriangleVert(vertexC);
        }

        RlEnd();
    }

    private delegate void Callback(ImDrawListPtr list, ImDrawCmdPtr cmd);

    private unsafe void RenderData() {
        RlDrawRenderBatchActive();
        RlDisableBackfaceCulling();

        var data = ImGui.GetDrawData();

        for (var l = 0; l < data.CmdListsCount; l++) {
            var commandList = data.CmdLists[l];

            for (var cmdIndex = 0; cmdIndex < commandList.CmdBuffer.Size; cmdIndex++) {
                var cmd = commandList.CmdBuffer[cmdIndex];

                EnableScissor(
                    cmd.ClipRect.X - data.DisplayPos.X,
                    cmd.ClipRect.Y - data.DisplayPos.Y,
                    cmd.ClipRect.Z - (cmd.ClipRect.X - data.DisplayPos.X),
                    cmd.ClipRect.W - (cmd.ClipRect.Y - data.DisplayPos.Y)
                );
                if (cmd.UserCallback != null) {
                    Marshal.GetDelegateForFunctionPointer<UserCallback>((nint) cmd.UserCallback)(commandList, &cmd);
                    continue;
                }

                RenderTriangles(cmd.ElemCount, cmd.IdxOffset, commandList.IdxBuffer, commandList.VtxBuffer, cmd.TextureId);

                RlDrawRenderBatchActive();
            }
        }

        RlSetTexture(0);
        RlDisableScissorTest();
        RlEnableBackfaceCulling();
    }

    public void EndFrame() {
        var io = ImGui.GetIO();
        ImGui.Render();
        ImGui.EndFrame();

        RenderData();
        OnRenderDrawData?.Invoke();

        // Update and Render additional Platform Windows
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }

    }

    /// <summary>
    /// Cleanup ImGui and unload font atlas
    /// </summary>
    public void Shutdown() {
        UnloadTexture(FontTexture);
        ImGui.DestroyContext();

        // remove this if you don't want font awesome support
        /*{
            if (IconFonts.FontAwesome6.IconFontRanges != IntPtr.Zero)
                Marshal.FreeHGlobal(IconFonts.FontAwesome6.IconFontRanges);

            IconFonts.FontAwesome6.IconFontRanges = IntPtr.Zero;
        }#1#
    }


    public static void PushFont(string name) {
        if (aliasToFont.TryGetValue(name, out ImFontPtr fontPtr)) {
            ImGui.PushFont(fontPtr);
            fontPushes++;
        }
    }

    public static void PushFont(string name, bool condition) {
        if (condition && aliasToFont.TryGetValue(name, out ImFontPtr fontPtr)) {
            ImGui.PushFont(fontPtr);
            fontPushes++;
        }
    }

    public static void PopFont() {
        if (fontPushes == 0) {
            return;
        }

        ImGui.PopFont();
        fontPushes--;
    }
    /// <summary>
    /// Draw a texture as an image in an ImGui Context
    /// Uses the current ImGui Cursor position and the full texture size.
    /// </summary>
    /// <param name="image">The raylib texture to draw</param>
    public static void Image(Texture image) {
        ImGui.Image(new IntPtr(image.Id), new Vector2(image.Width, image.Height));
    }

    /// <summary>
    /// Draw a texture as an image in an ImGui Context at a specific size
    /// Uses the current ImGui Cursor position and the specified width and height
    /// The image will be scaled up or down to fit as needed
    /// </summary>
    /// <param name="image">The raylib texture to draw</param>
    /// <param name="width">The width of the drawn image</param>
    /// <param name="height">The height of the drawn image</param>
    public static void ImageSize(Texture image, int width, int height) {
        ImGui.Image(new IntPtr(image.Id), new Vector2(width, height));
    }

    /// <summary>
    /// Draw a texture as an image in an ImGui Context at a specific size
    /// Uses the current ImGui Cursor position and the specified size
    /// The image will be scaled up or down to fit as needed
    /// </summary>
    /// <param name="image">The raylib texture to draw</param>
    /// <param name="size">The size of drawn image</param>
    public static void ImageSize(Texture image, Vector2 size) {
        ImGui.Image(new IntPtr(image.Id), size);
    }

    /// <summary>
    /// Draw a portion texture as an image in an ImGui Context at a defined size
    /// Uses the current ImGui Cursor position and the specified size
    /// The image will be scaled up or down to fit as needed
    /// </summary>
    /// <param name="image">The raylib texture to draw</param>
    /// <param name="destWidth">The width of the drawn image</param>
    /// <param name="destHeight">The height of the drawn image</param>
    /// <param name="sourceRect">The portion of the texture to draw as an image. Negative values for the width and height will flip the image</param>
    public static void ImageRect(Texture image, int destWidth, int destHeight, Rectangle sourceRect) {
        var uv0 = new Vector2();
        var uv1 = new Vector2();

        if (sourceRect.Width < 0) {
            uv0.X = -(sourceRect.X / image.Width);
            uv1.X = (uv0.X - Math.Abs(sourceRect.Width) / image.Width);
        } else {
            uv0.X = sourceRect.X / image.Width;
            uv1.X = uv0.X + sourceRect.Width / image.Width;
        }

        if (sourceRect.Height < 0) {
            uv0.Y = -(sourceRect.Y / image.Height);
            uv1.Y = (uv0.Y - Math.Abs(sourceRect.Height) / image.Height);
        } else {
            uv0.Y = sourceRect.Y / image.Height;
            uv1.Y = uv0.Y + sourceRect.Height / image.Height;
        }

        ImGui.Image(new IntPtr(image.Id), new Vector2(destWidth, destHeight), uv0, uv1);
    }

    /// <summary>
    /// Draws a render texture as an image an ImGui Context, automatically flipping the Y axis so it will show correctly on screen
    /// </summary>
    /// <param name="image">The render texture to draw</param>
    public static void ImageRenderTexture(RenderTexture image) {
        ImageRect(image.Texture, image.Texture.Width, image.Texture.Height, new Rectangle(0, 0, image.Texture.Width, -image.Texture.Height));
    }

    /// <summary>
    /// Draws a render texture as an image to the current ImGui Context, flipping the Y axis so it will show correctly on the screen
    /// The texture will be scaled to fit the content are available, centered if desired
    /// </summary>
    /// <param name="image">The render texture to draw</param>
    /// <param name="center">When true the texture will be centered in the content area. When false the image will be left and top justified</param>
    public static void ImageRenderTextureFit(RenderTexture image, bool center = true) {
        var area = ImGui.GetContentRegionAvail();

        var scale = area.X / image.Texture.Width;

        var y = image.Texture.Height * scale;
        if (y > area.Y) {
            scale = area.Y / image.Texture.Height;
        }

        var sizeX = (int) (image.Texture.Width * scale);
        var sizeY = (int) (image.Texture.Height * scale);

        if (center) {
            ImGui.SetCursorPosX(0);
            ImGui.SetCursorPosX(area.X / 2 - sizeX / 2);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (area.Y / 2 - sizeY / 2));
        }

        ImageRect(image.Texture, sizeX, sizeY, new Rectangle(0, 0, (image.Texture.Width), -(image.Texture.Height)));
    }

    /// <summary>
    /// Draws a texture as an image button in an ImGui context. Uses the current ImGui cursor position and the full size of the texture
    /// </summary>
    /// <param name="name">The display name and ImGui ID for the button</param>
    /// <param name="image">The texture to draw</param>
    /// <returns>True if the button was clicked</returns>
    public static bool ImageButton(string name, Texture image) {
        return ImageButtonSize(name, image, new Vector2(image.Width, image.Height));
    }

    /// <summary>
    /// Draws a texture as an image button in an ImGui context. Uses the current ImGui cursor position and the specified size.
    /// </summary>
    /// <param name="name">The display name and ImGui ID for the button</param>
    /// <param name="image">The texture to draw</param>
    /// <param name="size">The size of the button/param>
    /// <returns>True if the button was clicked</returns>
    public static bool ImageButtonSize(string name, Texture image, Vector2 size) {
        return ImGui.ImageButton(name, new IntPtr(image.Id), size);
    }
}*/