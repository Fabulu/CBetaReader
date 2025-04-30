# CBETA Reader

**CBETA Reader** is a free, offline desktop application for browsing, reading, and translating Chinese Buddhist texts from the [CBETA XML-P5](https://www.cbeta.org/) corpus.

This is a minimal and fast reader designed specifically for Zen study and translation work.

---

## ✨ Features

- 📂 Browse CBETA XML-P5 by Canon and Sutra
- 📖 Read TEI-formatted texts with heading-aware formatting
- 📚 Automatic Table of Contents (based on `<mulu>` tags)
- 📋 Right-click TOC entries to copy headings
- 🌐 Offline machine translation (Traditional Chinese → English)
- 🧠 Built-in translation using **Argos Translate** (with bundled Python)
- 🪷 No internet required. No tracking. No ads.

---

## 📸 Screenshots

![screenshot](Screenshots/manual.png) <!-- Add one later -->

---

## 🧰 Requirements

- Windows 10 or 11 (64-bit) - I am very sorry about this.
- No .NET installation needed — it's bundled

---

## 🐍 The Python in the room

If you want to compile and run this yourself, you need a Python install with the language model for the translation. That's the bad news. I included a batch file in the project directory called setup_python.bat. It probably won't work, but if you open it you'll see what you need to do if you tinker some.

The good news is that this is modular in the project, so it should be easy to replace with something else, like an API call if you want to go that route. Of course, APIs usually cost money.

But maybe... someday...

We can dream.

## 🚀 Getting Started

1. Download the latest release from [Releases](https://github.com/your-username/cbeta-zen-reader/releases)
2. Get the CBeta archive from https://github.com/cbeta-org/xml-p5/tree/master
3. Unzip the archives you downloaded. CBeta is about 2 Gigabytes unzipped. You can also clone the git repository instead for full access to the history of edits.
4. Double-click `CbetaZenReader.exe`
5. On first launch, you'll be asked to locate your **CBETA XML-P5 root folder**
6. Select any text → Click "Translate Selection" to generate an English translation

---

## 🧠 Powered By

- [Argos Translate](https://www.argosopentech.com/) — Offline neural translation
- [CBETA XML-P5](https://www.cbeta.org/)
- .NET 8 WPF (self-contained deployment)

---

## 🔒 Privacy

CBETA Zen Reader runs completely offline. No network access, no telemetry, no tracking.

---

## 💡 Planned Features

- 🔍 Full-text search
- 🧭 Improved navigation through canonical structure
- 📘 Markdown / PDF export
- 📖 Parallel text mode for full sutra translation work

---

## 📜 License

MIT License

---

## 🙏 Acknowledgments

Thanks to the CBETA project and the open-source developers behind Argos Translate.