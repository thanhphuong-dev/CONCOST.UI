# Release notes template

Copy this file, replace every `{{PLACEHOLDER}}`, then publish with:

```powershell
gh release create v{{VERSION}} --title "{{VERSION}} — {{THEME}}" --notes-file artifacts/release-notes-{{VERSION}}.md <assets…>
# or, for an existing release
gh release edit v{{VERSION}} --title "{{VERSION}} — {{THEME}}" --notes-file artifacts/release-notes-{{VERSION}}.md
```

Rules learned the hard way:

- **Title stays under ~55 characters** and never repeats the version twice — GitHub already prints the tag under it. Pattern: `{{VERSION}} — {{THEME}}` (`3.7.0 — Phase 2 · Scroll & chữ`).
- **Banner first, always.** It is the only thing that makes the release page look like the README.
- **No shields.io badges in release bodies.** GitHub's image proxy intermittently fails on them and leaves a broken-image icon; a chip line in inline code looks the same and never breaks.
- **One table per idea**, with the doc link in the last column — a release page is a changelog, not a manual.
- Asset links must be absolute: `https://github.com/…/releases/download/v{{VERSION}}/<file>`.
- Sizes are written with a decimal comma in Vietnamese (`0,81 MB`).

---

![CONCOST.UI — WPF controls for people who build things](https://raw.githubusercontent.com/thanhphuong-dev/CONCOST.UI/main/docs/assets/banner.png)

`{{VERSION}}` · `net48` · `net8.0-windows` · `net10.0-windows` · {{PAGES}} trang tài liệu · MIT

## {{THEME_HEADING}}

{{ONE_PARAGRAPH_LEAD — what changed and why it matters, two sentences maximum}}

| Control | Việc nó làm | Tài liệu |
| :--- | :--- | :---: |
| `{{Control}}` | {{What it does, in one sentence, with the key property names in code}} | [Mở](https://thanhphuong-dev.github.io/CONCOST.UI/#{{anchor}}) |

### {{OPTIONAL_SECTION — e.g. "Sửa lỗi" or "Lỗi tự bắt khi render thử"}}

- {{bullet}}

## Cài đặt

```powershell
dotnet nuget add source "C:\duong\dan\tai\ve" --name concost-local
dotnet add package CONCOST.UI --version {{VERSION}}
```

## Tải về

| File | Dung lượng | Dùng khi nào |
| :--- | ---: | :--- |
| [`CONCOST.UI.{{VERSION}}.nupkg`](https://github.com/thanhphuong-dev/CONCOST.UI/releases/download/v{{VERSION}}/CONCOST.UI.{{VERSION}}.nupkg) | {{NUPKG}} MB | Thêm thư viện vào project |
| [`...-win-x64-framework-dependent.zip`](https://github.com/thanhphuong-dev/CONCOST.UI/releases/download/v{{VERSION}}/CONCOST.UI.Showcase-{{VERSION}}-win-x64-framework-dependent.zip) | {{FD}} MB | Chạy app demo — cần **.NET 8 Desktop Runtime** |
| [`...-win-x64-selfcontained.zip`](https://github.com/thanhphuong-dev/CONCOST.UI/releases/download/v{{VERSION}}/CONCOST.UI.Showcase-{{VERSION}}-win-x64-selfcontained.zip) | {{SC}} MB | Chạy app demo trên máy **chưa cài .NET** |

## Kiểm chứng

- Build sạch **cả 3 target framework** (`net48`, `net8.0-windows`, `net10.0-windows`).
- {{How the new controls were verified — render to PNG, tests, check-xaml}}
- Wiki: **{{PAGES}} trang / {{PROPS}} thuộc tính**, song ngữ Việt / Anh.

---

**Tài liệu:** [Wiki](https://thanhphuong-dev.github.io/CONCOST.UI/) · [Getting Started](https://github.com/thanhphuong-dev/CONCOST.UI/blob/main/docs/GettingStarted.vi.md) · [Changelog](https://github.com/thanhphuong-dev/CONCOST.UI/blob/main/CHANGELOG.md) · [Roadmap hiệu ứng](https://github.com/thanhphuong-dev/CONCOST.UI/blob/main/docs/roadmap/effects-roadmap.md)

**Trước đó:** [{{PREV}}](https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v{{PREV}}) · **Tiếp theo:** {{NEXT — either a link or the planned phase}}
