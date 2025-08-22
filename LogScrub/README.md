# LogScrub - Log Anonymization Tool

A professional WPF application for secure log file anonymization, designed to help organizations protect sensitive data while maintaining log utility for analysis.

![LogScrub](icon.ico)

## 🚀 Features

- **IP Address Anonymization**: Multiple anonymization modes (zero-out, random, hash-based)
- **Domain Name Anonymization**: Replace FQDNs with anonymized alternatives
- **Username Anonymization**: Protect user identity in log files
- **Server/Hostname Anonymization**: Anonymize server and hostname references
- **RFC1918 Preservation**: Option to keep private IP ranges intact
- **Batch Processing**: Process multiple files and ZIP archives
- **Multi-threaded**: Configurable parallelism for large datasets
- **Progress Tracking**: Real-time processing status and logging
- **Material Design UI**: Modern, intuitive user interface

## 🔧 Requirements

- **Windows 10/11** (x64)
- **.NET 8.0 Runtime** (Windows Desktop)
- **Visual Studio 2022** or **VS Code** (for development)
- **WiX Toolset 6.x** (for building installers)

## 🏗️ Building from Source

### Prerequisites
```bash
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# For installer building (optional)
# Download and install WiX v6 from: https://wixtoolset.org/
```

### Build Steps
```bash
# Clone the repository
git clone https://github.com/yourusername/LogScrub.git
cd LogScrub

# Restore dependencies
dotnet restore

# Build the application
dotnet build --configuration Release

# Run the application
dotnet run --configuration Release
```

### Creating MSI Installer
MSI installer creation requires WiX Toolset and additional build scripts not included in this public repository. For development purposes, you can run the application directly with `dotnet run`.

## 📦 Installation

### Option 1: Download Pre-built MSI
1. Download `LogScrub-Setup-v1.0.0.0.msi` from the `releases/` folder
2. Run as Administrator  
3. Follow the installation wizard

### Option 2: Download from GitHub Releases
1. Visit the [Releases](https://github.com/yourusername/LogScrub/releases) page
2. Download the latest `LogScrub-Setup-v*.msi`
3. Run as Administrator

### Option 3: Build from Source
See [Building from Source](#-building-from-source) section above.

## 🎯 Usage

1. **Launch LogScrub** from Start Menu or Desktop
2. **Select Input**: Choose folder or ZIP file containing logs
3. **Select Output**: Choose destination for anonymized files
4. **Configure Options**:
   - Enable/disable anonymization types
   - Set IP anonymization mode
   - Configure parallelism level
   - Set target domain (optional)
5. **Start Processing**: Click "START PROCESSING"
6. **Monitor Progress**: View real-time status and logs

### Anonymization Options

- **IP Addresses**: Replace with anonymized equivalents
- **Domain Names**: Replace FQDNs (e.g., `company.com` → `domain1.local`)
- **Usernames**: Replace user identifiers
- **Servers/Hostnames**: Replace server names
- **RFC1918 Private IPs**: Option to preserve internal IP ranges

## 🔐 Code Signing

The provided MSI installer is digitally signed by Dominik Jakubowski for authenticity and security.

## 🧪 Testing

```bash
# Run unit tests
dotnet test LogScrub.Tests/
```

## 📁 Project Structure

```
LogScrub/
├── 📄 Main Application Files
├── 📁 Common/           # Shared utilities and base classes
├── 📁 Configuration/    # Application configuration
├── 📁 Engine/          # Core anonymization logic
├── 📁 Services/        # Business logic services
├── 📁 ViewModels/      # MVVM view models
├── 📁 Resources/       # Localization resources
├── 📁 LogScrub.Tests/  # Unit tests
└── 📁 releases/        # Pre-built MSI installer
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Dominik Jakubowski**

## 🙏 Acknowledgments

- Material Design In XAML Toolkit
- .NET Community
- WiX Toolset Team

## 📞 Support

- Create an [Issue](https://github.com/yourusername/LogScrub/issues) for bug reports
- Start a [Discussion](https://github.com/yourusername/LogScrub/discussions) for questions
- Check the [Wiki](https://github.com/yourusername/LogScrub/wiki) for detailed documentation

---
*Made with ❤️ for secure log processing*