# AudioToggle Unit Tests

This folder contains unit tests for the AudioToggle application.

## Test Framework

The test project uses:
- **NUnit** - Testing framework (v4.1.0)
- **Microsoft.NET.Test.Sdk** - Test platform
- **NUnit3TestAdapter** - Visual Studio test runner
- **Moq** - Mocking framework (v4.20.70)
- **coverlet.collector** - Code coverage

## Running Tests

### Visual Studio
1. Open the solution file (`src/AudioToggle.sln`)
2. Open Test Explorer (Test > Test Explorer)
3. Click "Run All Tests"

### Command Line
```bash
# From the repository root
dotnet test

# Or from the tests directory
cd tests
dotnet test
```

### With Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Structure

- `NotificationServiceTests.cs` - Tests for notification functionality
- `HotKeyServiceTests.cs` - Tests for hotkey management

## Adding New Tests

1. Create a new test class file in the `tests/` directory
2. Follow the naming convention: `[ClassName]Tests.cs`
3. Use `[TestFixture]` to mark test classes
4. Use `[Test]` for individual test methods
5. Use `[TestCase]` for parameterized tests

## NUnit Test Examples

```csharp
[TestFixture]
public class ExampleTests
{
    [Test]
    public void TestMethod_ShouldReturnExpectedResult()
    {
        // Arrange
        var expected = "expected result";

        // Act
        var actual = SomeMethod();

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("input1", "expected1")]
    [TestCase("input2", "expected2")]
    public void TestMethod_WithParameters_ShouldReturnExpectedResult(string input, string expected)
    {
        // Arrange & Act
        var actual = SomeMethod(input);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }
}
```

## Mocking with Moq

For testing classes with dependencies, use Moq:

```csharp
[Test]
public void ServiceMethod_ShouldCallDependency()
{
    // Arrange
    var mockDependency = new Mock<IDependency>();
    mockDependency.Setup(x => x.SomeMethod()).Returns("mocked result");

    var service = new Service(mockDependency.Object);

    // Act
    var result = service.MethodUnderTest();

    // Assert
    mockDependency.Verify(x => x.SomeMethod(), Times.Once);
    Assert.That(result, Is.EqualTo("expected result"));
}
```

## Mocking Dependencies

For testing classes with dependencies (like UI components or system services), consider using:
- **Moq** - Already included, primary mocking framework
- **NSubstitute** - Alternative mocking framework (if needed)

Add these packages to the test project as needed:
```xml
<PackageReference Include="NSubstitute" Version="5.1.0" />
```
