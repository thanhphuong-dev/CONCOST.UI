// English overlay — input & form pages.
Object.assign(DOCS_EN.pages, {

    numericupdown: {
        title: "NumericUpDown — precise numeric input",
        desc: "Numeric input with stepper buttons, step size, min/max limits and engineering unit formatting (mm, m³, kN, MPa).",
        sections: [
            { title: "XAML example", desc: "Concrete slab thickness input with limits and a millimetre suffix:" }
        ]
    },

    textbox: {
        title: "TextBox &amp; PasswordBox",
        desc: "Text input with placeholder, title, required marker, prefix/suffix, quick-clear button, character count and typed input filtering.",
        variants: [
            { desc: "Standard input with a 1px border." },
            { desc: "Sunken background, no border." },
            { desc: "Bottom rule only — fits toolbars." },
            { desc: "2px underline while focused." },
            { desc: "Fully rounded corners." },
            { desc: "Height 28px / 40px." },
            { desc: "Matching variant set for PasswordBox." }
        ],
        sections: [
            { title: "XAML example", desc: "Bind the text and drive validation through the attached metadata." }
        ]
    },

    passwordinput: {
        title: "PasswordInput — password field",
        desc: "Password input with a show/hide toggle, lock icon and placeholder — replaces hand-built PasswordBox + ToggleButton combinations.",
        sections: [
            { title: "Sign-in form", desc: "<code>Password</code> is two-way bound. With <code>ShowPassword=\"True\"</code> the content is rendered as plain text." }
        ]
    },

    otpinput: {
        title: "OtpInput — multi-cell verification code",
        desc: "Multi-cell one-time-code input that advances automatically and exposes the joined value — for two-factor sign-in and licence activation.",
        sections: [
            { title: "Six-digit verification code", desc: "<code>Length</code> sets the number of cells; <code>Value</code> is the joined string, two-way bound." }
        ]
    },

    pinbox: {
        title: "PinBox — PIN / OTP entry",
        desc: "Fixed-length PIN entry with optional masking, per-cell sizing and spacing — used for licence codes and access PINs.",
        sections: [
            { title: "XAML example", desc: "Set <code>Length</code>, then bind <code>Password</code> (or read the typed value) to submit." }
        ]
    },

    checkcombobox: {
        title: "CheckComboBox — multi-select dropdown",
        desc: "Dropdown with check boxes per row, a select-all entry, search and a compact preview of the current selection.",
        sections: [
            { title: "XAML syntax", desc: "Bind <code>ItemsSource</code>; <code>DisplayText</code> summarises the selected items for you." }
        ]
    },

    combobox: {
        title: "ComboBox — dropdown list",
        desc: "Dropdown styled to match the rest of the library, with placeholder, title and required marker through attached properties.",
        variants: [
            { desc: "Standard, with border and chevron." },
            { desc: "Sunken background." },
            { desc: "Fully rounded corners." },
            { desc: "Height 28px / 40px." }
        ],
        sections: [
            { title: "XAML example", desc: "Bind <code>ItemsSource</code> and <code>SelectedValue</code> to a lookup list." }
        ]
    },

    autocompletetextbox: {
        title: "AutoCompleteTextBox — type-ahead input",
        desc: "Text input that suggests matching items while typing, with a templated dropdown and keyboard selection.",
        sections: [
            { title: "1. XAML usage", desc: "Provide <code>ItemsSource</code> and <code>DisplayMemberPath</code>; filtering is handled for you." }
        ]
    },

    searchbox: {
        title: "SearchBox — smart search field",
        desc: "Search field with a leading magnifier icon, quick-clear button and an optional instant-search mode.",
        sections: [
            { title: "XAML example", desc: "Use <code>IsInstantSearch</code> to raise the search event on every keystroke, or bind <code>SearchCommand</code>." }
        ]
    },

    filepicker: {
        title: "FilePicker — file &amp; folder selection",
        desc: "Compact file/folder picker with a filter, dialog title and the selected path bound back to the view model.",
        sections: [
            { title: "XAML example", desc: "Restrict the dialog to drawing files with a Win32 filter string." }
        ]
    },

    colorpicker: {
        title: "ColorPicker — colour selection",
        desc: "Full colour picker with hex entry, optional alpha channel and two-way binding of the selected colour.",
        sections: [
            { title: "XAML example", desc: "Bind <code>SelectedColor</code>; read <code>HexCode</code> when you need a string for export." }
        ]
    },

    rangeslider: {
        title: "RangeSlider — range selection",
        desc: "Two-thumb slider for selecting a minimum/maximum interval, with a highlighted track between the thumbs.",
        sections: [
            { title: "1. XAML usage", desc: "Bind <code>Value1</code> and <code>Value2</code> to the lower and upper bounds." }
        ]
    },

    rating: {
        title: "Rating — star rating",
        desc: "Interactive star rating for contractor quality or inspection scoring, with an optional numeric score.",
        sections: [
            { title: "XAML example", desc: "Set <code>Max</code> for the scale and <code>ShowScore</code> to display the numeric value." }
        ]
    },

    tag: {
        title: "Tag / Chip — status labels",
        desc: "Compact label for element classification and status, with an optional icon and close button.",
        sections: [
            { title: "XAML example", desc: "Mark an inspection result as accepted and let the user dismiss the chip." }
        ]
    },

    toggleswitch: {
        title: "ToggleSwitch — on/off switch",
        desc: "Modern on/off switch with optional content for each state, used for viewport and visibility settings.",
        sections: [
            { title: "XAML example", desc: "Bind <code>IsChecked</code> and optionally set <code>OnContent</code> / <code>OffContent</code>." }
        ]
    }
});
