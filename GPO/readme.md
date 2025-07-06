# GPO Migration Script: VMware Horizon to Omnissa Horizon

![PowerShell](https://img.shields.io/badge/PowerShell-5.1%2B-blue.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)

A PowerShell script to automate the migration of Group Policy Objects (GPOs) from legacy VMware Horizon settings to the new Omnissa Horizon registry paths.

---

## 📜 Overview

This PowerShell script automates the process of cloning a Group Policy Object (GPO) with VMware Horizon settings and converting it to use the new registry paths for Omnissa Horizon. After the rebranding of VMware's End-User Computing (EUC) division to Omnissa, all associated registry paths for GPO settings have changed.

This script saves the significant manual effort required to create a new GPO and recreate hundreds of policy settings. It clones a source GPO, exports its registry settings, performs a detailed find-and-replace on all known paths, and injects the updated settings back into the new GPO.

---

## ✨ Features

-   **Clone GPO**: Prompts the user for a source GPO and a name for a new GPO, then creates a full copy.
-   **Export & Import Settings**: Uses Microsoft's `LGPO.exe` utility to export the binary `Registry.pol` file into a human-readable text format and later rebuilds it.
-   **Comprehensive Path Mapping**: Contains a large, built-in hash table that maps every known VMware Horizon registry path to its new Omnissa equivalent.
-   **Automated Replacement**: Iterates through the exported text file and replaces all old paths with new ones.
-   **Finalize GPO**: Replaces the `Registry.pol` file in the newly created GPO's folder within SYSVOL.
-   **Interactive & Safe**: Provides real-time progress updates, includes error handling, and automatically cleans up all temporary files.

---

## ⚙️ Requirements

- [ ] **Windows Environment**: A machine with access to an Active Directory domain.
- [ ] **PowerShell**: The `GroupPolicy` module must be available. This is typically included with Remote Server Administration Tools (RSAT).
- [ ] **Permissions**: The user running the script needs permissions to read, create, and modify Group Policy Objects in the domain.
- [ ] **LGPO.exe**: Microsoft's Local Group Policy Object Utility (`LGPO.exe`) must be available. It can be downloaded as part of the [Microsoft Security Compliance Toolkit](https://www.microsoft.com/en-us/download/details.aspx?id=55319).

---

## 🛠️ Configuration

Before running the script, you must specify the location of the `LGPO.exe` file by editing the script.

| Variable | Description | Example |
|---|---|---|
| `$LgpoExePath` | The full, absolute path to the `LGPO.exe` executable. | `C:\Tools\LGPO\LGPO.exe` |

---

## 🚀 Usage

1.  Ensure all **Requirements** are met and the **Configuration** step is complete.
2.  Open a PowerShell console.
3.  Navigate to the directory where you saved the `migration.ps1` script.
4.  Execute the script:
    ```powershell
    .\migration.ps1
    ```
5.  Follow the on-screen prompts to enter the name of the source (VMware) GPO and the desired name for the new (Omnissa) GPO.
6.  The script will display its progress and confirm when the operation is complete.

---

## 🧠 How It Works

The script follows these logical steps:

1.  **Get User Input**: Prompts for the source and new GPO names.
2.  **Copy GPO**: Creates a new GPO and copies all settings, files, and ACLs from the source GPO.
3.  **Create Temp Environment**: Sets up a temporary folder to work with policy files.
4.  **Extract `Registry.pol`**: Finds the `Registry.pol` file in the new GPO's SYSVOL folder and copies it to the temp directory.
5.  **Parse to Text**: Uses `LGPO.exe /parse` to convert the binary `.pol` file into a readable `.txt` file.
6.  **Transform Paths**: Reads the `.txt` file and runs a comprehensive find-and-replace operation using the internal `$pathMappings` hash table. It sorts the map keys by length (longest first) to ensure that specific paths are replaced before their generic parent paths.
7.  **Rebuild `.pol` File**: Uses `LGPO.exe /r /w` to build a new, modified `Registry.pol` file from the updated `.txt` file.
8.  **Replace in SYSVOL**: Copies the newly built `Registry.pol` from the temp folder back into the GPO's directory in SYSVOL.
9.  **Cleanup**: Deletes the temporary folder and all its contents.

---

## ⚠️ Disclaimer

> This script modifies Group Policy Objects in your Active Directory environment. It is **strongly recommended** to back up your GPOs before use. Always test this script in a non-production environment first. Use at your own risk.
