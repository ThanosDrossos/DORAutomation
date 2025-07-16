# ECB Validation Plugin - Testing and Usage Examples

## Unit Testing Examples

### Test Setup
```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

[TestClass]
public class ECBValidationPluginTests
{
    private IServiceProvider _serviceProvider;
    private IPluginExecutionContext _context;
    private IOrganizationService _service;
    private ITracingService _tracingService;

    [TestInitialize]
    public void Setup()
    {
        // Mock setup for unit testing
        _context = new MockPluginExecutionContext();
        _service = new MockOrganizationService();
        _tracingService = new MockTracingService();
        _serviceProvider = new MockServiceProvider(_context, _service, _tracingService);
    }

    [TestMethod]
    public void TestValidExcelFile_ShouldPass()
    {
        // Arrange
        var plugin = new ECBValidationPlugin();
        var validExcelBytes = CreateValidTestExcelFile();
        
        _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(validExcelBytes);
        
        // Act
        plugin.Execute(_serviceProvider);
        
        // Assert
        Assert.IsTrue(_context.OutputParameters.ContainsKey("ValidationResult"));
        var result = JsonSerializer.Deserialize<dynamic>(_context.OutputParameters["ValidationResult"].ToString());
        Assert.AreEqual("PASS", result.Status);
    }

    [TestMethod]
    public void TestInvalidExcelFile_ShouldFail()
    {
        // Arrange
        var plugin = new ECBValidationPlugin();
        var invalidExcelBytes = CreateInvalidTestExcelFile();
        
        _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(invalidExcelBytes);
        
        // Act
        plugin.Execute(_serviceProvider);
        
        // Assert
        var result = JsonSerializer.Deserialize<dynamic>(_context.OutputParameters["ValidationResult"].ToString());
        Assert.AreEqual("FAIL", result.Status);
        Assert.IsTrue(result.TotalErrors > 0);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidPluginExecutionException))]
    public void TestMissingInputParameter_ShouldThrowException()
    {
        // Arrange
        var plugin = new ECBValidationPlugin();
        // Don't set UserExcelFile parameter
        
        // Act
        plugin.Execute(_serviceProvider);
        
        // Assert - Exception expected
    }

    private byte[] CreateValidTestExcelFile()
    {
        // Create a minimal valid ECB Excel file for testing
        using (var package = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
            
            // Add column headers in row 6, starting from column D
            worksheet.Cells[6, 4].Value = "0020"; // c0020
            worksheet.Cells[6, 5].Value = "0030"; // c0030
            worksheet.Cells[6, 6].Value = "0040"; // c0040
            
            // Add sample data starting from row 8
            worksheet.Cells[8, 4].Value = 100;
            worksheet.Cells[8, 5].Value = "ENT001";
            worksheet.Cells[8, 6].Value = 50;
            
            worksheet.Cells[9, 4].Value = 200;
            worksheet.Cells[9, 5].Value = "ENT002";
            worksheet.Cells[9, 6].Value = 75;
            
            return package.GetAsByteArray();
        }
    }

    private byte[] CreateInvalidTestExcelFile()
    {
        // Create an ECB Excel file with validation errors
        using (var package = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
            
            // Add column headers
            worksheet.Cells[6, 4].Value = "0020";
            worksheet.Cells[6, 5].Value = "0030";
            
            // Add invalid data (missing required fields)
            worksheet.Cells[8, 4].Value = 100;
            worksheet.Cells[8, 5].Value = null; // Missing required field
            
            worksheet.Cells[9, 4].Value = null; // Missing required field
            worksheet.Cells[9, 5].Value = "ENT002";
            
            return package.GetAsByteArray();
        }
    }
}
```

### Mock Classes for Testing
```csharp
public class MockPluginExecutionContext : IPluginExecutionContext
{
    public ParameterCollection InputParameters { get; set; } = new ParameterCollection();
    public ParameterCollection OutputParameters { get; set; } = new ParameterCollection();
    public string MessageName { get; set; } = "dor_ECBValidation";
    public Guid UserId { get; set; } = Guid.NewGuid();
    // ... implement other required properties
}

public class MockOrganizationService : IOrganizationService
{
    // Implement IOrganizationService methods for testing
    public Guid Create(Entity entity) => Guid.NewGuid();
    public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => new Entity();
    // ... implement other methods
}

public class MockTracingService : ITracingService
{
    public List<string> Traces { get; } = new List<string>();
    
    public void Trace(string format, params object[] args)
    {
        Traces.Add(string.Format(format, args));
        System.Diagnostics.Debug.WriteLine(format, args);
    }
}

public class MockServiceProvider : IServiceProvider
{
    private readonly IPluginExecutionContext _context;
    private readonly IOrganizationService _service;
    private readonly ITracingService _tracingService;

    public MockServiceProvider(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService)
    {
        _context = context;
        _service = service;
        _tracingService = tracingService;
    }

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(IPluginExecutionContext))
            return _context;
        if (serviceType == typeof(IOrganizationService))
            return _service;
        if (serviceType == typeof(ITracingService))
            return _tracingService;
        
        return null;
    }
}
```

