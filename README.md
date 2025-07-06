# OmnissaTools Repository

## Overview

Welcome to the **OmnissaTools** repository! This is a collection of community-driven and officially supported tools designed to help administrators manage, automate, and migrate their **Omnissa Horizon** environments.

As the product landscape evolves, the need for robust automation and migration tools becomes critical. This repository aims to be the central hub for scripts and utilities that simplify the daily tasks of a Horizon administrator.

---

## Available Tools

### 1. GPO Migration Script

* **Location:** `/GPO/`
* **Description:** This powerful PowerShell script fully automates the migration of Group Policy Objects (GPOs) from legacy **VMware Horizon** settings to the new **Omnissa Horizon** registry paths. The tool clones a specified source policy, exports its registry settings, translates all known VMware-related paths to their Omnissa equivalents, and then creates a new, ready-to-use GPO in your domain.
* **Use Case:** Indispensable for administrators handling the rebranding and updating of their Horizon infrastructure, saving hours of manual work and preventing configuration errors.
* **[>> Go to the GPO Migration Script Details](./GPO/README.md)**

### 2. (More Tools Coming Soon)

This repository will be regularly updated with new solutions for reporting, automation, and environment management.

---

## Contribution

Contributions are welcome! If you have a tool or a script that could benefit the Omnissa community, please feel free to open a pull request or an issue to discuss your idea.

## Disclaimer

The tools provided in this repository are offered "as-is". It is strongly recommended to test all scripts and tools in a non-production environment before deploying them in your live infrastructure. Always ensure you have backups of your data and configurations before making any changes.
