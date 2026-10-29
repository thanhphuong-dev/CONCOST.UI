// CONCOST.UI Interactive Documentation Application Logic
document.addEventListener("DOMContentLoaded", () => {
    const mainContent = document.getElementById("mainContent");
    const searchInput = document.getElementById("searchInput");
    const themeToggleBtn = document.getElementById("themeToggleBtn");
    const toast = document.getElementById("toastNotification");
    const engineeringControlsMenu = document.getElementById("engineeringControlsMenu");
    const customControlsMenu = document.getElementById("customControlsMenu");
    const basicControlsMenu = document.getElementById("basicControlsMenu");

    // Full List of Engineering & CAD Controls (CONCOST Flagship)
    const engineeringControls = [
        { id: "spreadsheet", name: "Spreadsheet", icon: "fa-table-cells", desc: "Bảng tính Excel kỹ thuật cao cấp, công thức SUM/AVERAGE, căn lề, xuất/nhập CSV" },
        { id: "stackedheaderdatagrid", name: "StackedHeaderDataGrid", icon: "fa-table-columns", desc: "Bảng dữ liệu tiêu chuẩn xây dựng với header đa tầng và chân trang tổng hợp" },
        { id: "cadcanvas", name: "ZoomableCanvas & Ruler", icon: "fa-ruler-combined", desc: "Không gian đồ họa CAD 2D thu phóng vô hạn kèm thước đo mm/m" },
        { id: "diagramchart", name: "DiagramChart", icon: "fa-chart-area", desc: "Biểu đồ nội lực kỹ thuật (Mô-men uốn M, Lực cắt Q, Lực dọc N) có đảo trục" },
        { id: "rebarinput", name: "RebarInput", icon: "fa-cubes-stacked", desc: "Hộp nhập cú pháp cốt thép thông minh, tự động tính diện tích As (mm²)" },
        { id: "engineeringboxes", name: "Engineering Boxes", icon: "fa-screwdriver-wrench", desc: "Bộ 6 hộp nhập đại lượng kỹ thuật: LengthBox, ForceBox, MomentBox, v.v." },
        { id: "compassindicator", name: "CompassIndicator", icon: "fa-compass", desc: "La bàn chỉ hướng Bắc địa chính và góc xoay công trình" }
    ];

    // Full List of Custom Controls (CONCOST Engineering Suite)
    const customControls = [
        { id: "cardcarousel", name: "CardCarousel", icon: "fa-cubes", desc: "Vòng xoay thẻ 3D phối cảnh chiều sâu" },
        { id: "timeline", name: "Timeline", icon: "fa-timeline", desc: "Dòng thời gian sự kiện & tiến độ thi công" },
        { id: "dialog", name: "Dialog", icon: "fa-window-maximize", desc: "Hộp thoại popup modal làm mờ nền" },
        { id: "drawer", name: "Drawer", icon: "fa-bars-staggered", desc: "Ngăn kéo trượt cấu hình đối tượng" },
        { id: "growl", name: "Growl (Toast)", icon: "fa-bell", desc: "Hệ thống thông báo nổi góc màn hình" },
        { id: "propertygrid", name: "PropertyGrid", icon: "fa-sliders", desc: "Thanh thuộc tính đối tượng BIM/CAD" },
        { id: "numericupdown", name: "NumericUpDown", icon: "fa-arrow-up-1-9", desc: "Ô nhập số kỹ thuật độ chính xác cao" },
        { id: "charts", name: "Charts Suite", icon: "fa-chart-pie", desc: "Bộ biểu đồ vector Bar, Line, Pie, Radar" },
        { id: "stepbar", name: "StepBar", icon: "fa-list-ol", desc: "Quy trình các bước thực hiện tuần tự" },
        { id: "transfer", name: "Transfer", icon: "fa-right-left", desc: "Hộp chuyển danh sách 2 chiều" },
        { id: "checkcombobox", name: "CheckComboBox", icon: "fa-square-check", desc: "Dropdown chọn nhiều dòng có CheckBox" },
        { id: "pinbox", name: "PinBox", icon: "fa-key", desc: "Ô nhập mã PIN bản quyền / OTP" },
        { id: "colorpicker", name: "ColorPicker", icon: "fa-palette", desc: "Bộ chọn màu chuyên nghiệp toàn diện" },
        { id: "clock", name: "Clock & Calendar", icon: "fa-clock", desc: "Đồng hồ tương tác & Lịch ngày giờ" },
        { id: "sidemenu", name: "SideMenu", icon: "fa-bars", desc: "Thanh menu điều hướng dọc ứng dụng" },
        { id: "drawermenu", name: "DrawerMenu", icon: "fa-table-columns", desc: "Menu ngăn kéo thu phóng động chuẩn WPFDevelopers" },
        { id: "snowcanvas", name: "SnowCanvas", icon: "fa-snowflake", desc: "Hiệu ứng tuyết rơi hạt GPU mượt mà" },
        { id: "sprite", name: "Sprite", icon: "fa-circle-dot", desc: "Trợ lý nổi kéo thả hít viền chuẩn HandyControl" },
        { id: "compareslider", name: "CompareSlider", icon: "fa-arrows-left-right", desc: "Thanh trượt kéo so sánh 2 ảnh Before / After" },
        { id: "magnifier", name: "Magnifier", icon: "fa-magnifying-glass-plus", desc: "Kính lúp phóng to điểm ảnh soi bản vẽ CAD/BIM" },
        { id: "rangeslider", name: "RangeSlider", icon: "fa-sliders", desc: "Thanh trượt 2 đầu chọn khoảng giá trị Min-Max" },
        { id: "autocompletetextbox", name: "AutoCompleteTextBox", icon: "fa-list-check", desc: "Ô tìm kiếm gợi ý danh mục tự động" },
        { id: "searchbox", name: "SearchBox", icon: "fa-magnifying-glass", desc: "Ô tìm kiếm thông minh có nút xóa nhanh" },
        { id: "tag", name: "Tag / Chip", icon: "fa-tag", desc: "Nhãn trạng thái & phân loại cấu kiện" },
        { id: "pagination", name: "Pagination", icon: "fa-file-lines", desc: "Bộ điều khiển phân trang dữ liệu" },
        { id: "filepicker", name: "FilePicker", icon: "fa-folder-open", desc: "Hộp chọn file bản vẽ & thư mục" },
        { id: "breadcrumb", name: "Breadcrumb", icon: "fa-chevron-right", desc: "Đường dẫn phân cấp vị trí mô hình" },
        { id: "statisticcard", name: "StatisticCard (KPI)", icon: "fa-chart-line", desc: "Thẻ chỉ số tiến độ & khối lượng" },
        { id: "rating", name: "Rating", icon: "fa-star", desc: "Bộ đánh giá xếp hạng sao" },
        { id: "watermark", name: "Watermark", icon: "fa-copyright", desc: "Hình mờ bản quyền & trạng thái sơ bộ" },
        { id: "emptystate", name: "EmptyState", icon: "fa-inbox", desc: "Giao diện minh họa trạng thái trống" },
        { id: "divider", name: "Divider", icon: "fa-minus", desc: "Đường kẻ phân cách bố cục" },
        { id: "icons", name: "Icons Gallery", icon: "fa-icons", desc: "Thư viện 120+ vector icons kỹ thuật" }
    ];

    // Full List of Basic & Enhanced Controls
    const basicControls = [
        { id: "button", name: "Button", icon: "fa-square-check", desc: "Nút bấm đa trạng thái & loading spinner" },
        { id: "togglebutton", name: "ToggleButton & CheckBox", icon: "fa-toggle-on", desc: "Nút chuyển & hộp kiểm nhiều mục" },
        { id: "toggleswitch", name: "ToggleSwitch", icon: "fa-sliders", desc: "Công tắc gạt On/Off hiện đại" },
        { id: "textbox", name: "TextBox & PasswordBox", icon: "fa-pen-to-square", desc: "Ô nhập văn bản & mật khẩu an toàn" },
        { id: "combobox", name: "ComboBox", icon: "fa-caret-down", desc: "Menu chọn thả xuống dropdown" },
        { id: "datagrid", name: "DataGrid & ListView", icon: "fa-table", desc: "Bảng dữ liệu sọc vằn bóc tách khối lượng" },
        { id: "treeview", name: "TreeView", icon: "fa-folder-tree", desc: "Cây phân cấp dự án & đối tượng" },
        { id: "tabcontrol", name: "TabControl", icon: "fa-folder", desc: "Thanh tab chuyển đổi phân hệ" },
        { id: "card", name: "Card & Border & Badge", icon: "fa-id-card", desc: "Khung chứa chuẩn hóa & huy hiệu số lượng" },
        { id: "progressbar", name: "ProgressBar & Slider", icon: "fa-bars-progress", desc: "Thanh tiến trình & thanh trượt giá trị" }
    ];

    // Populate Sidebar
    function renderSidebar() {
        if (engineeringControlsMenu) {
            engineeringControls.forEach(ctrl => {
                const li = document.createElement("li");
                li.className = "nav-item";
                li.dataset.target = ctrl.id;
                li.innerHTML = `<i class="fa-solid ${ctrl.icon}"></i> ${ctrl.name}`;
                li.addEventListener("click", () => switchTab(ctrl.id));
                engineeringControlsMenu.appendChild(li);
            });
        }

        customControls.forEach(ctrl => {
            const li = document.createElement("li");
            li.className = "nav-item";
            li.dataset.target = ctrl.id;
            li.innerHTML = `<i class="fa-solid ${ctrl.icon}"></i> ${ctrl.name}`;
            li.addEventListener("click", () => switchTab(ctrl.id));
            customControlsMenu.appendChild(li);
        });

        basicControls.forEach(ctrl => {
            const li = document.createElement("li");
            li.className = "nav-item";
            li.dataset.target = ctrl.id;
            li.innerHTML = `<i class="fa-solid ${ctrl.icon}"></i> ${ctrl.name}`;
            li.addEventListener("click", () => switchTab(ctrl.id));
            basicControlsMenu.appendChild(li);
        });
    }

    // Switch active documentation tab
    window.switchTab = function(targetId) {
        document.querySelectorAll(".nav-item").forEach(el => {
            el.classList.toggle("active", el.dataset.target === targetId);
        });

        const data = DOCS_DATA[targetId];
        if (!data) return;

        let html = `
            <div class="doc-header">
                <span class="doc-badge">${data.badge}</span>
                <h1 class="doc-title">${data.title}</h1>
                <p class="doc-desc">${data.desc}</p>
            </div>
        `;

        // Render Property table if exists
        if (data.props && data.props.length > 0) {
            html += `
                <div class="doc-card">
                    <div class="doc-card-title"><i class="fa-solid fa-sliders" style="color:var(--primary);margin-right:8px;"></i> Bảng Thuộc Tính & Tham Số Cấu Hình (Properties)</div>
                    <table class="props-table">
                        <thead>
                            <tr>
                                <th>Thuộc Tính (Property)</th>
                                <th>Kiểu Dữ Liệu</th>
                                <th>Mặc Định</th>
                                <th>Mô Tả Chức Năng</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${data.props.map(p => `
                                <tr>
                                    <td><code>${p.name}</code></td>
                                    <td><em>${p.type}</em></td>
                                    <td><code>${p.default}</code></td>
                                    <td>${p.desc}</td>
                                </tr>
                            `).join("")}
                        </tbody>
                    </table>
                </div>
            `;
        }

        // Render Sections with Code Inspector
        if (data.sections) {
            data.sections.forEach((sec, idx) => {
                html += `
                    <div class="doc-card">
                        <div class="doc-card-title">${sec.title}</div>
                        <p class="doc-card-desc">${sec.desc}</p>
                `;

                if (sec.table) {
                    html += `
                        <table class="props-table" style="margin-bottom:18px;">
                            <thead>
                                <tr>
                                    <th>Tên Token Resource</th>
                                    <th>Mục Đích & Vị Trí Áp Dụng</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${sec.table.map(t => `
                                    <tr>
                                        <td><code>${t.token}</code></td>
                                        <td>${t.desc}</td>
                                    </tr>
                                `).join("")}
                            </tbody>
                        </table>
                    `;
                }

                if (sec.xaml || sec.csharp) {
                    html += `
                        <div class="code-inspector">
                            <div class="code-header">
                                <div class="code-tabs">
                                    ${sec.xaml ? `<button class="code-tab active" onclick="toggleCodeTab(this, 'xaml-${idx}')"><i class="fa-solid fa-code"></i> XAML Markup</button>` : ""}
                                    ${sec.csharp ? `<button class="code-tab ${!sec.xaml ? "active" : ""}" onclick="toggleCodeTab(this, 'csharp-${idx}')"><i class="fa-solid fa-file-code"></i> C# Code-Behind</button>` : ""}
                                </div>
                                <button class="btn-copy" onclick="copyCode(this)"><i class="fa-regular fa-copy"></i> Sao chép</button>
                            </div>
                            ${sec.xaml ? `<pre id="xaml-${idx}" class="language-markup"><code>${escapeHtml(sec.xaml)}</code></pre>` : ""}
                            ${sec.csharp ? `<pre id="csharp-${idx}" class="language-csharp" style="${sec.xaml ? "display:none;" : ""}"><code>${escapeHtml(sec.csharp)}</code></pre>` : ""}
                        </div>
                    `;
                }

                html += `</div>`;
            });
        }

        mainContent.innerHTML = html;
        Prism.highlightAllUnder(mainContent);
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    // Helper functions
    function escapeHtml(str) {
        return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    window.toggleCodeTab = function(btn, targetPreId) {
        const inspector = btn.closest(".code-inspector");
        inspector.querySelectorAll(".code-tab").forEach(t => t.classList.remove("active"));
        btn.classList.add("active");

        inspector.querySelectorAll("pre").forEach(p => p.style.display = "none");
        const targetPre = inspector.querySelector(`#${targetPreId}`);
        if (targetPre) targetPre.style.display = "block";
    };

    window.copyCode = function(btn) {
        const inspector = btn.closest(".code-inspector");
        const activePre = Array.from(inspector.querySelectorAll("pre")).find(p => p.style.display !== "none");
        if (activePre) {
            navigator.clipboard.writeText(activePre.innerText).then(() => {
                showToast();
            });
        }
    };

    function showToast() {
        toast.classList.add("show");
        setTimeout(() => toast.classList.remove("show"), 2400);
    }

    // Theme switching
    themeToggleBtn.addEventListener("click", () => {
        const isDark = document.documentElement.getAttribute("data-theme") === "dark";
        const nextTheme = isDark ? "light" : "dark";
        document.documentElement.setAttribute("data-theme", nextTheme);
        themeToggleBtn.innerHTML = nextTheme === "dark" ? '<i class="fa-solid fa-sun"></i>' : '<i class="fa-solid fa-moon"></i>';
    });

    // Palette dots
    document.querySelectorAll(".palette-dot").forEach(dot => {
        dot.addEventListener("click", () => {
            document.querySelectorAll(".palette-dot").forEach(d => d.classList.remove("active"));
            dot.classList.add("active");
            const color = dot.dataset.color;
            document.documentElement.style.setProperty("--primary", color);
            document.documentElement.style.setProperty("--primary-light", color + "1a");
            document.documentElement.style.setProperty("--primary-glow", color + "40");
        });
    });

    // Fast search filter (Ctrl+K or input)
    searchInput.addEventListener("input", (e) => {
        const q = e.target.value.toLowerCase().trim();
        document.querySelectorAll(".nav-item").forEach(item => {
            const text = item.innerText.toLowerCase();
            item.style.display = text.includes(q) ? "flex" : "none";
        });
    });

    window.addEventListener("keydown", (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
            e.preventDefault();
            searchInput.focus();
        }
    });

    // Init
    renderSidebar();
    document.querySelectorAll(".sidebar .nav-item").forEach(el => {
        el.addEventListener("click", () => switchTab(el.dataset.target));
    });

    // Open first tab by default
    switchTab("intro");
});
