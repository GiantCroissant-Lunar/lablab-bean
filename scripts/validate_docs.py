#!/usr/bin/env python3
"""
Documentation Validation Script

Validates all markdown files in docs/ for:
- Required front-matter fields
- Valid doc_type and status values
- Canonical uniqueness
"""

import pathlib
import sys
import re
from typing import Dict, List, Optional, Tuple

try:
    import yaml
except ImportError:
    print("ERROR: PyYAML not installed. Run: pip install pyyaml")
    sys.exit(1)

ROOT = pathlib.Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
INBOX = DOCS / "_inbox"
ARCHIVE = DOCS / "archive"

# Paths to exclude from validation
EXCLUDE_PATTERNS = [
    "/archive/",
    "/index/",
    "/_inbox/",
    "/node_modules/",
    "/.git/",
]

# Required front-matter fields
REQUIRED_FIELDS = ["doc_id", "title", "doc_type", "status", "canonical", "created", "tags", "summary"]

# Valid values
VALID_DOC_TYPES = ["spec", "rfc", "adr", "plan", "finding", "guide", "glossary", "reference"]
VALID_STATUSES = ["draft", "active", "superseded", "rejected", "archived"]


class ValidationError:
    def __init__(self, path: pathlib.Path, message: str, severity: str = "error"):
        self.path = path
        self.message = message
        self.severity = severity

    def __str__(self):
        rel_path = self.path.relative_to(ROOT)
        return f"[{self.severity.upper()}] {rel_path}: {self.message}"


def extract_frontmatter(text: str) -> Tuple[Optional[Dict], str]:
    """Extract YAML front-matter and body from markdown."""
    match = re.match(r"^---\r?\n(.*?)\r?\n---\r?\n", text, re.S)
    if not match:
        return None, text

    try:
        meta = yaml.safe_load(match.group(1))
        body = text[match.end():]
        return meta, body
    except yaml.YAMLError as e:
        return None, text


def should_exclude(path: pathlib.Path) -> bool:
    """Check if path should be excluded from validation."""
    path_str = str(path).replace("\\", "/")
    return any(pattern in path_str for pattern in EXCLUDE_PATTERNS)


def validate_doc(path: pathlib.Path) -> List[ValidationError]:
    """Validate a single markdown document."""
    errors = []
    
    try:
        text = path.read_text(encoding="utf-8")
    except Exception as e:
        errors.append(ValidationError(path, f"Cannot read file: {e}"))
        return errors

    meta, body = extract_frontmatter(text)
    
    if meta is None:
        errors.append(ValidationError(path, "Missing or invalid YAML front-matter"))
        return errors

    # Check required fields
    for field in REQUIRED_FIELDS:
        if field not in meta:
            errors.append(ValidationError(path, f"Missing required field: {field}"))

    # Validate doc_type
    if "doc_type" in meta and meta["doc_type"] not in VALID_DOC_TYPES:
        errors.append(ValidationError(
            path, 
            f"Invalid doc_type: {meta['doc_type']}. Must be one of: {', '.join(VALID_DOC_TYPES)}"
        ))

    # Validate status
    if "status" in meta and meta["status"] not in VALID_STATUSES:
        errors.append(ValidationError(
            path, 
            f"Invalid status: {meta['status']}. Must be one of: {', '.join(VALID_STATUSES)}"
        ))

    # Validate canonical is boolean
    if "canonical" in meta and not isinstance(meta["canonical"], bool):
        errors.append(ValidationError(path, f"canonical must be boolean, got: {type(meta['canonical']).__name__}"))

    return errors


def main():
    """Main validation entry point."""
    print(f"Validating documentation in {DOCS}...")
    
    all_errors = []
    canonical_docs = {}
    
    # Find all markdown files
    md_files = list(DOCS.glob("**/*.md"))
    
    for md_file in md_files:
        if should_exclude(md_file):
            continue
        
        errors = validate_doc(md_file)
        all_errors.extend(errors)
        
        # Track canonical documents
        try:
            text = md_file.read_text(encoding="utf-8")
            meta, _ = extract_frontmatter(text)
            if meta and meta.get("canonical") is True:
                title = meta.get("title", "Unknown")
                if title in canonical_docs:
                    all_errors.append(ValidationError(
                        md_file,
                        f"Duplicate canonical document for '{title}'. Other at: {canonical_docs[title]}"
                    ))
                else:
                    canonical_docs[title] = md_file.relative_to(ROOT)
        except:
            pass

    # Print results
    if all_errors:
        print(f"\n❌ Found {len(all_errors)} validation error(s):\n")
        for error in all_errors:
            print(f"  {error}")
        print()
        sys.exit(1)
    else:
        print(f"✅ All {len(md_files) - len([f for f in md_files if should_exclude(f)])} documents validated successfully!")
        sys.exit(0)


if __name__ == "__main__":
    main()
