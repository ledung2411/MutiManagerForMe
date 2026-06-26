# Design

## Source of truth
- Status: Active
- Last refreshed: 2026-06-24
- Primary product surfaces: Windows desktop application for daily planning and personal finance.
- Evidence reviewed: `README.md` and the product scope agreed in the Codex thread. No existing UI, brand assets, mockups, or accessibility requirements were present in the repository.

## Brand
- Personality: Calm, practical, trustworthy, and focused on daily action.
- Trust signals: Clear totals, explicit save/delete feedback, local-first storage, and visible data backup options.
- Avoid: Dense enterprise dashboards, playful finance visuals, excessive gradients, and hidden destructive actions.

## Product goals
- Goals: Put today's work, schedule, notes, reminders, and spending in one offline Windows application.
- Goals: Make common capture actions reachable within one click from the relevant screen.
- Goals: Keep financial summaries understandable without turning the product into accounting software.
- Non-goals: Team collaboration, bank integration, cloud sync, mobile clients, and formal accounting in the MVP.
- Success signals: A user can capture a task, note, event, or transaction in under 30 seconds; all persisted items survive restart; due reminders are surfaced while the app is running in the tray.

## Personas and jobs
- Primary personas: A single Vietnamese-speaking knowledge worker managing personal work and expenses.
- User jobs: Plan today, remember deadlines, keep context notes, block time, record spending, and check the monthly budget.
- Key contexts of use: Short capture sessions during work, morning planning, end-of-day review, and expense entry after purchase.

## Information architecture
- Primary navigation: Dashboard, Công việc, Ghi chú, Lịch trình, Thu chi, Cài đặt.
- Core routes/screens: One persistent left navigation rail with one active workspace panel.
- Content hierarchy: Page title and summary first, primary action second, filters and list third, detail/editor panel last.

## Design principles
- Principle 1: Today first. Current obligations and immediate financial state outrank historical detail.
- Principle 2: Capture before configuration. New-item forms start small and expose only fields needed for the MVP.
- Principle 3: Status is explicit. Priority, completion, income/expense direction, and remaining budget never rely on color alone.
- Tradeoffs: The MVP uses list and agenda views instead of a complex drag-and-drop calendar; local reliability takes priority over cloud features.

## Visual language
- Color: Warm off-white canvas, white surfaces, dark navy text, indigo primary actions, green success/income, amber warning, and red destructive/expense.
- Typography: Segoe UI, 13-14 px body equivalent, 20-28 px page titles, semibold labels.
- Spacing/layout rhythm: 4 px base grid with common gaps of 8, 12, 16, 24, and 32 px.
- Shape/radius/elevation: 8-14 px rounded surfaces, subtle one-pixel borders, minimal shadow.
- Motion: Short 120-180 ms state changes only; no decorative motion.
- Imagery/iconography: Text and simple Unicode/system glyphs in the MVP; labels accompany ambiguous icons.

## Components
- Existing components to reuse: None.
- New/changed components: Navigation item, stat card, section card, primary/secondary/danger buttons, form field, status badge, empty state, list row, and confirmation dialog.
- Variants and states: Default, hover, focused, selected, disabled, completed, overdue, income, expense, warning, and error.
- Token/component ownership: Shared WPF resources in `Themes/AppTheme.xaml`; screen-specific layout remains in each view.

## Accessibility
- Target standard: WCAG 2.1 AA where applicable to Windows desktop controls.
- Keyboard/focus behavior: All actions keyboard reachable; logical tab order; Enter submits focused forms where safe; Escape cancels dialogs.
- Contrast/readability: Minimum 4.5:1 for body text; status includes text or symbols in addition to color.
- Screen-reader semantics: Native WPF controls and meaningful AutomationProperties names.
- Reduced motion and sensory considerations: No required animation; reminders use visible text as well as sound.

## Responsive behavior
- Supported breakpoints/devices: Windows desktop, minimum usable window 1100 x 700 at 100-200% scaling.
- Layout adaptations: Main content scrolls; editor panels use stable minimum widths; cards wrap where practical.
- Touch/hover differences: Minimum 36 px action targets; hover is supplementary rather than required.

## Interaction states
- Loading: Short operations remain synchronous with disabled actions; long imports/backups require progress feedback in later versions.
- Empty: Explain what belongs in the area and show a direct creation action.
- Error: Human-readable Vietnamese message with the failed operation and retained form data.
- Success: Immediate list and summary refresh; avoid modal success dialogs.
- Disabled: Reduced contrast plus tooltip/reason when the restriction is not obvious.
- Offline/slow network: Core product has no network dependency.

## Content voice
- Tone: Direct, calm Vietnamese; concise labels and actionable errors.
- Terminology: “Công việc”, “Ghi chú”, “Lịch trình”, “Thu chi”, “Ví”, “Ngân sách”, and “Nhắc lúc”.
- Microcopy rules: Use sentence case, show currency as Vietnamese đồng, use explicit confirmation for destructive actions.

## Implementation constraints
- Framework/styling system: C# WinUI 3 / Windows App SDK on .NET desktop, MVVM, SQLite local storage.
- Design-token constraints: Colors, typography, spacing primitives, and reusable control styles live in shared resources.
- Performance constraints: Initial dashboard should appear within two seconds for normal personal datasets; lists should support at least 10,000 persisted records.
- Compatibility constraints: Windows 10/11 x64, offline use, single-user local profile.
- Test/screenshot expectations: Build must pass; data-service behavior receives automated tests; main screens are visually inspected at the minimum supported size.

## Open questions
- [ ] Decide whether closing the main window should always minimize to tray or ask once / product owner / affects reminder reliability.
- [ ] Decide whether Markdown or rich text is preferred for notes / product owner / affects editor choice.
- [ ] Confirm whether a week/month calendar grid is required before v1.0 / product owner / affects schedule scope.
- [ ] Confirm whether local data encryption is required / product owner / affects database and backup design.
