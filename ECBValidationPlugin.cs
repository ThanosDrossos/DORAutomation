using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using OfficeOpenXml;

namespace ECBValidation
{
    /// <summary>
    /// ECB Excel Validation Plugin for Microsoft Dataverse
    /// Extracts validation rules from ECB Excel file and validates user data
    /// Can be called from Power Automate flows
    /// </summary>
    public class ECBValidationPlugin : IPlugin
    {
        private const string ECB_RULES_URL = "https://eba.europa.eu/sites/default/files/2025-04/10100a51-275f-4c98-96a1-f81342a8f57d/Overview%20of%20the%20RoI%20reporting%20technical%20checks%20and%20validation%20rules%20%28updated%2028%20April%202025%29%20%284%29.xlsx";

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                tracingService.Trace("ECB Validation Plugin started");

                // Get input parameters from the request
                var validationRequest = GetValidationRequestFromContext(context, tracingService);
                
                // Extract rules from ECB Excel file
                var rules = ExtractECBRules(validationRequest.ECBRulesSource, tracingService).Result;
                tracingService.Trace($"Extracted {rules.Count} validation rules");

                // Validate the user Excel file
                var validationResults = ValidateUserExcelFile(validationRequest.UserExcelData, rules, tracingService);

                // Generate validation report
                var report = GenerateValidationReport(validationResults, tracingService);

                // Set output parameters
                context.OutputParameters["ValidationResult"] = JsonSerializer.Serialize(new
                {
                    Status = validationResults.OverallStatus,
                    TotalErrors = validationResults.TotalErrors,
                    Report = report,
                    ProcessedSheets = validationResults.ProcessedSheets,
                    Timestamp = DateTime.UtcNow
                });

