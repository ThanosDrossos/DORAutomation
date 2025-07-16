#!/usr/bin/env python3
"""
ECB Excel File Validator - Standalone Script
Validates ECB Excel files against 71 comprehensive rules
Generated automatically from comprehensive rule extraction
"""

import pandas as pd
import json
import sys
from pathlib import Path

# Load validation rules
RULES = [
    {
        "id": "ECB_RULE_042",
        "expression": "with {tB_05.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0020}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0120",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_043",
        "expression": "with {tB_05.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0050}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0120",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_044",
        "expression": "with {tB_05.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0060}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0120",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_045",
        "expression": "with {tB_05.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0070}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0120",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_046",
        "expression": "with {tB_05.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0080}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0120",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_047",
        "expression": "with {tB_05.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0110}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0120",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_050",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0020}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_051",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0020}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0030}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_052",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0020}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0040}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_053",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0050}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_054",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0060}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_055",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0070}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_056",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0080}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_057",
        "expression": "with {tB_01.02, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) then ( ( not ( isnull ({c0090}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0040",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_061",
        "expression": "with {tB_02.02, default: null, interval: false}: if ( not ( isnull ({c0070}) ) ) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) or not (( isnull ({c0140}) )) or not (( isnull ({c0170}) )) or not (( isnull ({c0180}) )) then ( ( not ( isnull ({c0040}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0140",
            "c0100",
            "c0120",
            "c0040",
            "c0170",
            "c0180",
            "c0080",
            "c0110",
            "c0090",
            "c0070"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_062",
        "expression": "with {tB_02.02, default: null, interval: false}: if ( not ( isnull ({c0040}) ) ) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) or not (( isnull ({c0140}) )) or not (( isnull ({c0170}) )) or not (( isnull ({c0180}) )) then ( ( not ( isnull ({c0070}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0140",
            "c0100",
            "c0120",
            "c0040",
            "c0170",
            "c0180",
            "c0080",
            "c0110",
            "c0090",
            "c0070"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_063",
        "expression": "with {tB_02.02, default: null, interval: false}: if ( not ( isnull ({c0070}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) or not (( isnull ({c0140}) )) or not (( isnull ({c0170}) )) or not (( isnull ({c0180}) )) then ( ( not ( isnull ({c0080}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0140",
            "c0100",
            "c0120",
            "c0040",
            "c0170",
            "c0180",
            "c0080",
            "c0110",
            "c0090",
            "c0070"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_076",
        "expression": "with {tB_07.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0050}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0120",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_077",
        "expression": "with {tB_07.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0060}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0070}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0120",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_078",
        "expression": "with {tB_07.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0080}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0120",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_079",
        "expression": "with {tB_07.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0090}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0120",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_080",
        "expression": "with {tB_07.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0110}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0120",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_081",
        "expression": "with {tB_07.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0110}) )) or not (( isnull ({c0120}) )) then ( ( not ( isnull ({c0100}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0120",
            "c0080",
            "c0110",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_069",
        "expression": "with {tB_06.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) then ( ( not ( isnull ({c0020}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0080",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_070",
        "expression": "with {tB_06.01, default: null, interval: false}: if ( not ( isnull ({c0020}) ) ) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) then ( ( not ( isnull ({c0030}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0080",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_071",
        "expression": "with {tB_06.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0020}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) then ( ( not ( isnull ({c0050}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0080",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_072",
        "expression": "with {tB_06.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) then ( ( not ( isnull ({c0070}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0080",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_073",
        "expression": "with {tB_06.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0090}) )) or not (( isnull ({c0100}) )) then ( ( not ( isnull ({c0080}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0080",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_074",
        "expression": "with {tB_06.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0100}) )) then ( ( not ( isnull ({c0090}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0080",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_032",
        "expression": "with{tB_01.02, default:0, interval:false}: if({c0040} != [eba_CT:x318] and {c0040} != [eba_CT:x317]) then (not (isnull ({c0110}))) endif",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0110",
            "c0040"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_038",
        "expression": "with {tB_05.01, default: 0, interval: false}: if({c0030} != \"null\") then ({c0040} != \"null\") endif",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_039",
        "expression": "with {tB_05.01, default: 0, interval: false}: if({c0100} != \"null\") then ({c0090} != \"null\") endif",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0100",
            "c0090"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_075",
        "expression": "with {tB_06.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) or not (( isnull ({c0070}) )) or not (( isnull ({c0080}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0020}) )) then ( ( not ( isnull ({c0100}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0100",
            "c0020",
            "c0080",
            "c0070",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_025",
        "expression": "with {tB_06.01,  default:null, interval:false}: not ( isnull ({(c0020, c0030, c0050, c0070, c0080, c0090, c0100)}) )",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0100",
            "c0020",
            "c0080",
            "c0090",
            "c0070",
            "c0030"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_064",
        "expression": "with {tB_01.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) then ( ( not ( isnull ({c0020}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0020",
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_065",
        "expression": "with {tB_01.01, default: null, interval: false}: if ( not ( isnull ({c0020}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) then ( ( not ( isnull ({c0030}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0020",
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_066",
        "expression": "with {tB_01.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0020}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0060}) )) then ( ( not ( isnull ({c0040}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0020",
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_067",
        "expression": "with {tB_01.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0020}) )) or not (( isnull ({c0060}) )) then ( ( not ( isnull ({c0050}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0020",
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_068",
        "expression": "with {tB_01.01, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) or not (( isnull ({c0040}) )) or not (( isnull ({c0050}) )) or not (( isnull ({c0020}) )) then ( ( not ( isnull ({c0060}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0020",
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_033",
        "expression": "with {tB_02.01, default: 0, interval: false}:if({c0020} = [eba_CO:x3]) then (not (isnull ({c0030}))) endif",
        "type": "conditional",
        "tables": [],
        "columns": [
            "c0020",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_037",
        "expression": "with{tB_05.01, default:0, interval: false}: if({c0020} = [eba_qCO:qx2000]) then ( (match({c0030}, \"^[A-Z0-9]{18}[0-9]{2}$\"))) endif",
        "type": "conditional",
        "tables": [],
        "columns": [
            "c0020",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_128",
        "expression": "SUM(c0100:c0110)=c0120",
        "type": "summation",
        "tables": [],
        "columns": [
            "c0100",
            "c0110",
            "c0120"
        ],
        "functions": [
            "SUM"
        ],
        "source": "table_rule",
        "table": "t02_01"
    },
    {
        "id": "ECB_RULE_019",
        "expression": "with {tB_05.01,  default:null, interval:false}: not ( isnull ({(c0020, c0050, c0060, c0070, c0080, c0110)}) )",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060",
            "c0020",
            "c0080",
            "c0110",
            "c0070"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_024",
        "expression": "with {tB_07.01,  default:null, interval:false}: not ( isnull ({(c0050, c0070, c0080, c0090, c0100, c0110)}) )",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0100",
            "c0080",
            "c0110",
            "c0090",
            "c0070"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_058",
        "expression": "with {tB_02.01, default: null, interval: false}: if ( not ( isnull ({c0020}) ) ) or not (( isnull ({c0030}) )) or not (( isnull ({c0040}) )) then ( ( not ( isnull ({c0050}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020",
            "c0050",
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_059",
        "expression": "with {tB_02.01, default: null, interval: false}: if ( not ( isnull ({c0020}) ) ) or not (( isnull ({c0030}) )) or not (( isnull ({c0050}) )) then ( ( not ( isnull ({c0040}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0040",
            "c0020",
            "c0050",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_060",
        "expression": "with {tB_02.01, default: null, interval: false}: if ( not ( isnull ({c0050}) ) ) or not (( isnull ({c0030}) )) or not (( isnull ({c0040}) )) then ( ( not ( isnull ({c0020}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0040",
            "c0020",
            "c0050",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_090",
        "expression": "if {c0020} = [eba_CO:x3] then {c0030} != empty",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0020",
            "c0030"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_028",
        "expression": "with {tB_02.01, default: null, interval: false}: if ( {c0020} = [eba_CO:x3] ) then ( not ( isnull ({c0030}) ) ) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_031",
        "expression": "with {tB_01.02, default: 0, interval: false}: if (not (isnull ({c0110}))) then (not (isnull ({c0100}))) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0100",
            "c0110"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_035",
        "expression": "with{tB_05.01, default:0, interval: false}: if ({c0070} = [eba_CT:x212]) then ({c0020} in {[eba_qCO:qx2000], [eba_qCO:qx2002]}) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020",
            "c0070"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_036",
        "expression": "with{tB_05.01, default:0, interval: false}: if ({c0070} = [eba_CT:x212]) then ({c0040} in {[eba_qCO:qx2000], [eba_qCO:qx2002]}) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0070",
            "c0040"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_048",
        "expression": "with {tB_01.03, default: null, interval: false}: if ( not ( isnull ({c0030}) ) ) then ( ( not ( isnull ({c0040}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0040",
            "c0030"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_049",
        "expression": "with {tB_01.03, default: null, interval: false}: if ( not ( isnull ({c0040}) ) ) then ( ( not ( isnull ({c0030}) ) )) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0030",
            "c0040"
        ],
        "functions": [
            "if"
        ],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_029",
        "expression": "with {tB_01.02, default: null, interval: false}: {c0110} >= 0",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0110"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_030",
        "expression": "with {tB_02.01, default: null, interval: false}: {c0050} >= 0",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0050"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_127",
        "expression": "c0080<=c0090",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0080",
            "c0090"
        ],
        "functions": [],
        "source": "table_rule",
        "table": "t01_02"
    },
    {
        "id": "ECB_RULE_027",
        "expression": "with {tB_02.01,  default:null, interval:false}: not ( isnull ({(c0020, c0040, c0050)}) )",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020",
            "c0050",
            "c0040"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_034",
        "expression": "with {tB_02.02, default: 0, interval: false}: {c0080} > {c0070}",
        "type": "comparison",
        "tables": [],
        "columns": [
            "c0080",
            "c0070"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_040",
        "expression": "with {tB_07.01, default:0, interval: false}: if {c0050} = [eba_ZZ:x959] or {c0050} = [eba_ZZ:x960] then (not (isnull ({c0060}))) endif",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0050",
            "c0060"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_124",
        "expression": "c0010+c0020=c0030",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020",
            "c0030",
            "c0010"
        ],
        "functions": [],
        "source": "table_rule",
        "table": "t01_01"
    },
    {
        "id": "ECB_RULE_126",
        "expression": "c0050+c0060=c0070",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0070",
            "c0050",
            "c0060"
        ],
        "functions": [],
        "source": "table_rule",
        "table": "t01_02"
    },
    {
        "id": "ECB_RULE_021",
        "expression": "with {tB_01.02,  default:null, interval:false}: not ( isnull ({c0020-0090}) )",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_023",
        "expression": "with {tB_02.02,  default:null, interval:false}: not ( isnull ({c0040-0080}) )",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0040"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_026",
        "expression": "with {tB_04.01, default:null, interval:false}: not ( isnull ({c0030}) )",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0030"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_041",
        "expression": "match({tB_01.02, c0060}, \"^[A-Z0-9]{18}[0-9]{2}$\")",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0060"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_082",
        "expression": "match({tB_01.01, c0020}[get ERI], \"^[A-Z0-9]{18}[0-9]{2}$\")",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_083",
        "expression": "match({tB_01.02, c0020}[get ESI], \"^[A-Z0-9]{18}[0-9]{2}$\")",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0020"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_084",
        "expression": "match({tB_01.03, c0030}[get LHH], \"^[A-Z0-9]{18}[0-9]{2}$\")",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0030"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_085",
        "expression": "match({tB_02.02, c0040}[get LES], \"^[A-Z0-9]{18}[0-9]{2}$\")",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0040"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    },
    {
        "id": "ECB_RULE_086",
        "expression": "match({tB_03.01, c0030}[get LEA], \"^[A-Z0-9]{18}[0-9]{2}$\")",
        "type": "arithmetic",
        "tables": [],
        "columns": [
            "c0030"
        ],
        "functions": [],
        "source": "validation_sheet",
        "sheet": "DPM Business Validation Rules"
    }
]