## Integration Testing Examples

### Power Automate Testing
```powershell
# Test the plugin through Power Automate HTTP trigger
$testPayload = @{
    UserExcelFile = [Convert]::ToBase64String([System.IO.File]::ReadAllBytes("test_ecb_file.xlsx"))
    ECBRulesUrl = ""  # Use default
    TableFilter = ""  # No filter
} | ConvertTo-Json

$headers = @{
    'Content-Type' = 'application/json'
}

$response = Invoke-RestMethod -Uri "https://prod-XX.eastus.logic.azure.com:443/workflows/XXX/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=XXX" -Method Post -Body $testPayload -Headers $headers

Write-Host "Validation Result: $($response.ValidationStatus)"
Write-Host "Error Count: $($response.ErrorCount)"
```

### Dataverse Web API Testing
```javascript
// Test the plugin through Dataverse Web API
async function testECBValidation() {
    const fileInput = document.getElementById('excelFile');
    const file = fileInput.files[0];
    
    if (!file) {
        alert('Please select an Excel file');
        return;
    }
    
    // Convert file to base64
    const base64 = await fileToBase64(file);
    
    // Call custom action
    const response = await fetch(`${window.parent.Xrm.Utility.getGlobalContext().getClientUrl()}/api/data/v9.2/dor_ECBValidation`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify({
            UserExcelFile: base64,
            ECBRulesUrl: '',
            TableFilter: ''
        })
    });
    
    const result = await response.json();
    const validationResult = JSON.parse(result.ValidationResult);
    
    console.log('Validation Status:', validationResult.Status);
    console.log('Total Errors:', validationResult.TotalErrors);
    console.log('Processed Sheets:', validationResult.ProcessedSheets);
    
    // Display results
    displayValidationResults(validationResult);
}

function fileToBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve(reader.result.split(',')[1]);
        reader.onerror = error => reject(error);
    });
}

function displayValidationResults(result) {
    const resultsDiv = document.getElementById('validationResults');
    
    let html = `
        <h3>Validation Results</h3>
        <p><strong>Status:</strong> <span class="${result.Status.toLowerCase()}">${result.Status}</span></p>
        <p><strong>Total Errors:</strong> ${result.TotalErrors}</p>
        <p><strong>Processed Sheets:</strong> ${result.ProcessedSheets.join(', ')}</p>
    `;
    
    if (result.Status === 'FAIL') {
        html += `
            <h4>Error Details:</h4>
            <div class="error-report">
                <pre>${result.Report}</pre>
            </div>
        `;
    }
    
    resultsDiv.innerHTML = html;
}
```

## Performance Testing

### Load Testing Script
```csharp
[TestMethod]
public void LoadTest_ECBValidation()
{
    const int numberOfTests = 100;
    const int maxConcurrentTests = 10;
    
    var tasks = new List<Task>();
    var results = new List<TimeSpan>();
    
    for (int i = 0; i < numberOfTests; i++)
    {
        if (tasks.Count >= maxConcurrentTests)
        {
            Task.WaitAny(tasks.ToArray());
            tasks.RemoveAll(t => t.IsCompleted);
        }
        
        var task = Task.Run(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var plugin = new ECBValidationPlugin();
                var context = CreateTestContext();
                var serviceProvider = CreateTestServiceProvider(context);
                
                plugin.Execute(serviceProvider);
                
                stopwatch.Stop();
                lock (results)
                {
                    results.Add(stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex.Message}");
            }
        });
        
        tasks.Add(task);
    }
    
    Task.WaitAll(tasks.ToArray());
    
    // Analyze results
    var avgTime = results.Average(t => t.TotalMilliseconds);
    var maxTime = results.Max(t => t.TotalMilliseconds);
    var minTime = results.Min(t => t.TotalMilliseconds);
    
    Console.WriteLine($"Load Test Results:");
    Console.WriteLine($"  Tests: {numberOfTests}");
    Console.WriteLine($"  Average Time: {avgTime:F2}ms");
    Console.WriteLine($"  Max Time: {maxTime:F2}ms");
    Console.WriteLine($"  Min Time: {minTime:F2}ms");
    
    Assert.IsTrue(avgTime < 5000, "Average execution time should be under 5 seconds");
}
```

## Sample Test Files

### Valid ECB Test File Structure
```
Sheet: tB_01.02
Row 6: [empty] [empty] [empty] c0020 c0030 c0040 c0050
Row 8: [data]  [data]  [data]  100   ENT001 50    1000
Row 9: [data]  [data]  [data]  200   ENT002 75    1200
```

