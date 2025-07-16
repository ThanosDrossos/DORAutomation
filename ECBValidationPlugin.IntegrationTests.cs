using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using OfficeOpenXml;
using ECBValidation.Tests;

namespace ECBValidation.IntegrationTests
{
    /// <summary>
    /// Integration tests that test the complete plugin workflow end-to-end
    /// These tests use real ECB file structures and validation scenarios
    /// </summary>
    [TestClass]
    public class ECBValidationIntegrationTests
    {
        private MockServiceProvider _serviceProvider;
        private MockPluginExecutionContext _context;
        private string _testDataPath;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // Set EPPlus license context for all integration tests
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize mock services
            _context = new MockPluginExecutionContext();
            var organizationService = new MockOrganizationService();
            var tracingService = new MockTracingService();
            _serviceProvider = new MockServiceProvider(_context, organizationService, tracingService);

            // Set up test data path
            _testDataPath = Path.Combine(TestContext.TestRunDirectory, "TestData");
            Directory.CreateDirectory(_testDataPath);
        }

        [TestMethod]
        public void FullWorkflow_ValidECBFile_ShouldCompleteSuccessfully()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var validECBFile = ECBTestUtilities.CreateSampleECBFile(100); // 100 rows
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(validECBFile);
            _context.InputParameters["ECBRulesUrl"] = ""; // Use default (will be mocked)
            _context.InputParameters["TableFilter"] = "";

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            plugin.Execute(_serviceProvider);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(_context.OutputParameters.ContainsKey("ValidationResult"));
            
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            Assert.AreEqual("PASS", result.GetProperty("Status").GetString());
            Assert.AreEqual(0, result.GetProperty("TotalErrors").GetInt32());
            Assert.IsTrue(result.GetProperty("ProcessedSheets").GetArrayLength() > 0);
            