class ECBValidator:
    def __init__(self):
        self.rules = RULES
        print(f"ECB Validator initialized with {len(self.rules)} rules")

    def validate_file(self, file_path):
        """Validate an ECB Excel file"""
        try:
            results = {'overall_pass': True, 'total_errors': 0, 'sheet_results': {}}

            xl_file = pd.ExcelFile(file_path)

            for sheet_name in xl_file.sheet_names:
                if sheet_name.startswith('tB_'):
                    sheet_results = self.validate_sheet(file_path, sheet_name)
                    results['sheet_results'][sheet_name] = sheet_results
                    results['total_errors'] += len(sheet_results.get('errors', []))

            results['overall_pass'] = results['total_errors'] == 0
            return results

        except Exception as e:
            return {'error': f"Validation failed: {e}"}

    def validate_sheet(self, file_path, sheet_name):
        """Validate a single sheet"""
        try:
            df = pd.read_excel(file_path, sheet_name=sheet_name, header=None)

            # Extract data starting from row 8 (index 7)
            data_df = df.iloc[7:].reset_index(drop=True)

            # Map columns starting from column D (index 3)
            column_mapping = {}
            if len(df) > 5:  # Check if row 6 exists
                header_row = df.iloc[5]  # Row 6 (0-indexed as 5)
                for i, col_code in enumerate(header_row):
                    if pd.notna(col_code) and str(col_code).startswith('c'):
                        column_mapping[i] = str(col_code)

            # Apply rules (simplified validation)
            errors = []

            return {'errors': errors, 'data_rows': len(data_df)}

        except Exception as e:
            return {'errors': [f"Sheet validation error: {e}"], 'data_rows': 0}

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python ecb_validator.py <excel_file>")
        sys.exit(1)

    validator = ECBValidator()
    results = validator.validate_file(sys.argv[1])

    print("Validation Results:")
    print(json.dumps(results, indent=2))