### Test Data Generator
```csharp
public static class ECBTestDataGenerator
{
    public static byte[] GenerateValidECBFile(string sheetName = "tB_01.02", int dataRows = 100)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            
            // Column headers (row 6, starting column D)
            var columns = new[] { "0020", "0030", "0040", "0050" };
            for (int i = 0; i < columns.Length; i++)
            {
                worksheet.Cells[6, 4 + i].Value = columns[i];
            }
            
            // Generate test data
            var random = new Random();
            for (int row = 8; row < 8 + dataRows; row++)
            {
                worksheet.Cells[row, 4].Value = random.Next(100, 1000);           // c0020
                worksheet.Cells[row, 5].Value = $"ENT{random.Next(1, 999):D3}";  // c0030
                worksheet.Cells[row, 6].Value = random.Next(50, 200);            // c0040
                worksheet.Cells[row, 7].Value = random.Next(1000, 5000);         // c0050
            }
            
            return package.GetAsByteArray();
        }
    }
    
    public static byte[] GenerateInvalidECBFile(string sheetName = "tB_01.02")
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            
            // Column headers
            worksheet.Cells[6, 4].Value = "0020";
            worksheet.Cells[6, 5].Value = "0030";
            
            // Invalid data (violating mandatory field rules)
            worksheet.Cells[8, 4].Value = null;        // Missing required field
            worksheet.Cells[8, 5].Value = "ENT001";
            
            worksheet.Cells[9, 4].Value = 200;
            worksheet.Cells[9, 5].Value = null;        // Missing required field
            
            return package.GetAsByteArray();
        }
    }
}
```

## Error Simulation and Testing

### Simulate Different Error Types
```csharp
[TestMethod]
public void TestMandatoryFieldErrors()
{
    var testFile = ECBTestDataGenerator.GenerateInvalidECBFile();
    var result = ExecuteValidation(testFile);
    
    Assert.AreEqual("FAIL", result.Status);
    Assert.IsTrue(result.TotalErrors > 0);
    
    // Verify specific error types
    var errors = GetErrorsFromResult(result);
    Assert.IsTrue(errors.Any(e => e.ErrorType == "MANDATORY_FIELD_NULL"));
}

[TestMethod]
public void TestRegexPatternErrors()
{
    var testFile = CreateFileWithInvalidPatterns();
    var result = ExecuteValidation(testFile);
    
    var errors = GetErrorsFromResult(result);
    Assert.IsTrue(errors.Any(e => e.ErrorType == "REGEX_PATTERN_MISMATCH"));
}

[TestMethod]
public void TestValueConstraintErrors()
{
    var testFile = CreateFileWithInvalidValues();
    var result = ExecuteValidation(testFile);
    
    var errors = GetErrorsFromResult(result);
    Assert.IsTrue(errors.Any(e => e.ErrorType == "VALUE_CONSTRAINT_VIOLATION"));
}
```

## Monitoring and Diagnostics

### Plugin Execution Monitoring
```csharp
public class ECBValidationMonitor
{
    public static void LogPluginExecution(ITracingService tracingService, 
        string operation, TimeSpan duration, int errorCount)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Operation = operation,
            Duration = duration.TotalMilliseconds,
            ErrorCount = errorCount,
            Status = errorCount == 0 ? "SUCCESS" : "FAILED"
        };
        
        tracingService.Trace($"ECB_MONITOR: {JsonSerializer.Serialize(logEntry)}");
    }
}
```

### Health Check Endpoint
```csharp
// Custom action for plugin health check
public class ECBHealthCheckPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        
        try
        {
            // Test ECB URL accessibility
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("https://eba.europa.eu/...").Result;
                var urlAccessible = response.IsSuccessStatusCode;
                
                context.OutputParameters["HealthStatus"] = JsonSerializer.Serialize(new
                {
                    Status = "HEALTHY",
                    ECBUrlAccessible = urlAccessible,
                    Timestamp = DateTime.UtcNow,
                    PluginVersion = "1.0.0"
                });
            }
        }
        catch (Exception ex)
        {
            context.OutputParameters["HealthStatus"] = JsonSerializer.Serialize(new
            {
                Status = "UNHEALTHY",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
```

---

## Automated Testing Pipeline

### CI/CD Integration
```yaml
# Azure DevOps Pipeline
trigger:
- main

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '$(solution)'

- task: VSBuild@1
  inputs:
    solution: '$(solution)'
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'

- task: VSTest@2
  inputs:
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'
    testAssemblyVer2: |
      **\*test*.dll
      !**\*testadapter.dll
      !**\obj\**

- task: PowerShell@2
  displayName: 'Deploy to Test Environment'
  inputs:
    targetType: 'inline'
    script: |
      # Deploy plugin to test environment
      ./Deploy-ECBValidationPlugin.ps1 -EnvironmentUrl "$(TestEnvironmentUrl)" -PluginAssemblyPath "$(Build.ArtifactStagingDirectory)/ECBValidationPlugin.dll"
```

This comprehensive testing framework ensures the ECB Validation Plugin works correctly across different scenarios and provides monitoring capabilities for production deployments.
