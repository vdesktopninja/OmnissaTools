# Contributing to LogScrub

Thank you for your interest in contributing to LogScrub! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites
- Windows 10/11 (x64)
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Setting up the Development Environment

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/LogScrub.git
   cd LogScrub
   ```
3. **Add the original repository as upstream**:
   ```bash
   git remote add upstream https://github.com/originalowner/LogScrub.git
   ```
4. **Install dependencies**:
   ```bash
   dotnet restore
   ```
5. **Build and test**:
   ```bash
   dotnet build
   dotnet test LogScrub.Tests/
   ```

## 📝 Contribution Guidelines

### Code Style
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and small
- Use MVVM pattern for UI components

### Commit Messages
Use conventional commit format:
- `feat: add new anonymization algorithm`
- `fix: resolve IP parsing issue`
- `docs: update installation guide`
- `test: add unit tests for FileProcessor`
- `refactor: simplify anonymization logic`

### Pull Request Process

1. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**:
   - Write clean, documented code
   - Add unit tests for new functionality
   - Update documentation if needed

3. **Test thoroughly**:
   ```bash
   dotnet test
   dotnet build --configuration Release
   ```

4. **Commit your changes**:
   ```bash
   git add .
   git commit -m "feat: your descriptive message"
   ```

5. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request**:
   - Use a descriptive title
   - Explain what changes you made and why
   - Reference any related issues

### What to Contribute

#### 🐛 Bug Fixes
- Check existing issues first
- Include steps to reproduce
- Add regression tests

#### ✨ New Features
- Open an issue to discuss the feature first
- Ensure it aligns with project goals
- Include comprehensive tests

#### 📚 Documentation
- Improve README clarity
- Add code comments
- Create usage examples
- Update installation guides

#### 🧪 Tests
- Increase test coverage
- Add edge case tests
- Performance testing

### Areas Needing Help

- **Internationalization**: Add support for more languages
- **Performance**: Optimize large file processing
- **Algorithms**: New anonymization techniques
- **UI/UX**: Improve user interface
- **Documentation**: Better examples and guides
- **Testing**: More comprehensive test coverage

## 🔒 Security Considerations

LogScrub handles sensitive data, so:
- Never commit actual log files
- Be careful with regex patterns (avoid ReDoS)
- Consider performance implications
- Test with large datasets
- Validate all user inputs

## 🎯 Development Tips

### Testing
```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "MethodName"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Building Installer
```bash
cd install
.\Build-Installer.bat
```

### Debugging
- Use Visual Studio debugger
- Check log files in `logs/` directory
- Enable verbose logging in `appsettings.json`

## 📋 Code Review Checklist

Before submitting a PR, ensure:
- [ ] Code follows project conventions
- [ ] All tests pass
- [ ] Documentation is updated
- [ ] No sensitive data in commits
- [ ] Performance implications considered
- [ ] Error handling is appropriate
- [ ] UI changes are user-friendly

## 🆘 Getting Help

- **Issues**: Use GitHub Issues for bugs and feature requests
- **Discussions**: Use GitHub Discussions for questions
- **Documentation**: Check the project Wiki

## 📄 License

By contributing to LogScrub, you agree that your contributions will be licensed under the MIT License.

---

Thank you for helping make LogScrub better! 🎉