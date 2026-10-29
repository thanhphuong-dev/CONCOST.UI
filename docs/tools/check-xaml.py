"""
check-xaml.py — sanity-check the XAML snippets in docs/data/*.js against the real library API.

It flags:
  * control tags that do not exist as a class in src/CONCOST.UI
  * attributes that are not a DependencyProperty of that control (and not a common WPF property)

Usage: python docs/tools/check-xaml.py
"""
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
DOCS = os.path.dirname(HERE)
REPO = os.path.dirname(DOCS)

spec = importlib.util.spec_from_file_location("bp", os.path.join(HERE, "build-props.py"))
bp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(bp)

CLASSES, ENUMS = bp.extract()

# Every public class name (including ones without their own DependencyProperty),
# plus the inheritance chain so inherited properties/events are accepted.
ALL_CLASSES = set()
BASE_OF = {}
for dirpath, dirnames, files in os.walk(os.path.join(REPO, "src", "CONCOST.UI")):
    dirnames[:] = [d for d in dirnames if d not in ("obj", "bin")]
    for f in files:
        if not f.endswith(".cs"):
            continue
        raw = open(os.path.join(dirpath, f), encoding="utf-8-sig", errors="ignore").read()
        ALL_CLASSES.update(re.findall(r"public\s+(?:sealed\s+|abstract\s+|partial\s+)*class\s+([A-Za-z0-9_]+)", raw))
        for m in re.finditer(r"public\s+(?:sealed\s+|abstract\s+|partial\s+)*class\s+([A-Za-z0-9_]+)\s*:\s*([A-Za-z0-9_]+)", raw):
            BASE_OF.setdefault(m.group(1), m.group(2))


def inherited(cls):
    """Own + inherited property and event names."""
    names = set()
    seen = set()
    cur = cls
    while cur and cur not in seen:
        seen.add(cur)
        info = CLASSES.get(cur)
        if info:
            names.update(p["name"] for p in info["props"])
            names.update(info["events"])
        cur = BASE_OF.get(cur)
    return names

# Common WPF / framework properties that are always fine in a snippet.
COMMON = {
    "x:Name", "x:Key", "Name", "Width", "Height", "MinWidth", "MinHeight", "MaxWidth", "MaxHeight",
    "Margin", "Padding", "Background", "Foreground", "BorderBrush", "BorderThickness", "FontSize",
    "FontWeight", "FontFamily", "HorizontalAlignment", "VerticalAlignment", "HorizontalContentAlignment",
    "VerticalContentAlignment", "Style", "Content", "Header", "Text", "IsEnabled", "Visibility",
    "Tag", "ToolTip", "DataContext", "ItemsSource", "SelectedItem", "SelectedIndex", "SelectedValue",
    "SelectionMode", "Click", "Command", "CommandParameter", "Orientation", "Grid.Row", "Grid.Column",
    "Grid.RowSpan", "Grid.ColumnSpan", "DockPanel.Dock", "Canvas.Left", "Canvas.Top", "Panel.ZIndex",
    "TextWrapping", "TextTrimming", "IsChecked", "IsThreeState", "GroupName", "Minimum", "Maximum",
    "Value", "TickFrequency", "IsSnapToTickEnabled", "IsDirectionReversed", "AutoGenerateColumns",
    "DisplayMemberPath", "SelectedValuePath", "IsReadOnly", "IsHitTestVisible", "Opacity", "Cursor",
    "RowDefinition", "ColumnDefinition", "IsExpanded", "ExpandDirection", "Placement", "IsOpen",
    "CornerRadius", "Effect", "ClipToBounds", "FlowDirection", "Language", "SnapsToDevicePixels",
    "UseLayoutRounding", "TextAlignment", "LineHeight", "IsTabStop", "Focusable", "ScrollViewer.HorizontalScrollBarVisibility",
    "ScrollViewer.VerticalScrollBarVisibility", "VerticalScrollBarVisibility", "HorizontalScrollBarVisibility",
    "x:Class", "xmlns", "xmlns:x", "Title", "WindowStartupLocation", "WindowStyle", "ResizeMode",
    "MinLines", "MaxLines", "AcceptsReturn", "AcceptsTab", "SpellCheck.IsEnabled", "StaysOpen",
    "IsDefault", "IsCancel", "IsDropDownOpen", "MaxDropDownHeight", "IsEditable", "IsTextSearchEnabled",
}

# Attached classes we ship, with their property names.
ATTACHED = {name: {p["name"] for p in info["props"]} for name, info in CLASSES.items()
            if info["namespace"].endswith("AttachedProperties")}

TAG_RE = re.compile(r"<([a-zA-Z][\w]*):([A-Za-z0-9_]+)((?:\s+[^<>]*?)?)/?>", re.S)
ATTR_RE = re.compile(r"([A-Za-z][\w:.]*)\s*=\s*\"([^\"]*)\"")


def load_pages():
    """Extract the xaml snippets from the data files without a JS engine."""
    pages = []
    for f in ("legacy.js", "guides.js", "examples.js", "web.js", "custom.js", "basic.js", "enrich.js"):
        path = os.path.join(DOCS, "data", f)
        src = open(path, encoding="utf-8").read()
        for m in re.finditer(r'xaml: `(.*?)`', src, re.S):
            start = src.rfind("\n    ", 0, m.start())
            head = src[start:m.start()]
            pid = re.findall(r"\n    ([a-z0-9\-]+): \{", src[:m.start()])
            pages.append((pid[-1] if pid else f, m.group(1)))
    return pages


def main():
    problems = []
    tags_seen = set()
    for page, xaml in load_pages():
        for m in TAG_RE.finditer(xaml):
            prefix, tag, attrs = m.group(1), m.group(2), m.group(3)
            if prefix in ("http", "https"):
                continue
            info = CLASSES.get(tag)
            if info is None and tag in ALL_CLASSES:
                info = {"props": [], "events": [], "namespace": ""}
            if tag in ("Application", "Window", "Grid", "StackPanel", "Border", "TextBlock", "Button",
                       "TabItem", "MenuItem", "Separator", "ListView", "GridView", "GridViewColumn",
                       "TreeView", "ScrollViewer", "ComboBox", "CheckBox", "RadioButton", "Slider",
                       "ProgressBar", "ListBox", "GroupBox", "Menu", "DataGrid", "ContextMenu"):
                info = None  # framework control: attributes checked against COMMON only
            elif info is None:
                problems.append(f"[{page}] unknown control tag <{prefix}:{tag}>")
                continue

            tags_seen.add(tag)
            known = inherited(tag) if info else set()
            for am in ATTR_RE.finditer(attrs):
                attr = am.group(1)
                if attr in COMMON or attr.startswith(("x:", "xmlns")):
                    continue
                if attr.startswith("attached:"):
                    cls, _, prop = attr[len("attached:"):].partition(".")
                    if cls in ATTACHED and prop not in ATTACHED[cls]:
                        problems.append(f"[{page}] <{tag}> attached property {attr} not found")
                    elif cls not in ATTACHED:
                        problems.append(f"[{page}] <{tag}> unknown attached class {cls}")
                    continue
                if attr in known:
                    continue
                # inherited/other framework props on our custom controls
                if info and attr in known:
                    continue
                problems.append(f"[{page}] <{tag}> attribute '{attr}' is not a property of {tag}")

    uniq = sorted(set(problems))
    print(f"scanned tags: {len(tags_seen)} | issues: {len(uniq)}")
    for p in uniq:
        print(" ", p)


if __name__ == "__main__":
    main()
