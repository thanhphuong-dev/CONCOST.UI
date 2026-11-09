"""
fill-summaries.py — one-shot helper that inserts type-level /// summaries for the public types the
ApiGen --check report lists. The summaries live in this table so a review can read them all in one
place before they land in the sources.

Usage: python docs/tools/fill-summaries.py
"""
import os
import re

SRC = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "src", "CONCOST.UI")

SUMMARIES = {
    "AttachedProperties/DragDropHelper.cs": {
        "DragDropHelper": "Attached drag-and-drop plumbing for the library's list surfaces: the drop zones, the drag ghost and the highlight a host turns on per element.",
    },
    "AttachedProperties/ScrollViewerHelper.cs": {
        "ScrollViewerHelper": "Attached behaviour that upgrades any ScrollViewer with the library's chrome: back-to-top, smooth scrolling and the scrollbar styling hooks.",
    },
    "Controls/AggregateType.cs": {},
    "Controls/Badge.cs": {
        "BadgeAppearance": "The visual family of a badge: solid, soft or outline.",
    },
    "Controls/Breadcrumb.cs": {
        "Breadcrumb": "A path of links for deep navigation — projects, folders, view levels — with overflow collapsing and a separator the host can restyle.",
        "BreadcrumbItem": "One link on a breadcrumb bar: content, optional icon and the click hand-off.",
    },
    "Controls/CalendarWithClock.cs": {
        "CalendarWithClock": "A calendar and a radial clock in one panel, for picking a date and its time of day — acceptance dates, inspection windows, concrete pour times.",
        "ClockDisplayMode": "Which of the clock's three wheels the user is on: hours, minutes or seconds.",
    },
    "Controls/Card.cs": {
        "Card": "The standard content surface: a raised panel with a header, an optional accent edge and room for anything a host puts inside.",
    },
    "Controls/CheckComboBox.cs": {
        "CheckComboBox": "A drop-down whose items are checkboxes, for picking several values from one compact field; a summary line names what was picked.",
        "CheckComboBoxItem": "One selectable row inside a CheckComboBox.",
    },
    "Controls/Clock.cs": {
        "Clock": "The material-style radial clock — an interactive dial for hours, minutes and seconds with auto-advance between them.",
    },
    "Controls/ColorPicker.cs": {
        "ColorPicker": "A full HSV colour editor: a saturation-value canvas, hue and alpha strips, a hex field and the preset swatches, with the picked colour reported two ways.",
    },
    "Controls/ConditionalFormatting.cs": {
        "ComparisonOperator": "The comparison a conditional formatting rule applies to a cell value.",
        "ConditionalFormattingRule": "One rule of a conditional formatting set: a property path, a comparison and the look the matching cells take.",
        "DataBarWidthConverter": "Turns a cell value and its column range into a data-bar width for the formatting pass.",
    },
    "Controls/DataGridFeatures/StackedHeader.cs": {
        "CellType": "The specialised editor a stacked-header column renders: text, numeric, date, progress, badge, rating and their kin.",
        "ColumnGroup": "A titled group of columns in a stacked header tree; groups nest, so a schedule can carry two levels of headings.",
        "ColumnLeaf": "A real column at the bottom of a stacked header tree: its binding, its width, its cell type and its footer aggregate.",
        "ColumnNode": "The shared shape of a stacked-header tree node — either a group or a leaf.",
        "AggregateType": "The footer aggregate a stacked-header column shows: sum, average, count, min or max.",
    },
    "Controls/Dialog.cs": {
        "Dialog": "The modal card: a scale-in surface with a header, body and footer, shown over a dimmed backdrop and resolved by a Task the host awaits.",
        "DialogMessageType": "The four voices a dialog's icon and buttons take: plain, confirm, warning or danger.",
    },
    "Controls/Divider.cs": {
        "DividerLineStyle": "The stroke a divider draws: solid, dashed or dotted.",
    },
    "Controls/Drawer.cs": {
        "Drawer": "A sliding overlay panel that docks to one edge of its host over a dimmed backdrop — settings sheets, inspectors, filter trays.",
        "DrawerMenuItem": "One entry of a DrawerMenu: icon, caption and the badge some entries carry.",
        "DrawerPlacement": "Which edge a drawer docks to.",
    },
    "Controls/FilePicker.cs": {
        "FilePicker": "An input row and a browse button over the file dialog: picks one file, several files or a folder, and shows the result inline.",
    },
    "Controls/Growl.cs": {
        "Growl": "The toast stack: non-intrusive notifications that slide in at a screen edge, auto-dismiss and can carry an action or a confirmation.",
        "GrowlItem": "One toast in the stack: its severity, message, optional action and the countdown it lives by.",
        "GrowlType": "The severity a toast speaks with — info, success, warning, error, question or fatal.",
        "GrowlWindow": "The borderless host window that pins a toast stack to one corner of the screen.",
    },
    "Controls/Loadings.cs": {
        "LoadingBase": "The shared base of the loading family: the IsLoading switch every spinner template reads.",
        "CycleLoading": "A spinner whose dots cycle through the perimeter one after another.",
    },
    "Controls/DataGridFeatures/DataGridSupport.cs": {
        "DataGridCellTemplateFactory": "Builds the editing and display templates the rich cell types of a stacked-header grid render with.",
        "DataGridClipboardHelper": "Excel-shaped clipboard support for a DataGrid: copies the selection as TSV with headers, and pastes a block back into the cells.",
        "DataGridContextMenuHelper": "Attaches the standard grid context menu — copy, select all, clear filters — to any DataGrid.",
        "DataGridFilterHelper": "The predicate engine behind the filter row: one search box, one operator and the column type decide how a column is matched.",
        "DataPager": "The page strip under a large list: page size, page index, first/prev/next/last and the count it reports.",
        "DetailsVisibilityToBoolConverter": "Reads a row-details visibility as a bool for the toggle switches that drive it.",
    },
    "Controls/GridPattern.cs": {
        "GridPatternType": "The pattern a GridPattern overlay draws: graph paper, dots or lines.",
    },
}


def insert_before_type(source, type_name, summary):
    pattern = re.compile(
        r"(?m)^(?:/// .*\n)*"
        r"((?:\[[^\]]*\]\n)*)"
        r"(public\s+(?:(?:sealed|abstract|static|partial|readonly)\s+)*(?:class|enum|struct|interface)\s+"
        + re.escape(type_name) + r"\b)"
    )

    match = pattern.search(source)
    if match is None:
        return None

    if "/// <summary>" in match.group(0):
        return source  # already documented

    doc = f"/// <summary>\n/// {summary}\n/// </summary>\n"
    return source[:match.start()] + doc + match.group(1) + match.group(2) + source[match.end():]


def main():
    applied, missed = 0, []

    for relative, entries in SUMMARIES.items():
        path = os.path.join(SRC, relative.replace("/", os.sep))
        if not os.path.exists(path):
            missed.append(f"{relative}: file not found")
            continue

        with open(path, encoding="utf-8-sig") as handle:
            source = handle.read()

        for type_name, summary in entries.items():
            updated = insert_before_type(source, type_name, summary)
            if updated is None:
                missed.append(f"{relative}:{type_name}: declaration not found")
                continue

            source = updated
            applied += 1

        with open(path, "w", encoding="utf-8-sig") as handle:
            handle.write(source)

    print(f"summaries inserted: {applied}")
    for entry in missed:
        print("  missed:", entry)


if __name__ == "__main__":
    main()
