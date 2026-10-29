# Roadmap hiệu ứng — CONCOST.UI

Kế hoạch bổ sung hiệu ứng theo trend thiết kế web 2026, đã lọc theo khả năng của WPF.
Cập nhật: 2026-10-29 · Trạng thái: đề xuất, chờ chốt phase.

---

## 1. Trend 2026 rút ra được gì

Tổng hợp từ các bài về web design 2026 (Bravery, JTech, iWeb, What If Design):

| Trend | Điều này nghĩa gì với WPF |
| --- | --- |
| **Micro-animation có mục đích, 200–400ms** | Đúng sở trường WPF. Ưu tiên hiệu ứng phản hồi trạng thái hơn là hiệu ứng trang trí. |
| **Scroll-driven animation** (CSS `animation-timeline: scroll()`) | Không có tương đương native; làm bằng `ScrollViewer.ScrollChanged` + `RenderTransform`. Chỉ animate transform/opacity. |
| **Chỉ animate `transform` + `opacity`** để chạy trên compositor | WPF: animate `RenderTransform`/`Opacity` là render-thread; animate `Width/Height/Margin` gây layout mỗi frame → **cấm** trong hiệu ứng. |
| **Dark-first, “glow thay cho shadow”** | Hiệu ứng phải dùng brush từ theme (`CONCOST.Brush.*`) để tự đổi theo dark mode. |
| **3D depth, layered composition, perspective** | WPF làm được thật bằng `Viewport3D` + `VisualBrush`, không cần shader. |
| **Performance-first** | Mỗi hiệu ứng phải có ngân sách primitive/frame và tắt khi không hiển thị. |
| **`prefers-reduced-motion` / WCAG 2.2.2** | WPF có `SystemParameters.ClientAreaAnimation` — hiện 13 hiệu ứng của thư viện **chưa tôn trọng** cờ này. Cần sửa trước khi thêm mới. |

---

## 2. Nợ kỹ thuật cần trả trước (Phase 0)

Hiện 9 hiệu ứng tự viết (`AuroraBackground`, `BorderBeam`, `ConfettiBurst`, `MarqueeTicker`, `SheenSweep`, `PulseRing`, `WaveDivider`, `GradientText`, `HeatmapCalendar`) đều lặp lại cùng một khối:

```csharp
private TimeSpan _last; private bool _hooked;
private void OnFrame(...) { /* delta time, clamp, invalidate */ }
private void Hook() { CompositionTarget.Rendering += OnFrame; }
protected override void OnRenderSizeChanged(...) { Hook(); }
```

**Đề xuất `AnimatedEffectBase : FrameworkElement`** với:
- `IsAnimated` (mặc định `true`), tự gắn/gỡ `CompositionTarget.Rendering` theo `IsVisible` + `IsLoaded`.
- Delta time chuẩn hoá (clamp 0–50ms, chống nhảy khi máy sleep).
- `RespectSystemMotion` — đọc `SystemParameters.ClientAreaAnimation`; nếu Windows tắt animation thì **vẽ khung tĩnh** (frame ở pha 0) thay vì đứng hình giữa chừng.
- Hook `SystemParameters.StaticPropertyChanged` để đổi ngay khi user bật/tắt "Show animations".
- `OnTick(double seconds)` cho lớp con; `InvalidateVisual` gom 1 lần/frame.

Lợi ích: 13 hiệu ứng giảm ~500 dòng trùng lặp, và **tự động** tuân thủ reduced-motion.

---

## 3. Danh sách hiệu ứng đề xuất

Đánh giá: **Giá trị** (dùng được trong app kỹ thuật) / **Công** (1–5). Ưu tiên = giá trị cao, công thấp.

### Nhóm A — Pointer & hover (micro-interaction)

| # | Control | Pattern web | Cách làm trong WPF | Giá trị | Công |
| --- | --- | --- | --- | --- | --- |
| A1 | `TiltCard` | Card nghiêng 3D theo chuột (Aceternity/Apple TV) | `Viewport3D` + `VisualBrush` của content, `AxisAngleRotation3D` theo `MouseMove`, easing về 0 khi rời | ★★★★ | 4 |
| A2 | `CursorGlow` | Quầng sáng mềm bám con trỏ | `FrameworkElement` phủ, vẽ `RadialGradientBrush` tại vị trí chuột (lấy từ `Mouse.GetPosition` của host) | ★★★ | 1 |
| A3 | `MagneticHover` | Nút “hút” về phía con trỏ vài px | Attached property + `TranslateTransform` (không layout), biên độ giới hạn | ★★★ | 2 |
| A4 | `HoverReveal` | Ảnh/panel lộ dần khi hover | Attached property: `Clip` hình chữ nhật + `ScaleTransform`, 220ms | ★★ | 2 |