            // Performance check
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, 
                $"Validation should complete within 5 seconds, but took {stopwatch.ElapsedMilliseconds}ms");
        }

        [TestMethod]
        public void FullWorkflow_InvalidECBFile_ShouldDetectAllErrors()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var invalidECBFile = ECBTestUtilities.CreateECBFileWithErrors();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(invalidECBFile);

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            Assert.AreEqual("FAIL", result.GetProperty("Status").GetString());
            Assert.IsTrue(result.GetProperty("TotalErrors").GetInt32() > 0);
            
            // Verify report contains error details
            var report = result.GetProperty("Report").GetString();
            Assert.IsTrue(report.Contains("FAIL"));
            Assert.IsTrue(report.Length > 100); // Should have substantial error details
        }

        [TestMethod]
        public void TableFilter_MultipleSheets_ShouldProcessOnlyFilteredSheets()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var multiSheetFile = ECBTestUtilities.CreateMultiSheetECBFile();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(multiSheetFile);
            _context.InputParameters["TableFilter"] = "tB_01"; // Should match tB_01.02 and tB_01.03

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            var processedSheets = result.GetProperty("ProcessedSheets");
            Assert.IsTrue(processedSheets.GetArrayLength() >= 2); // At least tB_01.02 and tB_01.03
            
            // Verify only tB_01 sheets were processed
            foreach (var sheet in processedSheets.EnumerateArray())
            {
                var sheetName = sheet.GetString();
                Assert.IsTrue(sheetName.StartsWith("tB_01"), 
                    $"Expected only tB_01 sheets, but found {sheetName}");
            }
        }

        [TestMethod]
        public void RuleExtraction_MockECBFile_ShouldExtractAllRuleTypes()
        {
            // Arrange
            var extractor = new ECBRuleExtractor();
            var mockECBFile = ECBTestUtilities.CreateMockECBRulesFile();

            // Act
            var rules = extractor.ExtractRules(mockECBFile);

            // Assert
            Assert.IsTrue(rules.Count >= 8, "Should extract at least 8 mock rules");
            
            // Verify all rule types are represented
            var ruleTypes = rules.Select(r => r.RuleType).Distinct().ToList();
            Assert.IsTrue(ruleTypes.Contains("mandatory_field"));
            Assert.IsTrue(ruleTypes.Contains("regex_validation"));
            Assert.IsTrue(ruleTypes.Contains("value_constraint"));
            Assert.IsTrue(ruleTypes.Contains("conditional_rule"));
            
            // Verify rule structure
            Assert.IsTrue(rules.All(r => !string.IsNullOrEmpty(r.Id)));
            Assert.IsTrue(rules.All(r => !string.IsNullOrEmpty(r.Expression)));
            Assert.IsTrue(rules.All(r => !string.IsNullOrEmpty(r.RuleType)));
            Assert.IsTrue(rules.All(r => r.SourceRow > 0));
        }

        [TestMethod]
        public void ValidationEngine_RealWorldScenario_ShouldHandleComplexData()
        {
            // Arrange
            var engine = new ECBValidationEngine(CreateRealWorldTestRules());
            var complexData = CreateComplexTestData();

            // Act
            var errors = engine.ValidateData(complexData, "tB_01.02");

            // Assert
            // Should detect specific validation errors
            var mandatoryFieldErrors = errors.Where(e => e.ErrorType == "MANDATORY_FIELD_NULL").ToList();
            var regexErrors = errors.Where(e => e.ErrorType == "REGEX_PATTERN_MISMATCH").ToList();
            
            Assert.IsTrue(mandatoryFieldErrors.Count > 0, "Should detect missing mandatory fields");
            Assert.IsTrue(regexErrors.Count > 0, "Should detect pattern mismatches");
            
            // Verify error details
            foreach (var error in errors)
            {
                Assert.IsNotNull(error.RuleId);
                Assert.IsNotNull(error.Message);
                Assert.IsTrue(error.RowIndex > 0);
                Assert.IsNotNull(error.Column);
            }
        }

        [TestMethod]
        public void ErrorReporting_DetailedScenario_ShouldProvideActionableInformation()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var fileWithSpecificErrors = CreateFileWithKnownErrors();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(fileWithSpecificErrors);

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            var report = result.GetProperty("Report").GetString();
            
            // Verify report contains actionable information
            Assert.IsTrue(report.Contains("Row"), "Report should specify row numbers");
            Assert.IsTrue(report.Contains("Column"), "Report should specify column names");
            Assert.IsTrue(report.Contains("c00"), "Report should reference specific columns");
            
            // Verify report structure
            Assert.IsTrue(report.Contains("# ECB"), "Report should have proper header");
            Assert.IsTrue(report.Contains("Status"), "Report should include status");
            Assert.IsTrue(report.Contains("Error"), "Report should detail errors");
        }

        [TestMethod]
        public void PerformanceTest_LargeFile_ShouldMeetPerformanceRequirements()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var largeFile = ECBTestUtilities.CreateSampleECBFile(2000); // 2000 rows
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(largeFile);

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            plugin.Execute(_serviceProvider);
            stopwatch.Stop();

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            // Verify processing completed
            Assert.AreEqual("PASS", result.GetProperty("Status").GetString());
            
            // Performance requirements
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, 
                $"Large file validation should complete within 10 seconds, but took {stopwatch.ElapsedMilliseconds}ms");
            
            // Memory usage check (basic)
            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(false);
            Assert.IsTrue(memoryAfter < 200 * 1024 * 1024, // 200MB
                $"Memory usage should be reasonable, but used {memoryAfter / (1024 * 1024)}MB");
        }

        [TestMethod]
        public void ConcurrentExecution_MultipleValidations_ShouldHandleParallelRequests()
        {
            // Arrange
            const int concurrentRequests = 3;
            var tasks = new List<Task<bool>>();

            // Act
            for (int i = 0; i < concurrentRequests; i++)
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        var plugin = new ECBValidationPlugin();
                        var testFile = ECBTestUtilities.CreateSampleECBFile(50);
                        var context = new MockPluginExecutionContext();
                        var serviceProvider = new MockServiceProvider(
                            context, 
                            new MockOrganizationService(), 
                            new MockTracingService()
                        );
                        
                        context.InputParameters["UserExcelFile"] = Convert.ToBase64String(testFile);
                        plugin.Execute(serviceProvider);
                        
                        return context.OutputParameters.ContainsKey("ValidationResult");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Concurrent test failed: {ex.Message}");
                        return false;
                    }
                });
                
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            var results = tasks.Select(t => t.Result).ToList();
            Assert.AreEqual(concurrentRequests, results.Count);
            Assert.IsTrue(results.All(r => r), "All concurrent validations should succeed");
        }

        [TestMethod]
        public void ErrorRecovery_MalformedInput_ShouldHandleGracefully()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            
            // Test various malformed inputs
            var testCases = new[]
            {
                new byte[0],                          // Empty array
                new byte[] { 0x00 },                  // Single byte
                System.Text.Encoding.UTF8.GetBytes("not an excel file"), // Text data
                CreateCorruptedExcelFile()            // Corrupted Excel structure
            };

            foreach (var testCase in testCases)
            {
                // Arrange
                var context = new MockPluginExecutionContext();
                var serviceProvider = new MockServiceProvider(
                    context, 
                    new MockOrganizationService(), 
                    new MockTracingService()
                );
                
                context.InputParameters["UserExcelFile"] = Convert.ToBase64String(testCase);

                // Act & Assert
                try
                {
                    plugin.Execute(serviceProvider);
                    
                    // Should not throw, should return error status
                    Assert.IsTrue(context.OutputParameters.ContainsKey("ValidationResult"));
                    
                    var resultJson = context.OutputParameters["ValidationResult"].ToString();
                    var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
                    
                    Assert.AreEqual("ERROR", result.GetProperty("Status").GetString());
                }
                catch (InvalidPluginExecutionException)
                {
                    // Acceptable for completely invalid inputs
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Should handle malformed input gracefully, but threw: {ex.Message}");
                }
            }
        }

        #region Helper Methods

        private List<ValidationRule> CreateRealWorldTestRules()
        {
            return new List<ValidationRule>
            {
                new ValidationRule("ECB_001", "not(isnull({c0020}))", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0020" }, "mandatory_field", 1),
                new ValidationRule("ECB_002", "not(isnull({c0030}))", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0030" }, "mandatory_field", 2),
                new ValidationRule("ECB_003", "match({c0030}, \"^ENT[0-9]{3}$\")", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0030" }, "regex_validation", 3),
                new ValidationRule("ECB_004", "{c0040} >= 0", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0040" }, "value_constraint", 4),
                new ValidationRule("ECB_005", "{c0020} > 0", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0020" }, "value_constraint", 5)
            };
        }

        private List<Dictionary<string, object>> CreateComplexTestData()
        {
            return new List<Dictionary<string, object>>
            {
                // Valid row
                new Dictionary<string, object>
                {
                    { "c0020", 1001 },
                    { "c0030", "ENT001" },
                    { "c0040", 100 },
                    { "_row_index", 8 }
                },
                // Missing mandatory field
                new Dictionary<string, object>
                {
                    { "c0020", null },
                    { "c0030", "ENT002" },
                    { "c0040", 200 },
                    { "_row_index", 9 }
                },
                // Invalid pattern
                new Dictionary<string, object>
                {
                    { "c0020", 1003 },
                    { "c0030", "INVALID" },
                    { "c0040", 300 },
                    { "_row_index", 10 }
                },
                // Negative value
                new Dictionary<string, object>
                {
                    { "c0020", 1004 },
                    { "c0030", "ENT004" },
                    { "c0040", -50 },
                    { "_row_index", 11 }
                }
            };
        }

        private byte[] CreateFileWithKnownErrors()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                // Headers
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[6, 5].Value = "0030";
                worksheet.Cells[6, 6].Value = "0040";
                
                // Row 8: Missing c0020 (mandatory field error)
                worksheet.Cells[8, 4].Value = null;
                worksheet.Cells[8, 5].Value = "ENT001";
                worksheet.Cells[8, 6].Value = 100;
                
                // Row 9: Invalid c0030 pattern (regex error)
                worksheet.Cells[9, 4].Value = 1002;
                worksheet.Cells[9, 5].Value = "BADFORMAT";
                worksheet.Cells[9, 6].Value = 200;
                
                // Row 10: Negative c0040 (value constraint error)
                worksheet.Cells[10, 4].Value = 1003;
                worksheet.Cells[10, 5].Value = "ENT003";
                worksheet.Cells[10, 6].Value = -150;
                
                return package.GetAsByteArray();
            }
        }

        private byte[] CreateCorruptedExcelFile()
        {
            // Create a file that looks like Excel but has corrupted structure
            var validFile = ECBTestUtilities.CreateSampleECBFile(1);
            var corrupted = new byte[validFile.Length];
            Array.Copy(validFile, corrupted, validFile.Length);
            
            // Corrupt some bytes in the middle
            for (int i = 100; i < 200 && i < corrupted.Length; i++)
            {
                corrupted[i] = 0xFF;
            }
            
            return corrupted;
        }

        #endregion
    }

    /// <summary>
    /// End-to-end integration tests that simulate real Power Platform scenarios
    /// </summary>
    [TestClass]
    public class PowerPlatformIntegrationTests
    {
        [TestMethod]
        public void PowerAutomateScenario_FileUploadValidation_ShouldReturnStructuredResults()
        {
            // Arrange - Simulate Power Automate calling the plugin
            var plugin = new ECBValidationPlugin();
            var context = new MockPluginExecutionContext();
            var serviceProvider = new MockServiceProvider(
                context, 
                new MockOrganizationService(), 
                new MockTracingService()
            );

            // Simulate SharePoint file upload trigger
            var uploadedFile = ECBTestUtilities.CreateSampleECBFile(50);
            context.InputParameters["UserExcelFile"] = Convert.ToBase64String(uploadedFile);
            context.InputParameters["ECBRulesUrl"] = ""; // Use default
            
            // Act
            plugin.Execute(serviceProvider);

            // Assert - Verify Power Automate can consume the results
            Assert.IsTrue(context.OutputParameters.ContainsKey("ValidationResult"));
            
            var resultJson = context.OutputParameters["ValidationResult"].ToString();
            
            // Verify JSON structure matches Power Automate expectations
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            // Required properties for Power Automate conditions
            Assert.IsTrue(result.TryGetProperty("Status", out _));
            Assert.IsTrue(result.TryGetProperty("TotalErrors", out _));
            Assert.IsTrue(result.TryGetProperty("ProcessedSheets", out _));
            Assert.IsTrue(result.TryGetProperty("Report", out _));
            Assert.IsTrue(result.TryGetProperty("Timestamp", out _));
            
            // Verify values are appropriate for flow conditions
            var status = result.GetProperty("Status").GetString();
            Assert.IsTrue(status == "PASS" || status == "FAIL" || status == "ERROR");
            
            var totalErrors = result.GetProperty("TotalErrors").GetInt32();
            Assert.IsTrue(totalErrors >= 0);
        }

        [TestMethod]
        public void PowerAppsScenario_UserFileValidation_ShouldProvideUserFriendlyResults()
        {
            // Arrange - Simulate Power Apps user uploading a file
            var plugin = new ECBValidationPlugin();
            var context = new MockPluginExecutionContext();
            var serviceProvider = new MockServiceProvider(
                context, 
                new MockOrganizationService(), 
                new MockTracingService()
            );

            // User uploads file with some errors
            var userFile = ECBTestUtilities.CreateECBFileWithErrors();
            context.InputParameters["UserExcelFile"] = Convert.ToBase64String(userFile);
            
            // Act
            plugin.Execute(serviceProvider);

            // Assert
            var resultJson = context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            // Verify user-friendly information is available
            Assert.AreEqual("FAIL", result.GetProperty("Status").GetString());
            
            var report = result.GetProperty("Report").GetString();
            
            // Report should contain user-actionable information
            Assert.IsTrue(report.Contains("Row"), "Users need to know which rows have errors");
            Assert.IsTrue(report.Contains("Column"), "Users need to know which columns have errors");
            Assert.IsTrue(report.Length > 50, "Report should provide substantial detail");
            
            // Should be suitable for display in Power Apps
            Assert.IsFalse(report.Contains("Exception"), "Should not expose technical errors to users");
            Assert.IsFalse(report.Contains("Stack"), "Should not expose stack traces to users");
        }

        [TestMethod]
        public void SharePointIntegration_DocumentLibraryValidation_ShouldHandleFileMetadata()
        {
            // Arrange - Simulate SharePoint document library trigger
            var plugin = new ECBValidationPlugin();
            var context = new MockPluginExecutionContext();
            var serviceProvider = new MockServiceProvider(
                context, 
                new MockOrganizationService(), 
                new MockTracingService()
            );

            // Add SharePoint-style metadata to context
            context.SharedVariables["FileName"] = "ECB_Report_2025_Q1.xlsx";
            context.SharedVariables["FileSize"] = "1024000";
            context.SharedVariables["UploadedBy"] = "user@company.com";
            
            var documentFile = ECBTestUtilities.CreateSampleECBFile(100);
            context.InputParameters["UserExcelFile"] = Convert.ToBase64String(documentFile);
            
            // Act
            plugin.Execute(serviceProvider);

            // Assert
            var resultJson = context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            // Verify validation completed successfully
            Assert.AreEqual("PASS", result.GetProperty("Status").GetString());
            
            // Verify processing was logged appropriately
            var tracingService = (MockTracingService)serviceProvider.GetService(typeof(ITracingService));
            Assert.IsTrue(tracingService.Traces.Any(t => t.Contains("ECB Validation Plugin started")));
            Assert.IsTrue(tracingService.Traces.Any(t => t.Contains("completed successfully")));
        }
    }
}
