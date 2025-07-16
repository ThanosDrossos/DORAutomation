using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using OfficeOpenXml;
using ECBValidation;

namespace ECBValidation.Tests
{
    [TestClass]
    public class ECBValidationPluginTests
    {
        private MockServiceProvider _serviceProvider;
        private MockPluginExecutionContext _context;
        private MockOrganizationService _organizationService;
        private MockTracingService _tracingService;

        [TestInitialize]
        public void TestInitialize()
        {
            // Set EPPlus license context for testing
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Initialize mock services
            _context = new MockPluginExecutionContext();
            _organizationService = new MockOrganizationService();
            _tracingService = new MockTracingService();
            _serviceProvider = new MockServiceProvider(_context, _organizationService, _tracingService);
        }

        [TestMethod]
        public void Execute_WithValidExcelFile_ShouldReturnPassStatus()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var validExcelFile = CreateValidTestExcelFile();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(validExcelFile);
            _context.InputParameters["ECBRulesUrl"] = ""; // Use default
            _context.InputParameters["TableFilter"] = "";

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            Assert.IsTrue(_context.OutputParameters.ContainsKey("ValidationResult"));
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            Assert.AreEqual("PASS", result.GetProperty("Status").GetString());
            Assert.AreEqual(0, result.GetProperty("TotalErrors").GetInt32());
            Assert.IsTrue(result.GetProperty("ProcessedSheets").GetArrayLength() > 0);
        }

        [TestMethod]
        public void Execute_WithInvalidExcelFile_ShouldReturnFailStatus()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var invalidExcelFile = CreateInvalidTestExcelFile();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(invalidExcelFile);

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            Assert.AreEqual("FAIL", result.GetProperty("Status").GetString());
            Assert.IsTrue(result.GetProperty("TotalErrors").GetInt32() > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Execute_WithMissingUserExcelFile_ShouldThrowException()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            // Don't set UserExcelFile parameter

            // Act
            plugin.Execute(_serviceProvider);

            // Assert - Exception expected
        }