### Nhóm B — Scroll & reveal

| # | Control | Pattern web | Cách làm | Giá trị | Công |
| --- | --- | --- | --- | --- | --- |
| B1 | `ScrollReveal` | Fade + slide-up khi phần tử vào viewport | Attached property trên `ScrollViewer`, dùng `ScrollChanged` + kiểm tra `TransformToAncestor`, một lần rồi thôi | ★★★★ | 3 |
| B2 | `ParallaxPanel` | Nhiều lớp trôi khác tốc độ | `ScrollViewer` offset → `TranslateTransform` từng layer (hệ số `Speed`) | ★★★ | 2 |
| B3 | `ScrollProgressBar` | Thanh tiến độ đọc ở đầu trang | `ScrollChanged` → `Width` của một `Border` (dùng `ScaleTransform` để không layout) | ★★★ | 1 |
| B4 | `StickyHeader` | Header co lại khi cuộn | Attached property: đổi `Height` theo ngưỡng, có animation | ★★ | 2 |

### Nhóm C — Typography

| # | Control | Pattern web | Cách làm | Giá trị | Công |
| --- | --- | --- | --- | --- | --- |
| C1 | `TypewriterText` | Gõ từng ký tự | `DispatcherTimer` hoặc frame tick, cắt chuỗi theo thời gian; có `Cursor` nhấp nháy | ★★★ | 2 |
| C2 | `TextReveal` | Từng từ hiện dần (stagger) | `FormattedText` theo từng từ + alpha/offset tăng dần | ★★★ | 3 |
| C3 | `ScrambleText` | Chữ “nhiễu” rồi hiện đúng | Thay ký tự ngẫu nhiên theo tiến trình, giữ độ dài | ★★ | 2 |

### Nhóm D — Nền & trang trí

| # | Control | Pattern web | Cách làm | Giá trị | Công |
| --- | --- | --- | --- | --- | --- |
| D1 | `ParticleField` | Hạt trôi + nối đường khi gần nhau | `OnRender` vẽ N hạt, nối cặp gần (giới hạn N ≤ 80) | ★★★ | 3 |
| D2 | `NoiseOverlay` | Lớp grain mịn trên nền | Sinh 1 `WriteableBitmap` nhiễu 128×128, tile bằng `ImageBrush`, đổi frame mỗi 80ms | ★★ | 2 |
| D3 | `GridPattern` | Lưới chấm/kẻ mờ có parallax | `DrawingBrush` tile + `TranslateTransform` chậm | ★★★ | 1 |

### Nhóm E — Phản hồi & trạng thái

| # | Control | Pattern web | Cách làm | Giá trị | Công |
| --- | --- | --- | --- | --- | --- |
| E1 | `CheckMarkDraw` | Dấu ✓ tự vẽ nét khi thành công | `StreamGeometry` + `DoubleAnimation` trên `StrokeDashOffset` | ★★★★ | 2 |
| E2 | `ShakeElement` | Rung khi validate lỗi | Attached property, `TranslateTransform` theo hàm sin tắt dần, 320ms | ★★★ | 1 |
| E3 | `RadialGauge` | Vòng tiến độ gradient có kim | `ArcSegment` + `DoubleAnimation` trên `EndAngle` | ★★★ | 3 |

### Nhóm F — Điều hướng & chuyển cảnh

| # | Control | Pattern web | Cách làm | Giá trị | Công |
| --- | --- | --- | --- | --- | --- |
| F1 | `CommandPalette` | `Ctrl+K` mở overlay tìm lệnh | `Popup`/`Adorner` + `TextBox` + `ListBox` lọc, phím ↑↓ Enter, focus trap | ★★★★★ | 4 |
| F2 | `PageTransitionHost` | Chuyển view mượt | `ContentControl` + storyboard fade/slide khi `Content` đổi | ★★★ | 3 |
| F3 | `FlipCard` | Lật thẻ 2 mặt | `Viewport3D` hai mặt phẳng, `AxisAngleRotation3D` 180° | ★★ | 3 |

---

## 4. Lộ trình 4 phase

Mỗi phase = 1 minor release, làm đủ Definition of Done.

### Phase 0 → 3.5.0 (nền móng, ưu tiên tuyệt đối) — ✅ ĐÃ XONG
1. ✅ `MotionSettings` + `AnimatedEffectBase` (3 hiệu ứng mới dùng base; 8 hiệu ứng cũ đã thêm guard).
2. ✅ Tôn trọng `SystemParameters.ClientAreaAnimation` (reduced motion) toàn thư viện.
3. ✅ `CursorGlow`, `GridPattern`, `ShakeElement` + `ThemePreset.Brutal`.
> Việc còn lại (không chặn phase sau): migrate 8 hiệu ứng cũ sang `AnimatedEffectBase` để bỏ ~40 dòng trùng lặp mỗi file.
> Giá trị: sửa nợ + 3 hiệu ứng, không có rủi ro API.

