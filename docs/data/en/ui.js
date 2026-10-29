// Bilingual layer. UI chrome strings + the container every en/*.js file fills.
const DOCS_UI = {
    vi: {
        searchPlaceholder: "Tìm control, token, hướng dẫn…",
        tocTitle: "Trong trang này",
        propsTitle: "API tham chiếu — {n} thuộc tính",
        propsDesc: "Các <code>DependencyProperty</code> dưới đây được trích trực tiếp từ source của <code>{cls}</code>.",
        variantsTitle: "Style variants có sẵn",
        variantsDesc: "Áp dụng qua <code>Style=\"{DynamicResource &lt;key&gt;}\"</code> — toàn bộ key dưới đây có trong <code>Themes/Generic.xaml</code>.",
        relatedTitle: "Xem thêm",
        prev: "Trước",
        next: "Tiếp",
        chipProps: "properties",
        chipVariants: "style variants",
        cmdkPlaceholder: "Tìm control, thuộc tính, style key…",
        cmdkEmpty: "Không tìm thấy kết quả phù hợp",
        cmdkCount: "{n} kết quả",
        cmdkNav: "di chuyển",
        cmdkOpen: "mở",
        toastCopied: "Đã sao chép vào clipboard",
        copy: "Sao chép",
        copied: "Đã chép",
        pagesCount: "{n} trang tài liệu",
        themeToggle: "Sáng / Tối",
        langToggle: "Đổi ngôn ngữ",
        notFoundTitle: "Không tìm thấy trang",
        notFoundDesc: "Trang <code>{id}</code> chưa có trong bộ tài liệu.",
        tocPage: "Trang",
        tocGroup: "Nhóm",
        typeHeader: "Kiểu",
        defaultHeader: "Mặc định",
        descHeader: "Mô tả",
        propHeader: "Thuộc tính",
        tokenHeader: "Token / API",
        tokenDescHeader: "Mục đích &amp; vị trí áp dụng"
    },
    en: {
        searchPlaceholder: "Search controls, tokens, guides…",
        tocTitle: "On this page",
        propsTitle: "API reference — {n} properties",
        propsDesc: "The <code>DependencyProperty</code> list below is extracted straight from the <code>{cls}</code> source.",
        variantsTitle: "Available style variants",
        variantsDesc: "Apply with <code>Style=\"{DynamicResource &lt;key&gt;}\"</code> — every key below exists in <code>Themes/Generic.xaml</code>.",
        relatedTitle: "Related",
        prev: "Previous",
        next: "Next",
        chipProps: "properties",
        chipVariants: "style variants",
        cmdkPlaceholder: "Search controls, properties, style keys…",
        cmdkEmpty: "No matching results",
        cmdkCount: "{n} results",
        cmdkNav: "to move",
        cmdkOpen: "to open",
        toastCopied: "Copied to clipboard",
        copy: "Copy",
        copied: "Copied",
        pagesCount: "{n} documentation pages",
        themeToggle: "Light / Dark",
        langToggle: "Switch language",
        notFoundTitle: "Page not found",
        notFoundDesc: "The page <code>{id}</code> is not part of this documentation.",
        tocPage: "Page",
        tocGroup: "Group",
        typeHeader: "Type",
        defaultHeader: "Default",
        descHeader: "Description",
        propHeader: "Property",
        tokenHeader: "Token / API",
        tokenDescHeader: "Purpose &amp; where to use it"
    }
};

// Vietnamese badge labels that appear in the page headers.
const DOCS_BADGE_EN = {
    "Bắt Đầu Nhanh": "Quick start",
    "Hỗ trợ": "Support",
    "Hướng dẫn": "Guide",
    "Engineering Flagship": "Engineering Flagship",
    "Dashboard Suite": "Dashboard Suite",
    "Custom Controls": "Custom Controls",
    "Dữ liệu & Dashboard": "Data & Dashboard",
    "Nhập liệu & Form": "Input & Form",
    "Điều hướng": "Navigation",
    "Phản hồi & Trạng thái": "Feedback & Status",
    "Layout & Shell": "Layout & Shell",
    "Hiệu ứng & Trang trí": "Effects & Decoration",
    "Controls cơ bản": "Basic Controls",
    "Thời gian": "Time"
};

// Every en/*.js file registers its pages here.
const DOCS_EN = { pages: {} };

// The datasets are top-level consts, so expose them for app.js.
window.DOCS_UI = DOCS_UI;
window.DOCS_EN = DOCS_EN;
window.DOCS_EN_PROPS = typeof DOCS_EN_PROPS !== "undefined" ? DOCS_EN_PROPS : {};
window.DOCS_BADGE_EN = DOCS_BADGE_EN;
