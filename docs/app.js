/* ==========================================================================
   CONCOST.UI Documentation Wiki — application shell
   Renders pages from DOCS_DATA / DOCS_AUTO / DOCS_NAV (see docs/data/*)
   ========================================================================== */
(function () {
    "use strict";

    const PAGES = window.DOCS_DATA || {};
    const AUTO = window.DOCS_AUTO || {};
    const NAV = window.DOCS_NAV || [];

    const $ = (sel, root) => (root || document).querySelector(sel);
    const $$ = (sel, root) => Array.from((root || document).querySelectorAll(sel));

    const el = {
        main: $("#mainContent"),
        toc: $("#toc"),
        nav: $("#sidebarNav"),
        sidebar: $("#sidebar"),
        overlay: $("#cmdkOverlay"),
        input: $("#cmdkInput"),
        results: $("#cmdkResults"),
        count: $("#cmdkCount"),
        toast: $("#toastNotification"),
        toastText: $("#toastText")
    };

    /* ---------- Helpers ---------- */
    const esc = (s) => String(s == null ? "" : s)
        .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");

    const escAttr = (s) => esc(s).replace(/'/g, "&#39;");

    let toastTimer;
    function toast(msg) {
        el.toastText.textContent = msg;
        el.toast.classList.add("show");
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => el.toast.classList.remove("show"), 2200);
    }

    /* ---------- Effective page model ---------- */
    function resolve(id) {
        const page = PAGES[id];
        if (!page) return null;
        const auto = AUTO[id] || {};
        const props = (page.props && page.props.length) ? page.props : (auto.props || []);
        const sections = (page.sections || []).map((s, i) => {
            if (i === 0 && !s.csharp) {
                const cs = page.csharp || auto.csharp;
                if (cs) return Object.assign({}, s, { csharp: cs });
            }
            return s;
        });
        return Object.assign({}, page, {
            id: id,
            props: props,
            sections: sections,
            className: page.className || auto.class || "",
            namespace: page.namespace || auto.namespace || "CONCOST.UI.Controls"
        });
    }

    const ORDER = [];
    const GROUP_OF = {};
    const ICON_OF = {};
    const NAME_OF = {};
    NAV.forEach(group => {
        (group.items || []).forEach(item => {
            ORDER.push(item.id);
            GROUP_OF[item.id] = group.title;
            ICON_OF[item.id] = item.icon || "fa-cube";
            NAME_OF[item.id] = item.name || (PAGES[item.id] && PAGES[item.id].title) || item.id;
        });
    });

    /* ---------- Sidebar ---------- */
    function renderSidebar() {
        let html = "";
        NAV.forEach(group => {
            html += `<div class="nav-category-title">${esc(group.title)}</div><ul class="nav-menu">`;
            (group.items || []).forEach(item => {
                const badge = item.badge ? `<span class="nav-badge">${esc(item.badge)}</span>` : "";
                html += `<li class="nav-item" data-target="${escAttr(item.id)}" title="${escAttr(item.name)}">
                    <i class="fa-solid ${escAttr(item.icon || "fa-cube")}"></i>
                    <span class="nav-label">${esc(item.name)}</span>${badge}
                </li>`;
            });
            html += `</ul>`;
        });
        el.nav.innerHTML = html;
        $("#footCount").textContent = `${ORDER.length} trang tài liệu`;
    }

    /* ---------- Blocks ---------- */
    function propsTable(props) {
        if (!props || !props.length) return "";
        const rows = props.map(p => {
            const ro = p.readonly ? `<span class="tag-ro">read-only</span>` : "";
            const att = p.attached ? `<span class="tag-ro">attached</span>` : "";
            const def = (p.default !== undefined && p.default !== "" && p.default !== null)
                ? `<span class="def" title="${escAttr(p.default)}">${esc(p.default)}</span>`
                : `<span style="color:var(--text-muted)">—</span>`;
            return `<tr>
                <td><code>${esc(p.name)}</code>${ro}${att}</td>
                <td><em>${esc(p.type)}</em></td>
                <td>${def}</td>
                <td>${p.desc || ""}</td>
            </tr>`;
        }).join("");
        const scrollable = props.length > 10 ? " scrollable" : "";
        return `<div class="table-wrap${scrollable}"><table class="props-table">
            <thead><tr><th>Thuộc tính</th><th>Kiểu</th><th>Mặc định</th><th>Mô tả</th></tr></thead>
            <tbody>${rows}</tbody></table></div>`;
    }

    function variantsBlock(variants) {
        if (!variants || !variants.length) return "";
        const cards = variants.map(v => `<div class="variant-card">
            <span class="vk">${esc(v.key)}</span>
            <span class="vd">${esc(v.desc)}</span>
        </div>`).join("");
        return `<div class="variant-grid">${cards}</div>`;
    }

    function tokenTable(table) {
        if (!table || !table.length) return "";
        const rows = table.map(t => `<tr><td><code>${esc(t.token)}</code></td><td>${t.desc || ""}</td></tr>`).join("");
        return `<div class="table-wrap"><table class="props-table">
            <thead><tr><th>Token / API</th><th>Mục đích &amp; vị trí áp dụng</th></tr></thead>
            <tbody>${rows}</tbody></table></div>`;
    }

    function callout(c) {
        if (!c) return "";
        const icons = { info: "fa-circle-info", tip: "fa-lightbulb", warn: "fa-triangle-exclamation", danger: "fa-circle-exclamation" };
        return `<div class="callout ${escAttr(c.type || "info")}">
            <i class="fa-solid ${icons[c.type] || icons.info}"></i>
            <div>${c.title ? `<b>${esc(c.title)}</b> — ` : ""}${c.body || ""}</div>
        </div>`;
    }

    function codeInspector(sec, idx) {
        if (!sec.xaml && !sec.csharp) return "";
        const tabs = [];
        if (sec.xaml) tabs.push(`<button class="code-tab active" data-pane="xaml-${idx}"><i class="fa-solid fa-code"></i> XAML</button>`);
        if (sec.csharp) tabs.push(`<button class="code-tab ${sec.xaml ? "" : "active"}" data-pane="csharp-${idx}"><i class="fa-solid fa-file-code"></i> C# Code-Behind</button>`);

        const xamlPane = sec.xaml
            ? `<pre id="xaml-${idx}" class="language-markup"><code>${esc(sec.xaml)}</code></pre>` : "";
        const csPane = sec.csharp
            ? `<pre id="csharp-${idx}" class="language-csharp"${sec.xaml ? ' style="display:none"' : ""}><code>${esc(sec.csharp)}</code></pre>` : "";

        return `<div class="code-inspector">
            <div class="code-header">
                <div class="code-tabs">${tabs.join("")}</div>
                <button class="btn-copy" data-copy><i class="fa-regular fa-copy"></i> Sao chép</button>
            </div>
            ${xamlPane}${csPane}
        </div>`;
    }

    function metaChips(page) {
        const chips = [];
        if (page.className) chips.push(`<span class="chip"><i class="fa-solid fa-cube"></i><b>${esc(page.className)}</b></span>`);
        if (page.namespace) chips.push(`<span class="chip"><i class="fa-solid fa-code"></i>${esc(page.namespace)}</span>`);
        if (page.props && page.props.length) chips.push(`<span class="chip"><i class="fa-solid fa-sliders"></i>${page.props.length} properties</span>`);
        if (page.variants && page.variants.length) chips.push(`<span class="chip"><i class="fa-solid fa-layer-group"></i>${page.variants.length} style variants</span>`);
        return chips.length ? `<div class="meta-row">${chips.join("")}</div>` : "";
    }

    /* ---------- Page renderer ---------- */
    function render(id, opts) {
        const page = resolve(id);
        if (!page) {
            el.main.innerHTML = `<div class="doc-header"><h1 class="doc-title">Không tìm thấy trang</h1>
                <p class="doc-desc">Trang <code>${esc(id)}</code> chưa có trong bộ tài liệu.</p></div>`;
            el.toc.innerHTML = "";
            return;
        }

        const pos = ORDER.indexOf(id);
        const prevId = ORDER[pos - 1];
        const nextId = ORDER[pos + 1];

        let html = "";

        if (page.hero) {
            const stats = (page.heroStats || []).map(s => `<div class="hero-stat">
                <div class="n">${esc(s.value)}</div><div class="l">${esc(s.label)}</div></div>`).join("");
            html += `<div class="hero">
                <h1>${page.title}</h1>
                <p>${page.desc}</p>
                ${stats ? `<div class="hero-stats">${stats}</div>` : ""}
            </div>`;
        } else {
            html += `<div class="doc-header">
                ${page.badge ? `<span class="doc-badge">${esc(page.badge)}</span>` : ""}
                <h1 class="doc-title">${page.title}</h1>
                ${page.desc ? `<p class="doc-desc">${page.desc}</p>` : ""}
                ${metaChips(page)}
            </div>`;
        }

        if (page.quick && page.quick.length) {
            html += `<div class="quick-grid">` + page.quick.map(q => `<div class="quick-card" data-target="${escAttr(q.id)}">
                <div class="qc-ico"><i class="fa-solid ${escAttr(q.icon || "fa-cube")}"></i></div>
                <div class="qc-t">${esc(q.title)}</div>
                <div class="qc-d">${esc(q.desc || "")}</div>
            </div>`).join("") + `</div>`;
        }

        if (page.props && page.props.length) {
            html += `<div class="doc-card" id="sec-props">
                <div class="doc-card-title"><span class="num"><i class="fa-solid fa-sliders" style="font-size:9px"></i></span> API tham chiếu — ${page.props.length} thuộc tính</div>
                <p class="doc-card-desc">Các <code>DependencyProperty</code> dưới đây được trích trực tiếp từ source của <code>${esc(page.className || id)}</code>.</p>
                ${propsTable(page.props)}
            </div>`;
        }

        if (page.variants && page.variants.length) {
            html += `<div class="doc-card" id="sec-variants">
                <div class="doc-card-title"><span class="num"><i class="fa-solid fa-layer-group" style="font-size:9px"></i></span> Style variants có sẵn</div>
                <p class="doc-card-desc">Áp dụng qua <code>Style="{DynamicResource &lt;key&gt;}"</code> — toàn bộ key dưới đây có trong <code>Themes/Generic.xaml</code>.</p>
                ${variantsBlock(page.variants)}
            </div>`;
        }

        (page.sections || []).forEach((sec, i) => {
            html += `<div class="doc-card" id="sec-${i}">
                <div class="doc-card-title"><span class="num">${i + 1}</span> ${sec.title}</div>
                ${sec.desc ? `<p class="doc-card-desc">${sec.desc}</p>` : ""}
                ${callout(sec.callout)}
                ${tokenTable(sec.table)}
                ${codeInspector(sec, i)}
            </div>`;
        });

        if (page.related && page.related.length) {
            const links = page.related.filter(r => PAGES[r]).map(r =>
                `<a data-target="${escAttr(r)}"><i class="fa-solid fa-arrow-right"></i> ${esc(NAME_OF[r] || r)}</a>`).join("");
            if (links) html += `<div class="doc-card"><div class="doc-card-title" style="font-size:13px">Xem thêm</div><div class="related">${links}</div></div>`;
        }

        html += `<div class="doc-pager">
            ${prevId ? `<div class="pager-link prev" data-target="${escAttr(prevId)}">
                <span class="pl"><i class="fa-solid fa-arrow-left"></i> Trước</span>
                <span class="pt">${esc(NAME_OF[prevId])}</span></div>` : `<div class="pager-link empty"></div>`}
            ${nextId ? `<div class="pager-link next" data-target="${escAttr(nextId)}">
                <span class="pl">Tiếp <i class="fa-solid fa-arrow-right"></i></span>
                <span class="pt">${esc(NAME_OF[nextId])}</span></div>` : `<div class="pager-link empty"></div>`}
        </div>`;

        el.main.innerHTML = html;
        renderToc(page);
        if (window.Prism) Prism.highlightAllUnder(el.main);

        $$(".nav-item", el.nav).forEach(n => n.classList.toggle("active", n.dataset.target === id));
        document.title = `${String(page.title).replace(/<[^>]+>/g, "")} — CONCOST.UI`;

        if (!opts || !opts.keepScroll) window.scrollTo({ top: 0 });
        el.sidebar.classList.remove("open");
    }

    /* ---------- TOC ---------- */
    function renderToc(page) {
        const items = [];
        if (page.props && page.props.length) items.push({ id: "sec-props", label: "API tham chiếu" });
        if (page.variants && page.variants.length) items.push({ id: "sec-variants", label: "Style variants" });
        (page.sections || []).forEach((s, i) => items.push({ id: `sec-${i}`, label: s.title }));

        if (!items.length) { el.toc.innerHTML = ""; return; }

        el.toc.innerHTML = `<div class="toc-title">Trong trang này</div>` +
            items.map(it => `<a data-scroll="${escAttr(it.id)}">${esc(it.label)}</a>`).join("") +
            `<div class="toc-stats">
                <div>Trang <b>${ORDER.indexOf(page.id) + 1}</b>/${ORDER.length}</div>
                <div>Nhóm <b>${esc(GROUP_OF[page.id] || "—")}</b></div>
            </div>`;

        setupScrollSpy();
    }

    let spyObserver;
    function setupScrollSpy() {
        if (spyObserver) spyObserver.disconnect();
        const links = $$("a[data-scroll]", el.toc);
        if (!links.length) return;
        const map = new Map();
        links.forEach(l => map.set(l.dataset.scroll, l));

        spyObserver = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    links.forEach(l => l.classList.remove("active"));
                    const link = map.get(entry.target.id);
                    if (link) link.classList.add("active");
                }
            });
        }, { rootMargin: "-72px 0px -68% 0px", threshold: 0 });

        map.forEach((_, id) => {
            const node = document.getElementById(id);
            if (node) spyObserver.observe(node);
        });
    }

    /* ---------- Command palette ---------- */
    const searchIndex = ORDER.map(id => {
        const p = PAGES[id] || {};
        const auto = AUTO[id] || {};
        const props = (p.props || auto.props || []).map(x => x.name).join(" ");
        return {
            id: id,
            name: NAME_OF[id],
            group: GROUP_OF[id] || "",
            icon: ICON_OF[id] || "fa-cube",
            desc: String(p.desc || "").replace(/<[^>]+>/g, ""),
            haystack: [NAME_OF[id], id, GROUP_OF[id], String(p.desc || "").replace(/<[^>]+>/g, ""), props, (p.keywords || []).join(" ")]
                .join(" ").toLowerCase()
        };
    });

    let cmdkResults = [];
    let cmdkIndex = 0;

    function openPalette() {
        el.overlay.classList.add("open");
        el.input.value = "";
        runSearch("");
        setTimeout(() => el.input.focus(), 30);
    }

    function closePalette() {
        el.overlay.classList.remove("open");
        el.input.blur();
    }

    function runSearch(q) {
        q = q.trim().toLowerCase();
        const terms = q.split(/\s+/).filter(Boolean);

        cmdkResults = searchIndex
            .map(item => {
                if (!terms.length) return { item: item, score: 0 };
                let score = 0;
                for (const t of terms) {
                    if (item.haystack.indexOf(t) === -1) return null;
                    if (item.name.toLowerCase().startsWith(t)) score += 60;
                    else if (item.name.toLowerCase().indexOf(t) !== -1) score += 30;
                    else score += 8;
                }
                return { item: item, score: score };
            })
            .filter(Boolean)
            .sort((a, b) => b.score - a.score || a.item.name.localeCompare(b.item.name))
            .slice(0, 40)
            .map(x => x.item);

        cmdkIndex = 0;
        paintResults(terms);
    }

    function paintResults(terms) {
        if (!cmdkResults.length) {
            el.results.innerHTML = `<div class="cmdk-empty"><i class="fa-solid fa-magnifying-glass"></i><br>Không tìm thấy kết quả phù hợp</div>`;
            el.count.textContent = "0 kết quả";
            return;
        }
        el.results.innerHTML = cmdkResults.map((r, i) => {
            let name = esc(r.name);
            (terms || []).forEach(t => {
                if (!t) return;
                const re = new RegExp(`(${t.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")})`, "ig");
                name = name.replace(re, "<mark>$1</mark>");
            });
            return `<div class="cmdk-item ${i === cmdkIndex ? "sel" : ""}" data-idx="${i}">
                <div class="ci-icon"><i class="fa-solid ${escAttr(r.icon)}"></i></div>
                <div class="ci-text">
                    <div class="ci-title">${name}</div>
                    <div class="ci-sub">${esc(r.desc.slice(0, 96))}</div>
                </div>
                <div class="ci-group">${esc(r.group)}</div>
            </div>`;
        }).join("");
        el.count.textContent = `${cmdkResults.length} kết quả`;
        const sel = $(".cmdk-item.sel", el.results);
        if (sel) sel.scrollIntoView({ block: "nearest" });
    }

    function moveSelection(delta) {
        if (!cmdkResults.length) return;
        cmdkIndex = (cmdkIndex + delta + cmdkResults.length) % cmdkResults.length;
        $$(".cmdk-item", el.results).forEach((n, i) => n.classList.toggle("sel", i === cmdkIndex));
        const sel = $(".cmdk-item.sel", el.results);
        if (sel) sel.scrollIntoView({ block: "nearest" });
    }

    /* ---------- Routing ---------- */
    function go(id) {
        if (!PAGES[id]) return;
        if (location.hash.slice(1) !== id) history.pushState({ id: id }, "", "#" + id);
        render(id);
    }

    function currentId() {
        const id = decodeURIComponent(location.hash.slice(1));
        return PAGES[id] ? id : (ORDER[0] || "intro");
    }

    /* ---------- Global events ---------- */
    document.addEventListener("click", (e) => {
        const cmdItem = e.target.closest(".cmdk-item");
        if (cmdItem) {
            const r = cmdkResults[Number(cmdItem.dataset.idx)];
            if (r) { closePalette(); go(r.id); }
            return;
        }

        const navItem = e.target.closest("[data-target]");
        if (navItem) {
            e.preventDefault();
            go(navItem.dataset.target);
            return;
        }

        const scrollLink = e.target.closest("[data-scroll]");
        if (scrollLink) {
            const node = document.getElementById(scrollLink.dataset.scroll);
            if (node) window.scrollTo({ top: node.getBoundingClientRect().top + window.scrollY - 78, behavior: "smooth" });
            return;
        }

        const tab = e.target.closest(".code-tab");
        if (tab) {
            const inspector = tab.closest(".code-inspector");
            $$(".code-tab", inspector).forEach(t => t.classList.remove("active"));
            tab.classList.add("active");
            $$("pre", inspector).forEach(p => { p.style.display = "none"; });
            const pane = document.getElementById(tab.dataset.pane);
            if (pane) pane.style.display = "block";
            return;
        }

        const copyBtn = e.target.closest("[data-copy]");
        if (copyBtn) {
            const inspector = copyBtn.closest(".code-inspector");
            const active = $$("pre", inspector).find(p => p.style.display !== "none");
            if (active) {
                navigator.clipboard.writeText(active.innerText).then(() => {
                    copyBtn.classList.add("copied");
                    copyBtn.innerHTML = `<i class="fa-solid fa-check"></i> Đã chép`;
                    toast("Đã sao chép vào clipboard");
                    setTimeout(() => {
                        copyBtn.classList.remove("copied");
                        copyBtn.innerHTML = `<i class="fa-regular fa-copy"></i> Sao chép`;
                    }, 1800);
                });
            }
            return;
        }

        if (e.target === el.overlay) closePalette();
    });

    $("#searchTrigger").addEventListener("click", openPalette);
    $("#menuToggle").addEventListener("click", () => el.sidebar.classList.toggle("open"));
    el.input.addEventListener("input", (e) => runSearch(e.target.value));

    el.input.addEventListener("keydown", (e) => {
        if (e.key === "ArrowDown") { e.preventDefault(); moveSelection(1); }
        else if (e.key === "ArrowUp") { e.preventDefault(); moveSelection(-1); }
        else if (e.key === "Enter") {
            e.preventDefault();
            const r = cmdkResults[cmdkIndex];
            if (r) { closePalette(); go(r.id); }
        } else if (e.key === "Escape") { closePalette(); }
    });

    document.addEventListener("keydown", (e) => {
        const typing = /^(INPUT|TEXTAREA|SELECT)$/.test(document.activeElement.tagName);
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
            e.preventDefault();
            el.overlay.classList.contains("open") ? closePalette() : openPalette();
        } else if (e.key === "Escape" && el.overlay.classList.contains("open")) {
            closePalette();
        } else if (!typing && e.key === "/") {
            e.preventDefault();
            openPalette();
        }
    });

    /* ---------- Theme ---------- */
    const themeBtn = $("#themeToggleBtn");
    function applyTheme(theme) {
        document.documentElement.setAttribute("data-theme", theme);
        themeBtn.innerHTML = theme === "dark"
            ? '<i class="fa-solid fa-sun"></i>'
            : '<i class="fa-solid fa-moon"></i>';
        try { localStorage.setItem("concost.theme", theme); } catch (e) { }
    }
    themeBtn.addEventListener("click", () => {
        applyTheme(document.documentElement.getAttribute("data-theme") === "dark" ? "light" : "dark");
    });

    $$(".palette-dot").forEach(dot => {
        const color = dot.dataset.color;
        try {
            if (localStorage.getItem("concost.accent") === color) {
                $$(".palette-dot").forEach(d => d.classList.remove("active"));
                dot.classList.add("active");
            }
        } catch (e) { }
        dot.addEventListener("click", () => {
            $$(".palette-dot").forEach(d => d.classList.remove("active"));
            dot.classList.add("active");
            document.documentElement.style.setProperty("--accent", color);
            document.documentElement.style.setProperty("--accent-soft", color + "1a");
            document.documentElement.style.setProperty("--accent-line", color + "47");
            try { localStorage.setItem("concost.accent", color); } catch (e) { }
        });
    });

    window.addEventListener("popstate", () => render(currentId(), { keepScroll: true }));

    /* ---------- Boot ---------- */
    renderSidebar();
    applyTheme(document.documentElement.getAttribute("data-theme") || "light");
    render(currentId(), { keepScroll: true });

})();