### Phase 1 → 3.6.0 (tương tác) — ✅ ĐÃ XONG
`TiltCard` (2.5D skew+scale), `MagneticHover`, `CheckMarkDraw`, `ScrollProgressBar`.
> Đây là nhóm “micro-interaction có mục đích” đúng trend 2026, và `CheckMarkDraw` dùng được ngay trong luồng duyệt/lưu.

### Phase 2 → 3.7.0 (scroll & chữ)
`ScrollReveal`, `ParallaxPanel`, `TypewriterText`, `TextReveal`.
> Nhóm scrollytelling — dùng cho màn hình giới thiệu/tổng quan dự án.

### Phase 3 → 3.8.0 (nâng cao)
`CommandPalette`, `ParticleField`, `RadialGauge`, `PageTransitionHost`.
> `CommandPalette` là control giá trị nhất còn lại nhưng cũng nặng nhất; tách release riêng để test kỹ bàn phím.

---

## 5. Definition of Done cho mỗi hiệu ứng

- [ ] Control trong `src/CONCOST.UI/Controls/`, kế thừa `AnimatedEffectBase` (nếu có animation).
- [ ] DP có XML doc + mặc định hợp lý; brush lấy từ `CONCOST.Brush.*` khi có thể.
- [ ] `ClipToBounds` + `IsHitTestVisible=false` nếu là lớp phủ.
- [ ] Tắt render khi `!IsVisible`; không giữ event handler sau `Unloaded`.
- [ ] Ngân sách: ≤ 300 primitive/frame, không cấp phát trong `OnRender` (freeze brush/pen).
- [ ] Demo trong Showcase **có nút View XAML / View C#**.
- [ ] Trang docs VI + EN, props table sinh tự động, ≥ 1 callout về hiệu năng.
- [ ] `build-props.py`, `check-xaml.py` (0 issue), `build-index.js` chạy lại.
- [ ] Render harness → PNG kiểm tra thị giác trước khi viết docs.
- [ ] Build 3 TFM + Showcase Release, bump version, tag, release kèm 3 asset.

---

## 6. Không nên port (và lý do)

| Pattern web | Lý do bỏ |
| --- | --- |
| Glassmorphism thật (`backdrop-filter: blur`) | WPF `BlurEffect` chạy CPU, blur toàn màn hình làm tụt FPS; chỉ dùng cho khối nhỏ, không làm control chung. |
| Scroll-jacking (khoá scroll để chạy animation) | Phá thói quen cuộn của người dùng desktop, tệ với accessibility. |
| WebGL/three.js scene | WPF không có WebGL; `Viewport3D` đủ cho card/flip, không đủ cho scene phức tạp. |
| Custom cursor toàn cục | `Cursor` của WPF chỉ hỗ trợ `.cur`/`.ani`; thay con trỏ bằng control sẽ lệch input, không đáng. |
| Neo-brutalism | Là phong cách thị giác, không phải hiệu ứng — nếu muốn thì làm bộ style variant riêng. |

---

## 7. Ưu tiên theo giá trị/công

| Ưu tiên | Hiệu ứng | Lý do |
| --- | --- | --- |
| 1 | `AnimatedEffectBase` + reduced motion | Sửa nợ cho 13 hiệu ứng đã có |
| 2 | `CommandPalette` | Giá trị cao nhất cho app nghiệp vụ nhiều màn hình |
| 3 | `CheckMarkDraw`, `ShakeElement`, `ScrollProgressBar` | Công thấp, dùng ngay |
| 4 | `TiltCard`, `ScrollReveal` | “Wow” nhưng vẫn có mục đích |
| 5 | `TypewriterText`, `TextReveal`, `ParallaxPanel` | Cho màn hình giới thiệu |
| 6 | `ParticleField`, `RadialGauge`, `PageTransitionHost`, `FlipCard` | Nặng hơn, ưu tiên sau |

---

## 8. Cần bạn chốt

1. Làm **Phase 0 (nền móng)** trước hay nhảy thẳng vào `CommandPalette`?
2. Có cần **tôn trọng reduced-motion** mặc định bật không (khuyến nghị: có)?
3. Tên control có giữ tiếng Anh như hiện tại không (`TiltCard`, `CursorGlow`) hay đặt theo nghiệp vụ (`BeamTiltCard`)?
4. Có cần **bộ style Neo-brutalism** như một theme preset riêng (`ThemePreset.Brutal`) không?
