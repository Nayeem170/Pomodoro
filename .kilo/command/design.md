---
description: Create or update a Pencil mock design for a UI feature
---
Create a Pencil mock design for the requested UI feature.

Prerequisites:
- Pencil CLI installed: `npm install -g @pen.dev/cli`
- Authenticated: `pen login` (run once, interactive)
- Pencil VS Code extension installed: `highagency.pencildev`

Steps:
1. Read the feature description from $ARGUMENTS. If no argument, ask what to design.
2. Check existing CSS theme in `src/Pomodoro.Web/wwwroot/css/` for colors, spacing, and typography.
3. Check existing component structure in `src/Pomodoro.Web/Components/` for layout patterns.
4. Run the Pencil CLI to generate the design:
   ```
   pen --out designs/<FeatureName>.pen --prompt "<design prompt based on feature + theme>" --agent claude
   ```
   The prompt should reference the app's visual style (minimal, clean Pomodoro timer aesthetic).
5. Export a PNG preview:
   ```
   pen --in designs/<FeatureName>.pen --export designs/<FeatureName>.png --export-scale 2
   ```
6. After the design is created, output a summary of:
   - File locations: `.pen` design + `.png` preview
   - Layout structure and components shown
   - Mapping notes: which Blazor components from `src/Pomodoro.Web/Components/` would implement each section
   - Any new CSS utilities needed

Do NOT create or modify any Blazor source files. This command is for mock design only.

Tip: To iterate on an existing design, use:
```
pen --in designs/<FeatureName>.pen --out designs/<FeatureName>.pen --prompt "<modification>" --agent claude
```

Tip: To open the design in the VS Code canvas, open the `.pen` file in VS Code - the pen.dev extension renders it visually.
