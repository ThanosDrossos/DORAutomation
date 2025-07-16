using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ECBValidation.Tests
{
    /// <summary>
    /// Mock implementation of IPluginExecutionContext for testing
    /// </summary>
    public class MockPluginExecutionContext : IPluginExecutionContext
    {
        public MockPluginExecutionContext()
        {
            InputParameters = new ParameterCollection();
            OutputParameters = new ParameterCollection();
            SharedVariables = new ParameterCollection();
            PreEntityImages = new EntityImageCollection();
            PostEntityImages = new EntityImageCollection();
            
            // Set default values
            UserId = Guid.NewGuid();
            InitiatingUserId = UserId;
            OrganizationId = Guid.NewGuid();
            OrganizationName = "TestOrganization";
            MessageName = "dor_ECBValidation";
            Stage = 30; // Main Operation
            Mode = 0;   // Synchronous
            Depth = 1;
            CorrelationId = Guid.NewGuid();
            RequestId = Guid.NewGuid();
            OperationId = Guid.NewGuid();
            OperationCreatedOn = DateTime.UtcNow;
        }

        public int Stage { get; set; }
        public IPluginExecutionContext ParentContext { get; set; }
        public int Mode { get; set; }
        public int IsolationMode { get; set; }
        public int Depth { get; set; }
        public string MessageName { get; set; }
        public string PrimaryEntityName { get; set; }
        public Guid? RequestId { get; set; }
        public string SecondaryEntityName { get; set; }
        public ParameterCollection InputParameters { get; set; }
        public ParameterCollection OutputParameters { get; set; }
        public ParameterCollection SharedVariables { get; set; }
        public Guid UserId { get; set; }
        public Guid InitiatingUserId { get; set; }
        public Guid BusinessUnitId { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid PrimaryEntityId { get; set; }
        public EntityImageCollection PreEntityImages { get; set; }
        public EntityImageCollection PostEntityImages { get; set; }
        public Guid OwningExtension { get; set; }
        public Guid CorrelationId { get; set; }
        public bool IsExecutingOffline { get; set; }
        public bool IsOfflinePlayback { get; set; }
        public bool IsInTransaction { get; set; }
        public Guid OperationId { get; set; }
        public DateTime OperationCreatedOn { get; set; }
    }

    /// <summary>
    /// Mock implementation of IOrganizationService for testing
    /// </summary>
    public class MockOrganizationService : IOrganizationService
    {
        private readonly Dictionary<Guid, Entity> _entities = new Dictionary<Guid, Entity>();

        public Guid Create(Entity entity)
        {
            var id = Guid.NewGuid();
            entity.Id = id;
            _entities[id] = entity;
            return id;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            if (_entities.ContainsKey(id))
            {
                return _entities[id];
            }
            throw new InvalidOperationException($"Entity with id {id} not found");
        }

        public void Update(Entity entity)
        {
            _entities[entity.Id] = entity;
        }

        public void Delete(string entityName, Guid id)
        {
            _entities.Remove(id);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            // Mock implementation for Execute method
            return new OrganizationResponse();
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            // Mock implementation
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            // Mock implementation
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            // Return empty collection for tests
            return new EntityCollection();
        }
    }

    /// <summary>
    /// Mock implementation of ITracingService for testing
    /// </summary>
    public class MockTracingService : ITracingService
    {
        public List<string> Traces { get; } = new List<string>();

        public void Trace(string format, params object[] args)
        {
            var message = args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, format, args) : format;
            Traces.Add(message);
            
            // Also output to debug console for easier debugging
            System.Diagnostics.Debug.WriteLine($"[TRACE] {message}");
        }
    }

    /// <summary>
    /// Mock implementation of IOrganizationServiceFactory for testing
    /// </summary>
    public class MockOrganizationServiceFactory : IOrganizationServiceFactory
    {
        private readonly IOrganizationService _service;

        public MockOrganizationServiceFactory(IOrganizationService service)
        {
            _service = service;
        }

        public IOrganizationService CreateOrganizationService(Guid? userId)
        {
            return _service;
        }
    }

    /// <summary>
    /// Mock implementation of IServiceProvider for testing
    /// </summary>
    public class MockServiceProvider : IServiceProvider
    {
        private readonly IPluginExecutionContext _context;
        private readonly IOrganizationService _organizationService;
        private readonly ITracingService _tracingService;
        private readonly IOrganizationServiceFactory _serviceFactory;

        public MockServiceProvider(
            IPluginExecutionContext context, 
            IOrganizationService organizationService, 
            ITracingService tracingService)
        {
            _context = context;
            _organizationService = organizationService;
            _tracingService = tracingService;
            _serviceFactory = new MockOrganizationServiceFactory(_organizationService);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IPluginExecutionContext))
                return _context;
                
            if (serviceType == typeof(IOrganizationService))
                return _organizationService;
                
            if (serviceType == typeof(ITracingService))
                return _tracingService;
                
            if (serviceType == typeof(IOrganizationServiceFactory))
                return _serviceFactory;

            throw new InvalidOperationException($"Service of type {serviceType.Name} is not supported by MockServiceProvider");
        }
    }

    /// <summary>
    /// Test utilities for creating test data and files
    /// </summary>
    public static class ECBTestUtilities
    {
        /// <summary>
        /// Creates a sample ECB Excel file with valid structure and data
        /// </summary>
        public static byte[] CreateSampleECBFile(int dataRows = 10)
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                // Set license context
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                // Add column headers in row 6, starting from column D
                worksheet.Cells[6, 4].Value = "0020"; // c0020 - Entity ID
                worksheet.Cells[6, 5].Value = "0030"; // c0030 - Entity Type
                worksheet.Cells[6, 6].Value = "0040"; // c0040 - Amount
                worksheet.Cells[6, 7].Value = "0050"; // c0050 - Currency
                worksheet.Cells[6, 8].Value = "0060"; // c0060 - Date
                
                // Add sample data starting from row 8
                var random = new Random(42); // Fixed seed for reproducible tests
                for (int i = 0; i < dataRows; i++)
                {
                    var row = 8 + i;
                    worksheet.Cells[row, 4].Value = 1000 + i;                           // c0020
                    worksheet.Cells[row, 5].Value = $"ENT{(i % 10 + 1):D3}";           // c0030
                    worksheet.Cells[row, 6].Value = random.Next(100, 10000);            // c0040
                    worksheet.Cells[row, 7].Value = "EUR";                              // c0050
                    worksheet.Cells[row, 8].Value = DateTime.Today.AddDays(-i);        // c0060
                }
                
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Creates an ECB Excel file with validation errors
        /// </summary>
        public static byte[] CreateECBFileWithErrors()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var worksheet = package.Workbook.Worksheets.Add("tB_01.02");
                
                // Add column headers
                worksheet.Cells[6, 4].Value = "0020";
                worksheet.Cells[6, 5].Value = "0030";
                worksheet.Cells[6, 6].Value = "0040";
                
                // Row 8: Missing mandatory field (c0020 is null)
                worksheet.Cells[8, 4].Value = null;
                worksheet.Cells[8, 5].Value = "ENT001";
                worksheet.Cells[8, 6].Value = 100;
                
                // Row 9: Invalid entity format (should be ENTxxx)
                worksheet.Cells[9, 4].Value = 1001;
                worksheet.Cells[9, 5].Value = "INVALID";
                worksheet.Cells[9, 6].Value = 200;
                
                // Row 10: Negative amount (business rule violation)
                worksheet.Cells[10, 4].Value = 1002;
                worksheet.Cells[10, 5].Value = "ENT002";
                worksheet.Cells[10, 6].Value = -50;
                
                // Row 11: All mandatory fields missing
                worksheet.Cells[11, 4].Value = null;
                worksheet.Cells[11, 5].Value = null;
                worksheet.Cells[11, 6].Value = null;
                
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Creates a mock ECB rules file with common validation patterns
        /// </summary>
        public static byte[] CreateMockECBRulesFile()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var worksheet = package.Workbook.Worksheets.Add("ValidationRules");
                
                // Mock ECB validation expressions
                var rules = new[]
                {
                    // Mandatory field rules
                    "with {tB_01.02, c0020}: not(isnull({c0020}))",
                    "with {tB_01.02, c0030}: not(isnull({c0030}))",
                    
                    // Pattern matching rules
                    "with {tB_01.02, c0030}: match({c0030}, \"^ENT[0-9]{3}$\")",
                    "with {tB_01.02, c0050}: match({c0050}, \"^[A-Z]{3}$\")",
                    
                    // Value constraint rules
                    "with {tB_01.02, c0040}: {c0040} >= 0",
                    "with {tB_01.02, c0020}: {c0020} > 0",
                    
                    // Conditional rules
                    "with {tB_01.02, c0040, c0050}: if not(isnull({c0040})) then not(isnull({c0050})) endif",
                    "with {tB_01.02, c0020, c0030}: if {c0020} > 1000 then not(isnull({c0030})) endif",
                    
                    // Complex validation rules
                    "with {tB_01.02, c0060}: if not(isnull({c0060})) then {c0060} <= today() endif",
                    "with {tB_01.02, c*}: sum({c0040}) > 0"
                };
                
                // Add rules to worksheet
                for (int i = 0; i < rules.Length; i++)
                {
                    worksheet.Cells[i + 1, 1].Value = rules[i];
                }
                
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Creates a multi-sheet ECB file for testing table filtering
        /// </summary>
        public static byte[] CreateMultiSheetECBFile()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var sheets = new[] { "tB_01.02", "tB_01.03", "tB_02.01", "tB_03.01", "Summary" };
                
                foreach (var sheetName in sheets)
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);
                    
                    // Only add ECB structure to tB_ sheets
                    if (sheetName.StartsWith("tB_"))
                    {
                        // Add column headers
                        worksheet.Cells[6, 4].Value = "0020";
                        worksheet.Cells[6, 5].Value = "0030";
                        
                        // Add sample data
                        worksheet.Cells[8, 4].Value = 1000;
                        worksheet.Cells[8, 5].Value = "ENT001";
                        worksheet.Cells[9, 4].Value = 1001;
                        worksheet.Cells[9, 5].Value = "ENT002";
                    }
                }
                
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Validates that a test result has expected structure
        /// </summary>
        public static void ValidateTestResult(string resultJson, string expectedStatus)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(resultJson);
            
            // Verify required properties exist
            if (!result.TryGetProperty("Status", out var statusElement))
                throw new AssertFailedException("ValidationResult missing 'Status' property");
                
            if (!result.TryGetProperty("TotalErrors", out var errorsElement))
                throw new AssertFailedException("ValidationResult missing 'TotalErrors' property");
                
            if (!result.TryGetProperty("ProcessedSheets", out var sheetsElement))
                throw new AssertFailedException("ValidationResult missing 'ProcessedSheets' property");
            
            // Verify status matches expected
            var actualStatus = statusElement.GetString();
            if (actualStatus != expectedStatus)
                throw new AssertFailedException($"Expected status '{expectedStatus}', but got '{actualStatus}'");
        }
    }
}
