# 🧩 Unity-Package-Reader

A simple C# console tool that extracts `.cs` scripts from Unity `.unitypackage` files without opening Unity.

---

## 💡 Features

- 🔍 Reads and lists all `.cs` script files in a `.unitypackage`
- 📂 Extracts selected or all scripts to an `ExtractedScripts/` folder
- ⚡ Fast — works entirely from the command line
- 🛠️ .NET-powered standalone `.exe` build

---

## 🧪 How It Works

Unity `.unitypackage` files are just `.tar.gz` archives. This tool:
1. Decompresses the archive
2. Parses the internal `pathname` and `asset` files
3. Matches script paths with raw file data
4. Saves the scripts to disk in their original folder structure
