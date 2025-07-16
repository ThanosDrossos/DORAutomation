
# ECB Validation Tool - Usage Guide

## Overview
This tool extracts and applies ECB validation rules from the official Excel file.

## Files Generated:
- `final_ecb_validation_rules.json`: 48 extracted validation rules
- `comprehensive_validation_report.html`: Detailed validation report
- `validation_errors.csv`: Exportable error data
- `ECB_Rules_Translation.ipynb`: This notebook with full implementation

## Quick Usage:

### 1. Load Validation Rules
```python
import json
import pandas as pd

with open('final_ecb_validation_rules.json', 'r') as f:
    rules = json.load(f)

engine = ValidationEngine(rules)
```

### 2. Validate Your Data
```python
# Load your data
your_data = pd.read_excel('your_regulatory_data.xlsx')

# Run validation
errors = engine.validate_data(your_data, table_name='tB_01.02')

# Generate report
if errors:
    error_df = engine.generate_error_report()
    save_validation_report(errors, 'your_validation_report.html')
```

### 3. Rule Statistics
- Total Rules: 48
- Table References: 9
- Column References: 16
- Function Types: 4

### 4. Common Validation Patterns
The rules check for:
- Mandatory field presence (not null conditions)
- Conditional requirements (if-then logic)
- Cross-field dependencies
- Data consistency across related fields

## Table Coverage:
{'tB_04.01', 'tB_02.02', 'tB_07.01', 'tB_05.01', 'tB_02.01', 'tB_01.02'}

## Next Steps:
1. Load your regulatory reporting data
2. Map your column names to ECB column references (c0010, c0020, etc.)
3. Run validation using the ValidationEngine
4. Review and export error reports
5. Fix data issues and re-validate
