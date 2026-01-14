# Jahro — Unity Debugging Platform

<div align="center">

![Jahro Interface](https://vdepoiw1jnimcohf.public.blob.vercel-storage.com/Jahro_3%201-f2rMPbdeHAvaTZaFEGHbfxIQ8hqFM0.png)

**One-tap snapshots with logs, screenshots, and device info — shareable instantly.**

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-black.svg?style=flat&logo=unity)](https://unity.com)
[![License](https://img.shields.io/badge/License-Custom-blue.svg)](https://github.com/jahro-console/unity-package/blob/main/LICENSE.md)

[Getting Started](https://jahro.io/docs/getting-started?utm_source=github&utm_medium=readme&utm_campaign=unity_client) •
[Website](https://jahro.io?utm_source=github&utm_medium=readme&utm_campaign=unity_client) •
[Documentation](https://jahro.io/docs?utm_source=github&utm_medium=readme&utm_campaign=unity_client) •
[Web Console](https://console.jahro.io?utm_source=github&utm_medium=readme&utm_campaign=unity_client) •
[Discord](https://discord.gg/txcHFRDeV4)

</div>

---

## Contents

- [The Problem](#the-problem)
- [The Solution](#the-solution)
- [Core Features](#core-features)
  - [Snapshots — Complete Context in One Tap](#snapshots--complete-context-in-one-tap)
  - [Runtime Commands — Control Without Rebuilding](#runtime-commands--control-without-rebuilding)
  - [Variable Watcher — See Values Live](#variable-watcher--see-values-live)
- [Who Is This For?](#who-is-this-for)
  - [Unity Developers](#unity-developers)
  - [QA Testers](#qa-testers)
  - [Tech Leads](#tech-leads)
  - [Game Designers](#game-designers)
- [Quick Start](#quick-start)
- [Key Features at a Glance](#key-features-at-a-glance)
- [Documentation & Resources](#documentation--resources)
- [Community & Support](#community--support)
- [Requirements](#requirements)
- [License](#license)

---

## The Problem

**For Developers:** Tired of chasing QA for logs and reproduction steps? Spending hours debugging mobile-specific issues that "work in Editor"?

**For QA:** Bugs dismissed as "can't reproduce"? No way to capture logs without cables, ADB, or asking devs for help?

**The reality:** 50-70% of bug reports lack sufficient context on first report. The average "can't reproduce" bug wastes 2-4 hours of developer time.

---

## The Solution

Jahro helps Unity game teams **fix bugs faster** by enabling anyone to capture complete debug context — logs, screenshots, device info — directly on mobile devices and share it instantly with developers.

**No cables. No ADB. No back-and-forth.**

### What Makes Jahro Different

|                             | Jahro | SRDebugger & Lunar | Sentry & Crashlytics |
| --------------------------- | :---: | :----------------: | :------------------: |
| **On-device logs**          |  ✅   |         ✅         |          ❌          |
| **Cloud sharing**           |  ✅   |         ❌         |  ✅ (crashes only)   |
| **QA-friendly** (no cables) |  ✅   |   ⚠️ Local only    |          ❌          |
| **Runtime commands**        |  ✅   |         ✅         |          ❌          |
| **Real-time debugging**     |  ✅   |         ✅         |          ❌          |
| **Shareable web links**     |  ✅   |         ❌         |          ✅          |

**The key difference:** Jahro is the only tool that bridges QA bug discovery and developer reproduction with shareable, complete context.

---

## Core Features

### Snapshots — Complete Context in One Tap

![Web Console Logs](https://jahro-snapshots-bucket.fra1.digitaloceanspaces.com/promo/screenshots/web-logs-all.png)

Capture full device logs (with stack traces), screenshots, and device metadata in a session. Share a web link instantly.

**Two modes:**

- **Recording Mode**: Capture locally, upload when ready
- **Streaming Mode**: Real-time log streaming (~5-second intervals) — watch QA reproduce the bug live

> **"The tester says that the game is broken and sends a snapshot. You can finish your task and calmly review what went wrong with ALL logs in hands."** — Andriy, Unity Developer

**Why this matters:** No more "can you send logs?" messages. QA captures everything in one tap. Dev opens link and sees full context. Bug gets fixed instead of dismissed.

---

### Runtime Commands — Control Without Rebuilding

![Unity Visual Commands](https://jahro-snapshots-bucket.fra1.digitaloceanspaces.com/promo/screenshots/unity-visual-full.png)

Execute C# methods at runtime through visual UI or text commands. Change game parameters, skip levels, grant items, trigger events — **without waiting for rebuilds**.

```csharp
[JahroCommand]
public void AddGold(int amount) {
    playerInventory.gold += amount;
}
```

**Visual Mode:** Button-based interface — perfect for QA and designers. No memorization required.

**Text Mode:** CLI with autocomplete — for power users who want speed.

**Why this matters:** Stop wasting 5-15 minutes per rebuild when testing balance changes. Iterate instantly on any device.

---

### Variable Watcher — See Values Live

![Unity Watcher](https://jahro-snapshots-bucket.fra1.digitaloceanspaces.com/promo/screenshots/unity-watcher-full.png)

Monitor variable values in real-time without polluting logs with `Debug.Log` statements.

```csharp
[JahroWatch]
public int PlayerHealth { get; set; }
```

**Why this matters:** Track values frame-by-frame without performance impact or log spam. Perfect for tuning gameplay mechanics.

---

## Who Is This For?

### Unity Developers

Stop spending hours debugging mobile-specific issues. See exactly what happened on any device without cables or ADB.

- ✅ Reproduce bugs in minutes instead of hours
- ✅ Get complete context from QA automatically
- ✅ Review bugs async — no interruptions to your flow
- ✅ Debug on any device

### QA Testers

Send complete bug reports in one tap. No more "can't reproduce" — devs get all logs, screenshots, and device info automatically.

- ✅ Capture everything developers need without technical knowledge or ADB/Xcode
- ✅ Triple-tap to open, tap to capture, share link — done
- ✅ Your bugs get believed and fixed faster
- ✅ Work independently without asking devs for setup help

### Tech Leads

Standardize debugging across all projects. Stop rebuilding debug tools from scratch.

- ✅ Faster bug resolution = more stable releases
- ✅ QA works independently = devs focus on features
- ✅ Clear ROI: Save 10-15 hours/week per developer

### Game Designers

Change game parameters at runtime without asking devs to rebuild.

- ✅ Test balance changes instantly
- ✅ Iterate independently without blocking developers
- ✅ Visual command interface — no coding required
- ✅ See key values realtime directly inside Game view

---

## Quick Start

### Installation

1. **Add Package via Unity Package Manager**

   ```
   https://github.com/jahro-console/unity-package.git
   ```

2. **Get Your API Key**

   - Register at [jahro.io](https://jahro.io?utm_source=github&utm_medium=readme&utm_campaign=unity_client)
   - Create a project and copy your API key

3. **Configure in Unity**

   - Go to **Tools > Jahro Settings**
   - Paste your API key

4. **Add Your First Command**

   ```csharp
   [JahroCommand]
   public void GiveCoins(int amount) {
       coins += amount;
   }
   ```

5. **Build & Test**
   - Build to your device or enter Play Mode in Unity
   - Triple-tap the screen to open Jahro
   - Execute your command or capture a snapshot

---

## Key Features at a Glance

- ✅ **Snapshots** — One-tap capture of logs + screenshots + device metadata
- ✅ **Shareable Web Links** — QA sends link, dev reviews in browser
- ✅ **Runtime Commands** — Execute C# methods with visual or text interface
- ✅ **Variable Watcher** — Monitor values in real-time without Debug.Log spam
- ✅ **Recording & Streaming Modes** — Capture locally or stream live to team
- ✅ **Cross-Platform** — iOS, Android (works on low-end devices)
- ✅ **Minimal Integration** — Attribute-based API
- ✅ **Production-Safe** — Auto-disable in production builds
- ✅ **Team Collaboration** — Projects, members, roles, shared sessions

---

## Documentation & Resources

- 📖 **[Full Documentation](https://jahro.io/docs?utm_source=github&utm_medium=readme&utm_campaign=unity_client)** — Complete guides and API reference
- 🚀 **[Getting Started Guide](https://jahro.io/docs/getting-started?utm_source=github&utm_medium=readme&utm_campaign=unity_client)** — Step-by-step installation
- 📚 **[Snapshots Guide](https://jahro.io/docs/unity/snapshots?utm_source=github&utm_medium=readme&utm_campaign=unity_client)** — Learn snapshot workflows
- ⚡ **[Commands Guide](https://jahro.io/docs/unity/commands?utm_source=github&utm_medium=readme&utm_campaign=unity_client)** — Runtime commands deep-dive
- 👁️ **[Watcher Guide](https://jahro.io/docs/unity/watcher?utm_source=github&utm_medium=readme&utm_campaign=unity_client)** — Variable watching tutorial
- 🖥️ **[Web Console](https://console.jahro.io?utm_source=github&utm_medium=readme&utm_campaign=unity_client)** — Review snapshots in browser
- 🗺️ **[Roadmap](https://jahro.io/roadmap?utm_source=github&utm_medium=readme&utm_campaign=unity_client)** — See what's coming next

---

[![Stand with Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://vshymanskyy.github.io/StandWithUkraine)

---

## Community & Support

### Get Help

- 💬 **[Discord Community](https://discord.gg/txcHFRDeV4)** — Chat with other users and the team
- 🐛 **[GitHub Issues](https://github.com/jahro-console/unity-package/issues)** — Report bugs or request features
- 📧 **[Email Support](mailto:support@jahro.com)** — Direct support for Pro users

---

## Requirements

**Unity Version:**

- Unity 2021.3 LTS or later
- Unity 6 (recommended)

**Account:**

- Free account at [jahro.io](https://jahro.io?utm_source=github&utm_medium=readme&utm_campaign=unity_client) for cloud features

---

## License

This plugin is distributed under a [custom license](https://github.com/jahro-console/unity-package/blob/main/LICENSE.md).

© 2026 Oleksii Bulat. All rights reserved.

---