        [TestMethod]
        public void Execute_WithEmptyExcelFile_ShouldReturnPassWithNoErrors()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var emptyExcelFile = CreateEmptyTestExcelFile();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(emptyExcelFile);

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            Assert.AreEqual("PASS", result.GetProperty("Status").GetString());
            Assert.AreEqual(0, result.GetProperty("TotalErrors").GetInt32());
        }

        [TestMethod]
        public void Execute_WithTableFilter_ShouldProcessOnlyFilteredSheets()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var multiSheetExcelFile = CreateMultiSheetTestExcelFile();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(multiSheetExcelFile);
            _context.InputParameters["TableFilter"] = "tB_01";

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            var processedSheets = result.GetProperty("ProcessedSheets");
            Assert.IsTrue(processedSheets.GetArrayLength() > 0);
            
            // Verify only tB_01 sheets were processed
            foreach (var sheet in processedSheets.EnumerateArray())
            {
                Assert.IsTrue(sheet.GetString().StartsWith("tB_01"));
            }
        }

        [TestMethod]
        public void Execute_WithMalformedExcelFile_ShouldReturnErrorStatus()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var malformedFile = new byte[] { 0x00, 0x01, 0x02, 0x03 }; // Not a valid Excel file
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(malformedFile);

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            var resultJson = _context.OutputParameters["ValidationResult"].ToString();
            var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
            
            Assert.AreEqual("ERROR", result.GetProperty("Status").GetString());
        }

        [TestMethod]
        public void Execute_ShouldLogTraceInformation()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var validExcelFile = CreateValidTestExcelFile();
            
            _context.InputParameters["UserExcelFile"] = Convert.ToBase64String(validExcelFile);

            // Act
            plugin.Execute(_serviceProvider);

            // Assert
            Assert.IsTrue(_tracingService.Traces.Count > 0);
            Assert.IsTrue(_tracingService.Traces.Any(t => t.Contains("ECB Validation Plugin started")));
            Assert.IsTrue(_tracingService.Traces.Any(t => t.Contains("completed successfully")));
        }

        #region Helper Methods for Creating Test Files

        private byte[] CreateValidTestExcelFile()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                // Add column headers in row 6, starting from column D (index 4)
                worksheet.Cells[6, 4].Value = "0020"; // c0020
                worksheet.Cells[6, 5].Value = "0030"; // c0030
                worksheet.Cells[6, 6].Value = "0040"; // c0040
                worksheet.Cells[6, 7].Value = "0050"; // c0050
                
                // Add valid sample data starting from row 8
                worksheet.Cells[8, 4].Value = 100;        // c0020 - numeric value
                worksheet.Cells[8, 5].Value = "ENT001";   // c0030 - entity code
                worksheet.Cells[8, 6].Value = 50;         // c0040 - numeric value
                worksheet.Cells[8, 7].Value = 1000;       // c0050 - numeric value
                
                worksheet.Cells[9, 4].Value = 200;
                worksheet.Cells[9, 5].Value = "ENT002";
                worksheet.Cells[9, 6].Value = 75;
                worksheet.Cells[9, 7].Value = 1200;
                
                worksheet.Cells[10, 4].Value = 150;
                worksheet.Cells[10, 5].Value = "ENT003";
                worksheet.Cells[10, 6].Value = 60;
                worksheet.Cells[10, 7].Value = 1500;
                
                return package.GetAsByteArray();
            }
        }

        private byte[] CreateInvalidTestExcelFile()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                // Add column headers
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[6, 5].Value = "0030";
                worksheet.Cells[6, 6].Value = "0040";
                
                // Add invalid data (missing required fields, invalid formats)
                worksheet.Cells[8, 4].Value = 100;
                worksheet.Cells[8, 5].Value = null;       // Missing required field
                worksheet.Cells[8, 6].Value = 50;
                
                worksheet.Cells[9, 4].Value = null;       // Missing required field
                worksheet.Cells[9, 5].Value = "ENT002";
                worksheet.Cells[9, 6].Value = -10;        // Invalid negative value
                
                worksheet.Cells[10, 4].Value = 0;         // Zero value (might be invalid)
                worksheet.Cells[10, 5].Value = "INVALID"; // Invalid entity format
                worksheet.Cells[10, 6].Value = 999999;    // Value too large
                
                return package.GetAsByteArray();
            }
        }

        private byte[] CreateEmptyTestExcelFile()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                // Add only column headers, no data
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[6, 5].Value = "0030";
                worksheet.Cells[6, 6].Value = "0040";
                
                // No data rows added
                
                return package.GetAsByteArray();
            }
        }

        private byte[] CreateMultiSheetTestExcelFile()
        {
            using (var package = new ExcelPackage())
            {
                // Create multiple sheets
                var sheets = new[] { "tB_01.02", "tB_01.03", "tB_02.01", "tB_03.01" };
                
                foreach (var sheetName in sheets)
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);
                    
                    // Add column headers
                    worksheet.Cells[6, 4].Value = "0020";
                    worksheet.Cells[6, 5].Value = "0030";
                    
                    // Add sample data
                    worksheet.Cells[8, 4].Value = 100;
                    worksheet.Cells[8, 5].Value = "ENT001";
                }
                
                return package.GetAsByteArray();
            }
        }

        #endregion
    }

    [TestClass]
    public class ECBRuleExtractorTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [TestMethod]
        public void ExtractRules_WithValidECBFile_ShouldExtractRules()
        {
            // Arrange
            var extractor = new ECBRuleExtractor();
            var testECBFile = CreateMockECBRulesFile();

            // Act
            var rules = extractor.ExtractRules(testECBFile);

            // Assert
            Assert.IsTrue(rules.Count > 0);
            Assert.IsTrue(rules.All(r => !string.IsNullOrEmpty(r.Id)));
            Assert.IsTrue(rules.All(r => !string.IsNullOrEmpty(r.Expression)));
            Assert.IsTrue(rules.All(r => !string.IsNullOrEmpty(r.RuleType)));
        }

        [TestMethod]
        public void ExtractRules_ShouldClassifyRuleTypesCorrectly()
        {
            // Arrange
            var extractor = new ECBRuleExtractor();
            var testFile = CreateMockECBRulesFileWithSpecificRules();

            // Act
            var rules = extractor.ExtractRules(testFile);

            // Assert
            Assert.IsTrue(rules.Any(r => r.RuleType == "mandatory_field"));
            Assert.IsTrue(rules.Any(r => r.RuleType == "regex_validation"));
            Assert.IsTrue(rules.Any(r => r.RuleType == "value_constraint"));
        }

        private byte[] CreateMockECBRulesFile()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Rules");
                
                // Mock ECB validation rules
                var rules = new[]
                {
                    "with {tB_01.02, c0020}: not(isnull({c0020}))",
                    "with {tB_01.02, c0030}: match({c0030}, \"^ENT[0-9]{3}$\")",
                    "with {tB_01.02, c0040}: {c0040} >= 0",
                    "with {tB_02.01, c0050}: if {c0020} > 0 then not(isnull({c0050})) endif"
                };
                
                for (int i = 0; i < rules.Length; i++)
                {
                    worksheet.Cells[i + 1, 1].Value = rules[i];
                }
                
                return package.GetAsByteArray();
            }
        }

        private byte[] CreateMockECBRulesFileWithSpecificRules()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Rules");
                
                // Specific rules for type classification testing
                worksheet.Cells[1, 1].Value = "with {tB_01.02, c0020}: not(isnull({c0020}))"; // mandatory_field
                worksheet.Cells[2, 1].Value = "with {tB_01.02, c0030}: match({c0030}, \"^[A-Z]+$\")"; // regex_validation
                worksheet.Cells[3, 1].Value = "with {tB_01.02, c0040}: {c0040} >= 100"; // value_constraint
                worksheet.Cells[4, 1].Value = "with {tB_01.02, c0050}: if {c0020} > 0 then {c0050} = 1 endif"; // conditional_rule
                
                return package.GetAsByteArray();
            }
        }
    }

    [TestClass]
    public class ECBValidationEngineTests
    {
        private List<ValidationRule> _testRules;

        [TestInitialize]
        public void TestInitialize()
        {
            _testRules = new List<ValidationRule>
            {
                new ValidationRule("RULE_001", "not(isnull({c0020}))", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0020" }, "mandatory_field", 1),
                new ValidationRule("RULE_002", "match({c0030}, \"^ENT[0-9]{3}$\")", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0030" }, "regex_validation", 2),
                new ValidationRule("RULE_003", "{c0040} >= 0", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0040" }, "value_constraint", 3)
            };
        }

        [TestMethod]
        public void ValidateData_WithValidData_ShouldReturnNoErrors()
        {
            // Arrange
            var engine = new ECBValidationEngine(_testRules);
            var validData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "c0020", 100 },
                    { "c0030", "ENT001" },
                    { "c0040", 50 },
                    { "_row_index", 8 }
                }
            };

            // Act
            var errors = engine.ValidateData(validData, "tB_01.02");

            // Assert
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void ValidateData_WithMissingMandatoryField_ShouldReturnErrors()
        {
            // Arrange
            var engine = new ECBValidationEngine(_testRules);
            var invalidData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "c0020", null }, // Missing mandatory field
                    { "c0030", "ENT001" },
                    { "c0040", 50 },
                    { "_row_index", 8 }
                }
            };

            // Act
            var errors = engine.ValidateData(invalidData, "tB_01.02");

            // Assert
            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Any(e => e.ErrorType == "MANDATORY_FIELD_NULL"));
        }

        [TestMethod]
        public void ValidateData_WithInvalidRegexPattern_ShouldReturnErrors()
        {
            // Arrange
            var engine = new ECBValidationEngine(_testRules);
            var invalidData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "c0020", 100 },
                    { "c0030", "INVALID" }, // Invalid pattern
                    { "c0040", 50 },
                    { "_row_index", 8 }
                }
            };

            // Act
            var errors = engine.ValidateData(invalidData, "tB_01.02");

            // Assert
            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Any(e => e.ErrorType == "REGEX_PATTERN_MISMATCH"));
        }
    }

    [TestClass]
    public class ECBExcelValidatorTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [TestMethod]
        public void ValidateExcelFile_WithValidFile_ShouldReturnPassResult()
        {
            // Arrange
            var mockRules = new List<ValidationRule>
            {
                new ValidationRule("RULE_001", "not(isnull({c0020}))", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0020" }, "mandatory_field", 1)
            };
            var engine = new ECBValidationEngine(mockRules);
            var validator = new ECBExcelValidator(engine);
            var validFile = CreateValidTestFile();

            // Act
            var result = validator.ValidateExcelFile(validFile);

            // Assert
            Assert.AreEqual("PASS", result.OverallStatus);
            Assert.AreEqual(0, result.TotalErrors);
            Assert.IsTrue(result.SheetsProcessed.Count > 0);
        }

        [TestMethod]
        public void ValidateExcelFile_WithInvalidFile_ShouldReturnFailResult()
        {
            // Arrange
            var mockRules = new List<ValidationRule>
            {
                new ValidationRule("RULE_001", "not(isnull({c0020}))", 
                    new List<string> { "tB_01.02" }, new List<string> { "c0020" }, "mandatory_field", 1)
            };
            var engine = new ECBValidationEngine(mockRules);
            var validator = new ECBExcelValidator(engine);
            var invalidFile = CreateInvalidTestFile();

            // Act
            var result = validator.ValidateExcelFile(invalidFile);

            // Assert
            Assert.AreEqual("FAIL", result.OverallStatus);
            Assert.IsTrue(result.TotalErrors > 0);
        }

        private byte[] CreateValidTestFile()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[8, 4].Value = 100; // Valid data
                
                return package.GetAsByteArray();
            }
        }

        private byte[] CreateInvalidTestFile()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[8, 4].Value = null; // Invalid data (null mandatory field)
                
                return package.GetAsByteArray();
            }
        }
    }

    #region Performance Tests

    [TestClass]
    public class ECBValidationPerformanceTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [TestMethod]
        public void Execute_WithLargeFile_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var plugin = new ECBValidationPlugin();
            var largeFile = CreateLargeTestFile(1000); // 1000 rows
            var context = new MockPluginExecutionContext();
            var serviceProvider = new MockServiceProvider(
                context, 
                new MockOrganizationService(), 
                new MockTracingService()
            );
            
            context.InputParameters["UserExcelFile"] = Convert.ToBase64String(largeFile);

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            plugin.Execute(serviceProvider);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, "Validation should complete within 10 seconds");
            Assert.IsTrue(context.OutputParameters.ContainsKey("ValidationResult"));
        }

        [TestMethod]
        public void Execute_ConcurrentValidations_ShouldHandleMultipleRequests()
        {
            // Arrange
            const int concurrentRequests = 5;
            var tasks = new List<System.Threading.Tasks.Task>();
            var results = new List<bool>();

            // Act
            for (int i = 0; i < concurrentRequests; i++)
            {
                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var plugin = new ECBValidationPlugin();
                        var testFile = CreateValidTestFile();
                        var context = new MockPluginExecutionContext();
                        var serviceProvider = new MockServiceProvider(
                            context, 
                            new MockOrganizationService(), 
                            new MockTracingService()
                        );
                        
                        context.InputParameters["UserExcelFile"] = Convert.ToBase64String(testFile);
                        plugin.Execute(serviceProvider);
                        
                        lock (results)
                        {
                            results.Add(context.OutputParameters.ContainsKey("ValidationResult"));
                        }
                    }
                    catch
                    {
                        lock (results)
                        {
                            results.Add(false);
                        }
                    }
                });
                
                tasks.Add(task);
            }

            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.AreEqual(concurrentRequests, results.Count);
            Assert.IsTrue(results.All(r => r), "All concurrent validations should succeed");
        }

        private byte[] CreateLargeTestFile(int rowCount)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                // Headers
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[6, 5].Value = "0030";
                worksheet.Cells[6, 6].Value = "0040";
                
                // Data
                var random = new Random();
                for (int i = 0; i < rowCount; i++)
                {
                    var row = 8 + i;
                    worksheet.Cells[row, 4].Value = random.Next(100, 1000);
                    worksheet.Cells[row, 5].Value = $"ENT{random.Next(1, 999):D3}";
                    worksheet.Cells[row, 6].Value = random.Next(50, 200);
                }
                
                return package.GetAsByteArray();
            }
        }

        private byte[] CreateValidTestFile()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[8, 4].Value = 100;
                
                return package.GetAsByteArray();
            }
        }
    }

    #endregion
}