                tracingService.Trace("ECB Validation Plugin completed successfully");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error in ECB Validation Plugin: {ex.Message}");
                throw new InvalidPluginExecutionException($"ECB Validation failed: {ex.Message}", ex);
            }
        }

        #region Input/Output Models

        private class ValidationRequest
        {
            public string ECBRulesSource { get; set; } // URL or base64 encoded Excel file
            public byte[] UserExcelData { get; set; }
            public string TableFilter { get; set; } // Optional: filter for specific tables
        }

        private class ValidationRule
        {
            public string Id { get; set; }
            public string Expression { get; set; }
            public List<string> TableReferences { get; set; } = new List<string>();
            public List<string> ColumnReferences { get; set; } = new List<string>();
            public string RuleType { get; set; }
            public int SourceRow { get; set; }
        }

        private class ValidationResults
        {
            public string OverallStatus { get; set; }
            public int TotalErrors { get; set; }
            public List<string> ProcessedSheets { get; set; } = new List<string>();
            public Dictionary<string, SheetValidationResult> SheetResults { get; set; } = new Dictionary<string, SheetValidationResult>();
        }

        private class SheetValidationResult
        {
            public string SheetName { get; set; }
            public string Status { get; set; }
            public int ErrorCount { get; set; }
            public int DataRows { get; set; }
            public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        }

        private class ValidationError
        {
            public string RuleId { get; set; }
            public string RuleType { get; set; }
            public int RowIndex { get; set; }
            public string Column { get; set; }
            public string ErrorType { get; set; }
            public string Message { get; set; }
            public string Expression { get; set; }
        }

        #endregion

        #region Input Processing

        private ValidationRequest GetValidationRequestFromContext(IPluginExecutionContext context, ITracingService tracingService)
        {
            var request = new ValidationRequest();

            // Get ECB rules source (URL or file data)
            if (context.InputParameters.Contains("ECBRulesUrl"))
            {
                request.ECBRulesSource = context.InputParameters["ECBRulesUrl"].ToString();
                tracingService.Trace($"Using ECB rules from URL: {request.ECBRulesSource}");
            }
            else
            {
                request.ECBRulesSource = ECB_RULES_URL; // Default URL
                tracingService.Trace("Using default ECB rules URL");
            }

            // Get user Excel file data
            if (context.InputParameters.Contains("UserExcelFile"))
            {
                var fileData = context.InputParameters["UserExcelFile"];
                if (fileData is string base64Data)
                {
                    request.UserExcelData = Convert.FromBase64String(base64Data);
                }
                else if (fileData is byte[] bytes)
                {
                    request.UserExcelData = bytes;
                }
                else
                {
                    throw new InvalidPluginExecutionException("UserExcelFile must be provided as base64 string or byte array");
                }
                tracingService.Trace($"User Excel file loaded: {request.UserExcelData.Length} bytes");
            }
            else
            {
                throw new InvalidPluginExecutionException("UserExcelFile parameter is required");
            }

            // Optional table filter
            if (context.InputParameters.Contains("TableFilter"))
            {
                request.TableFilter = context.InputParameters["TableFilter"].ToString();
                tracingService.Trace($"Table filter applied: {request.TableFilter}");
            }

            return request;
        }

        #endregion

        #region ECB Rules Extraction

        private async Task<List<ValidationRule>> ExtractECBRules(string source, ITracingService tracingService)
        {
            byte[] excelData;

            // Download or get Excel file data
            if (source.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                excelData = await DownloadECBRulesFile(source, tracingService);
            }
            else
            {
                // Assume it's base64 encoded data
                excelData = Convert.FromBase64String(source);
            }

            return ExtractRulesFromExcelData(excelData, tracingService);
        }

        private async Task<byte[]> DownloadECBRulesFile(string url, ITracingService tracingService)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", 
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                
                tracingService.Trace($"Downloading ECB rules from: {url}");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var data = await response.Content.ReadAsByteArrayAsync();
                tracingService.Trace($"Downloaded {data.Length} bytes");
                return data;
            }
        }

        private List<ValidationRule> ExtractRulesFromExcelData(byte[] excelData, ITracingService tracingService)
        {
            var rules = new List<ValidationRule>();
            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var stream = new MemoryStream(excelData))
            using (var package = new ExcelPackage(stream))
            {
                // Scan all worksheets for validation expressions
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    tracingService.Trace($"Scanning worksheet: {worksheet.Name}");
                    
                    var expressions = ScanWorksheetForValidationExpressions(worksheet, tracingService);
                    
                    foreach (var (expression, row) in expressions)
                    {
                        var rule = ParseValidationExpression(expression, row, rules.Count + 1);
                        if (rule != null)
                        {
                            rules.Add(rule);
                        }
                    }
                }
            }

            tracingService.Trace($"Extracted {rules.Count} validation rules total");
            return rules;
        }

        private List<(string expression, int row)> ScanWorksheetForValidationExpressions(ExcelWorksheet worksheet, ITracingService tracingService)
        {
            var expressions = new List<(string, int)>();
            
            // Scan all cells for validation expressions
            for (int row = 1; row <= worksheet.Dimension?.Rows; row++)
            {
                for (int col = 1; col <= worksheet.Dimension?.Columns; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value?.ToString();
                    
                    if (!string.IsNullOrEmpty(cellValue) && IsValidECBRuleExpression(cellValue))
                    {
                        expressions.Add((cellValue, row));
                    }
                }
            }
            
            tracingService.Trace($"Found {expressions.Count} expressions in {worksheet.Name}");
            return expressions;
        }

        private bool IsValidECBRuleExpression(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 10) return false;

            // Check for ECB validation rule patterns
            var patterns = new[]
            {
                @"with\s*\{[^}]+\}.*:",           // "with {tB_XX.XX, ...}:"
                @"match\s*\(\s*\{[^}]+\}",       // "match({tB_XX.XX, cXXXX}..."
                @"\{c\d{4}\}",                   // Column references like {c0020}
                @"tB_\d{2}\.\d{2}",              // Table references like tB_01.02
                @"isnull\s*\(",                  // isnull function
                @"not\s*\(\s*isnull"             // not(isnull(...))
            };

            int patternMatches = patterns.Count(pattern => Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase));
            return patternMatches >= 2;
        }

        private ValidationRule ParseValidationExpression(string expression, int sourceRow, int ruleNumber)
        {
            try
            {
                var rule = new ValidationRule
                {
                    Id = $"ECB_RULE_{ruleNumber:000}",
                    Expression = expression.Trim(),
                    SourceRow = sourceRow
                };

                // Extract table references (e.g., tB_01.02)
                var tableMatches = Regex.Matches(expression, @"tB_\d{2}\.\d{2}");
                rule.TableReferences = tableMatches.Cast<Match>().Select(m => m.Value).Distinct().ToList();

                // Extract column references
                ExtractColumnReferences(expression, rule);

                // Classify rule type
                rule.RuleType = ClassifyRuleType(expression);

                return rule;
            }
            catch
            {
                return null; // Skip malformed rules
            }
        }

        private void ExtractColumnReferences(string expression, ValidationRule rule)
        {
            // Pattern 1: Individual columns {c0020}
            var individualCols = Regex.Matches(expression, @"\{c(\d{4})\}");
            rule.ColumnReferences.AddRange(individualCols.Cast<Match>().Select(m => $"c{m.Groups[1].Value}"));

            // Pattern 2: Column ranges {c0020-0090}
            var rangeMatches = Regex.Matches(expression, @"\{c(\d{4})-(\d{4})\}");
            foreach (Match match in rangeMatches)
            {
                rule.ColumnReferences.Add($"c{match.Groups[1].Value}-{match.Groups[2].Value}");
            }

            // Pattern 3: Column wildcards {c*}
            if (Regex.IsMatch(expression, @"\{c\*\}"))
            {
                rule.ColumnReferences.Add("c*");
            }

            // Pattern 4: Column lists {(c0020, c0030, c0040)}
            var listMatches = Regex.Matches(expression, @"\{\([^)]+\)\}");
            foreach (Match listMatch in listMatches)
            {
                var colsInList = Regex.Matches(listMatch.Value, @"c(\d{4})");
                rule.ColumnReferences.AddRange(colsInList.Cast<Match>().Select(m => $"c{m.Groups[1].Value}"));
            }

            // Remove duplicates
            rule.ColumnReferences = rule.ColumnReferences.Distinct().ToList();
        }

        private string ClassifyRuleType(string expression)
        {
            var expr = expression.ToLowerInvariant();

            if (expr.Contains("match(")) return "regex_validation";
            if (expr.Contains("isnull") && expr.Contains("not")) return "mandatory_field";
            if (new[] { ">=", "<=", ">", "<", "!=" }.Any(op => expr.Contains(op))) return "value_constraint";
            if (expr.Contains("if") && expr.Contains("then")) return "conditional_rule";
            if (expr.Contains("=") && !expr.Contains("if")) return "equality_check";
            
            return "complex_validation";
        }

        #endregion

        #region User Excel Validation

        private ValidationResults ValidateUserExcelFile(byte[] userExcelData, List<ValidationRule> rules, ITracingService tracingService)
        {
            var results = new ValidationResults
            {
                OverallStatus = "PASS",
                TotalErrors = 0
            };

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream(userExcelData))
            using (var package = new ExcelPackage(stream))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    // Only process ECB table sheets (tB_XX.XX format)
                    if (IsECBTableSheet(worksheet.Name))
                    {
                        tracingService.Trace($"Validating sheet: {worksheet.Name}");
                        
                        var sheetResult = ValidateWorksheet(worksheet, rules, tracingService);
                        results.SheetResults[worksheet.Name] = sheetResult;
                        results.ProcessedSheets.Add(worksheet.Name);
                        results.TotalErrors += sheetResult.ErrorCount;
                    }
                }
            }

            results.OverallStatus = results.TotalErrors == 0 ? "PASS" : "FAIL";
            tracingService.Trace($"Validation completed: {results.TotalErrors} total errors");

            return results;
        }

        private bool IsECBTableSheet(string sheetName)
        {
            return Regex.IsMatch(sheetName, @"tB_\d{2}\.\d{2}", RegexOptions.IgnoreCase);
        }

        private SheetValidationResult ValidateWorksheet(ExcelWorksheet worksheet, List<ValidationRule> rules, ITracingService tracingService)
        {
            var result = new SheetValidationResult
            {
                SheetName = worksheet.Name,
                Status = "PASS"
            };

            try
            {
                // Read ECB sheet structure (columns from D6, data from row 8)
                var data = ReadECBSheetStructure(worksheet, tracingService);
                result.DataRows = data.Count;

                if (data.Count == 0)
                {
                    tracingService.Trace($"No data found in sheet {worksheet.Name}");
                    return result;
                }

                // Get applicable rules for this sheet
                var applicableRules = GetApplicableRules(rules, worksheet.Name);
                tracingService.Trace($"Applying {applicableRules.Count} rules to sheet {worksheet.Name}");

                // Apply each rule
                foreach (var rule in applicableRules)
                {
                    var ruleErrors = ApplyValidationRule(rule, data, worksheet.Name);
                    result.Errors.AddRange(ruleErrors);
                }

                result.ErrorCount = result.Errors.Count;
                result.Status = result.ErrorCount == 0 ? "PASS" : "FAIL";
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error validating sheet {worksheet.Name}: {ex.Message}");
                result.Errors.Add(new ValidationError
                {
                    ErrorType = "PROCESSING_ERROR",
                    Message = $"Error processing sheet: {ex.Message}"
                });
                result.ErrorCount = 1;
                result.Status = "ERROR";
            }

            return result;
        }

        private List<Dictionary<string, object>> ReadECBSheetStructure(ExcelWorksheet worksheet, ITracingService tracingService)
        {
            var data = new List<Dictionary<string, object>>();

            if (worksheet.Dimension == null || worksheet.Dimension.Rows < 8)
            {
                return data;
            }

            // Extract column mapping from row 6, starting from column D (4)
            var columnMapping = new Dictionary<int, string>();
            
            for (int col = 4; col <= worksheet.Dimension.Columns; col++) // Start from column D
            {
                var cellValue = worksheet.Cells[6, col].Value?.ToString(); // Row 6
                if (!string.IsNullOrEmpty(cellValue) && cellValue.All(char.IsDigit))
                {
                    columnMapping[col] = $"c{cellValue.PadLeft(4, '0')}";
                }
            }

            if (columnMapping.Count == 0)
            {
                tracingService.Trace($"No column mapping found in sheet {worksheet.Name}");
                return data;
            }

            // Extract data starting from row 8
            for (int row = 8; row <= worksheet.Dimension.Rows; row++)
            {
                var rowData = new Dictionary<string, object>();
                bool hasData = false;

                foreach (var (colIndex, colName) in columnMapping)
                {
                    var cellValue = worksheet.Cells[row, colIndex].Value;
                    if (cellValue != null)
                    {
                        rowData[colName] = cellValue;
                        hasData = true;
                    }
                    else
                    {
                        rowData[colName] = null;
                    }
                }

                if (hasData)
                {
                    rowData["_row_index"] = row;
                    data.Add(rowData);
                }
            }

            tracingService.Trace($"Extracted {data.Count} data rows from {worksheet.Name}");
            return data;
        }

        private List<ValidationRule> GetApplicableRules(List<ValidationRule> rules, string sheetName)
        {
            return rules.Where(rule => 
                !rule.TableReferences.Any() || 
                rule.TableReferences.Any(tableRef => sheetName.Contains(tableRef, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        private List<ValidationError> ApplyValidationRule(ValidationRule rule, List<Dictionary<string, object>> data, string sheetName)
        {
            var errors = new List<ValidationError>();

            try
            {
                switch (rule.RuleType)
                {
                    case "mandatory_field":
                        errors.AddRange(ValidateMandatoryFields(rule, data));
                        break;
                    case "value_constraint":
                        errors.AddRange(ValidateValueConstraints(rule, data));
                        break;
                    case "regex_validation":
                        errors.AddRange(ValidateRegexPatterns(rule, data));
                        break;
                    case "conditional_rule":
                        errors.AddRange(ValidateConditionalRules(rule, data));
                        break;
                    // Add more validation types as needed
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError
                {
                    RuleId = rule.Id,
                    ErrorType = "RULE_PROCESSING_ERROR",
                    Message = $"Error processing rule {rule.Id}: {ex.Message}",
                    Expression = rule.Expression
                });
            }

            return errors;
        }

        private List<ValidationError> ValidateMandatoryFields(ValidationRule rule, List<Dictionary<string, object>> data)
        {
            var errors = new List<ValidationError>();

            foreach (var columnRef in rule.ColumnReferences)
            {
                var mappedColumns = MapColumnReference(columnRef, data.FirstOrDefault()?.Keys ?? new string[0]);
                
                foreach (var column in mappedColumns)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        var row = data[i];
                        if (!row.ContainsKey(column) || row[column] == null || string.IsNullOrEmpty(row[column].ToString()))
                        {
                            errors.Add(new ValidationError
                            {
                                RuleId = rule.Id,
                                RuleType = rule.RuleType,
                                RowIndex = (int)(row["_row_index"] ?? i + 8),
                                Column = column,
                                ErrorType = "MANDATORY_FIELD_NULL",
                                Message = $"Required field {column} is null or empty",
                                Expression = rule.Expression
                            });
                        }
                    }
                }
            }

            return errors;
        }

        private List<ValidationError> ValidateValueConstraints(ValidationRule rule, List<Dictionary<string, object>> data)
        {
            var errors = new List<ValidationError>();
            
            // Extract constraint from expression (simplified implementation)
            var expression = rule.Expression.ToLowerInvariant();
            
            // Look for numeric constraints
            var constraintPatterns = new Dictionary<string, string>
            {
                [">="] = "greater than or equal to",
                ["<="] = "less than or equal to", 
                [">"] = "greater than",
                ["<"] = "less than",
                ["!="] = "not equal to"
            };

            foreach (var (constraint, description) in constraintPatterns)
            {
                if (expression.Contains(constraint))
                {
                    // Try to extract the constraint value (simplified)
                    var match = Regex.Match(expression, $@"\{{\w+\}}\s*{Regex.Escape(constraint)}\s*(\d+)");
                    if (match.Success && double.TryParse(match.Groups[1].Value, out double constraintValue))
                    {
                        foreach (var columnRef in rule.ColumnReferences)
                        {
                            var mappedColumns = MapColumnReference(columnRef, data.FirstOrDefault()?.Keys ?? new string[0]);
                            
                            foreach (var column in mappedColumns)
                            {
                                ValidateNumericConstraint(rule, data, column, constraint, constraintValue, description, errors);
                            }
                        }
                    }
                }
            }

            return errors;
        }

        private void ValidateNumericConstraint(ValidationRule rule, List<Dictionary<string, object>> data, 
            string column, string constraint, double constraintValue, string description, List<ValidationError> errors)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];
                if (row.ContainsKey(column) && row[column] != null)
                {
                    if (double.TryParse(row[column].ToString(), out double value))
                    {
                        bool violatesConstraint = constraint switch
                        {
                            ">=" => value < constraintValue,
                            "<=" => value > constraintValue,
                            ">" => value <= constraintValue,
                            "<" => value >= constraintValue,
                            "!=" => value == constraintValue,
                            _ => false
                        };

                        if (violatesConstraint)
                        {
                            errors.Add(new ValidationError
                            {
                                RuleId = rule.Id,
                                RuleType = rule.RuleType,
                                RowIndex = (int)(row["_row_index"] ?? i + 8),
                                Column = column,
                                ErrorType = "VALUE_CONSTRAINT_VIOLATION",
                                Message = $"Value {value} in {column} must be {description} {constraintValue}",
                                Expression = rule.Expression
                            });
                        }
                    }
                }
            }
        }

        private List<ValidationError> ValidateRegexPatterns(ValidationRule rule, List<Dictionary<string, object>> data)
        {
            var errors = new List<ValidationError>();

            // Extract regex pattern from expression
            var patternMatch = Regex.Match(rule.Expression, @"""([^""]+)""");
            if (!patternMatch.Success) return errors;

            var regexPattern = patternMatch.Groups[1].Value;

            foreach (var columnRef in rule.ColumnReferences)
            {
                var mappedColumns = MapColumnReference(columnRef, data.FirstOrDefault()?.Keys ?? new string[0]);
                
                foreach (var column in mappedColumns)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        var row = data[i];
                        if (row.ContainsKey(column) && row[column] != null)
                        {
                            var value = row[column].ToString();
                            if (!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, regexPattern))
                            {
                                errors.Add(new ValidationError
                                {
                                    RuleId = rule.Id,
                                    RuleType = rule.RuleType,
                                    RowIndex = (int)(row["_row_index"] ?? i + 8),
                                    Column = column,
                                    ErrorType = "REGEX_PATTERN_MISMATCH",
                                    Message = $"Value '{value}' in {column} does not match required pattern {regexPattern}",
                                    Expression = rule.Expression
                                });
                            }
                        }
                    }
                }
            }

            return errors;
        }

        private List<ValidationError> ValidateConditionalRules(ValidationRule rule, List<Dictionary<string, object>> data)
        {
            var errors = new List<ValidationError>();
            
            // Simplified conditional rule validation
            // Full implementation would require a proper expression parser
            
            return errors;
        }

        private List<string> MapColumnReference(string columnRef, IEnumerable<string> availableColumns)
        {
            var mapped = new List<string>();
            var availableList = availableColumns.ToList();

            if (columnRef == "c*")
            {
                // Wildcard - map to all c-columns
                mapped.AddRange(availableList.Where(col => col.StartsWith("c") && col.Length > 1 && char.IsDigit(col[1])));
            }
            else if (columnRef.Contains("-"))
            {
                // Range - expand range
                var match = Regex.Match(columnRef, @"c(\d{4})-(\d{4})");
                if (match.Success)
                {
                    var start = int.Parse(match.Groups[1].Value);
                    var end = int.Parse(match.Groups[2].Value);
                    
                    for (int i = start; i <= end; i += 10) // Increment by 10 for ECB columns
                    {
                        var colName = $"c{i:0000}";
                        if (availableList.Contains(colName))
                        {
                            mapped.Add(colName);
                        }
                    }
                }
            }
            else
            {
                // Direct mapping
                if (availableList.Contains(columnRef))
                {
                    mapped.Add(columnRef);
                }
            }

            return mapped;
        }

        #endregion

        #region Report Generation

        private string GenerateValidationReport(ValidationResults results, ITracingService tracingService)
        {
            var report = new StringBuilder();
            
            report.AppendLine("# ECB Excel Validation Report");
            report.AppendLine($"**Generated:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"**Status:** {results.OverallStatus}");
            report.AppendLine($"**Total Errors:** {results.TotalErrors}");
            report.AppendLine($"**Sheets Processed:** {results.ProcessedSheets.Count}");
            report.AppendLine();

            foreach (var (sheetName, sheetResult) in results.SheetResults)
            {
                report.AppendLine($"## Sheet: {sheetName}");
                report.AppendLine($"- **Status:** {sheetResult.Status}");
                report.AppendLine($"- **Data Rows:** {sheetResult.DataRows}");
                report.AppendLine($"- **Errors:** {sheetResult.ErrorCount}");
                
                if (sheetResult.Errors.Any())
                {
                    report.AppendLine("### Validation Errors:");
                    foreach (var error in sheetResult.Errors.Take(20)) // Limit to first 20 errors per sheet
                    {
                        report.AppendLine($"- **Row {error.RowIndex}, Column {error.Column}:** {error.Message} (Rule: {error.RuleId})");
                    }
                    
                    if (sheetResult.Errors.Count > 20)
                    {
                        report.AppendLine($"- ... and {sheetResult.Errors.Count - 20} more errors");
                    }
                }
                
                report.AppendLine();
            }

            tracingService.Trace($"Generated validation report: {report.Length} characters");
            return report.ToString();
        }

        #endregion
    }
}
