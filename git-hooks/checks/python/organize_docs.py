#!/usr/bin/env python3
# type: ignore
"""
Documentation Organization Script

Organizes root-level markdown files according to the R-DOC-001 rule: new docs
should go to docs/_inbox/ first, then be properly categorized.

SCOPE: This script ONLY processes:
- Markdown files directly at the repository root level (*.md)
- Files in docs/ directory that may need reorganization
- NEVER touches .agent/, .kiro/, or other system directories

PROTECTION: Agent system files (AGENTS.md, CLAUDE.md, KIRO.md) are explicitly
excluded and will never be moved.

This script:
1. Finds root-level .md files and docs/ files needing organization
2. Analyzes their content and front-matter
3. Suggests or automatically moves them to appropriate docs/ subdirectories
4. Updates internal cross-references
5. Validates organization follows the documentation schema

Usage:
    python git-hooks/checks/python/organize_docs.py [--dry-run] [--auto-move]
"""

import argparse
import os
import pathlib
import re
import shutil
import sys
from datetime import datetime, timezone
from typing import Any, Dict, List, Optional, Tuple

try:
    import yaml
except ImportError:
    print("ERROR: PyYAML not installed. Run: pip install pyyaml")
    sys.exit(1)

ROOT = (
    pathlib.Path(__file__).resolve().parents[3]
)  # Go up from git-hooks/checks/python/
DOCS_DIR = ROOT / "docs"
INBOX_DIR = DOCS_DIR / "_inbox"


def is_relative_to(path: pathlib.Path, parent: pathlib.Path) -> bool:
    """Check if path is relative to parent (Python 3.9+ compatibility)."""
    try:
        path.relative_to(parent)
        return True
    except ValueError:
        return False


# Directories to exclude from scanning
EXCLUDE_DIRS = {
    ".git",
    ".agent",  # Agent system files - NEVER move these
    ".kiro",  # Kiro steering files - managed separately
    "node_modules",
    "build",
    "_artifacts",
    "bin",
    "obj",
    "packages",
    "dist",
    "coverage",
    "test-results",
    "logs",
    "ref-projects",  # Exclude reference projects
    ".vs",
    ".vscode",
    ".idea",  # IDE directories
    "TestResults",
    "BenchmarkDotNet.Artifacts",  # Test artifacts
}

# Files to exclude from organization
EXCLUDE_FILES = {
    "README.md",  # Keep root README
    "CHANGELOG.md",
    "CHANGES.md",
    "HISTORY.md",  # Version control files
    "LICENSE.md",
    "COPYING.md",  # Legal files
    "CONTRIBUTING.md",  # Keep in root for GitHub
    "AGENTS.md",
    "CLAUDE.md",  # Agent pointer files (managed by generate_pointers.py)
    "KIRO.md",  # Kiro pointer file
}

# Valid doc types from the schema
VALID_DOC_TYPES = {
    "spec",
    "rfc",
    "adr",
    "plan",
    "finding",
    "guide",
    "glossary",
    "reference",
}

# Directory mapping based on doc_type
DOC_TYPE_DIRS = {
    "spec": "specs",
    "rfc": "rfcs",
    "adr": "adrs",
    "plan": "plans",
    "finding": "findings",
    "guide": "guides",
    "glossary": "glossary",
    "reference": "guides",  # Reference docs go to guides
}


class DocumentFile:
    """Represents a markdown document with metadata."""

    def __init__(self, path: pathlib.Path):
        self.path = path
        self.relative_path = path.relative_to(ROOT)
        self.content = ""
        self.frontmatter: Optional[Dict[str, Any]] = None
        self.body = ""
        self.suggested_location: Optional[pathlib.Path] = None
        self.issues: List[str] = []

        self._load_content()
        self._analyze()

    def _load_content(self):
        """Load file content."""
        try:
            self.content = self.path.read_text(encoding="utf-8")
        except Exception as e:
            self.issues.append(f"Failed to read file: {e}")
            return

    def _analyze(self):
        """Analyze content and determine suggested location."""
        if not self.content:
            return

        # Extract frontmatter
        self.frontmatter, self.body = self._extract_frontmatter()

        # Determine suggested location
        self._suggest_location()

    def _extract_frontmatter(self) -> Tuple[Optional[Dict], str]:
        """Extract YAML front-matter from content."""
        match = re.match(r"^---\r?\n(.*?)\r?\n---\r?\n", self.content, re.S)
        if not match:
            return None, self.content

        try:
            meta = yaml.safe_load(match.group(1))
            body = self.content[match.end() :]
            return meta or {}, body
        except yaml.YAMLError as e:
            self.issues.append(f"YAML parse error: {e}")
            return {"_parse_error": str(e)}, self.content

    def _suggest_location(self):
        """Suggest where this document should be located."""
        # If already in docs/, check if it's in the right place
        if is_relative_to(self.path, DOCS_DIR):
            if self.frontmatter and "doc_type" in self.frontmatter:
                doc_type = self.frontmatter["doc_type"]
                if doc_type in DOC_TYPE_DIRS:
                    expected_dir = DOCS_DIR / DOC_TYPE_DIRS[doc_type]
                    if not is_relative_to(self.path, expected_dir):
                        self.suggested_location = expected_dir / self.path.name
                        self.issues.append(
                            f"Should be in {DOC_TYPE_DIRS[doc_type]}/ based on doc_type"
                        )
            return

        # If has proper frontmatter, suggest appropriate directory
        if self.frontmatter and "doc_type" in self.frontmatter:
            doc_type = self.frontmatter["doc_type"]
            if doc_type in DOC_TYPE_DIRS:
                self.suggested_location = (
                    DOCS_DIR / DOC_TYPE_DIRS[doc_type] / self.path.name
                )
            else:
                self.suggested_location = INBOX_DIR / self.path.name
                self.issues.append(f"Invalid doc_type '{doc_type}', moving to inbox")
        else:
            # No frontmatter or invalid - goes to inbox
            self.suggested_location = INBOX_DIR / self.path.name
            if not self.frontmatter:
                self.issues.append("Missing YAML front-matter, moving to inbox")
            else:
                self.issues.append("Missing doc_type in front-matter, moving to inbox")

    def should_be_moved(self) -> bool:
        """Check if this file should be moved."""
        if not self.suggested_location:
            return False
        return self.path != self.suggested_location

    def get_move_description(self) -> str:
        """Get human-readable description of the move."""
        if not self.should_be_moved():
            return "No move needed"

        from_rel = self.path.relative_to(ROOT)
        to_rel = self.suggested_location.relative_to(ROOT)
        return f"{from_rel} → {to_rel}"


class DocumentOrganizer:
    """Main class for organizing documentation."""

    def __init__(self, dry_run: bool = True, auto_move: bool = False):
        self.dry_run = dry_run
        self.auto_move = auto_move
        self.documents: List[DocumentFile] = []
        self.moves_performed: List[Tuple[pathlib.Path, pathlib.Path]] = []

    def scan_repository(self) -> List[DocumentFile]:
        """Scan repository for markdown files at root level only."""
        print("Scanning root level for markdown files...")

        found_files = []
        excluded_count = 0

        # Only scan root level files (*.md directly in ROOT)
        for md_file in ROOT.glob("*.md"):
            # Skip excluded files
            if md_file.name in EXCLUDE_FILES:
                excluded_count += 1
                continue

            # Skip agent system files (extra protection)
            if md_file.name in {"AGENTS.md", "CLAUDE.md", "KIRO.md"}:
                excluded_count += 1
                continue

            found_files.append(md_file)

        # Also check docs/ directory for files that might need reorganization
        if DOCS_DIR.exists():
            for md_file in DOCS_DIR.rglob("*.md"):
                # Skip files already in proper subdirectories unless they're misplaced
                relative_to_docs = md_file.relative_to(DOCS_DIR)

                # If it's directly in docs/ (not in a subdirectory), it might need organizing
                if len(relative_to_docs.parts) == 1:
                    found_files.append(md_file)
                # If it's in a subdirectory, check if it has proper frontmatter for that location
                else:
                    found_files.append(md_file)

        print(f"Found {len(found_files)} markdown files (excluded {excluded_count})")

        # Analyze each file
        self.documents = []
        for file_path in found_files:
            doc = DocumentFile(file_path)
            self.documents.append(doc)

        return self.documents

    def analyze_organization(self) -> Dict[str, Any]:
        """Analyze current organization and return summary."""
        summary: Dict[str, Any] = {
            "total_files": len(self.documents),
            "files_to_move": 0,
            "files_in_docs": 0,
            "files_with_frontmatter": 0,
            "files_with_issues": 0,
            "moves_by_type": {},
            "issues_summary": {},
        }

        for doc in self.documents:
            if is_relative_to(doc.path, DOCS_DIR):
                summary["files_in_docs"] += 1

            if doc.frontmatter:
                summary["files_with_frontmatter"] += 1

            if doc.issues:
                summary["files_with_issues"] += 1
                for issue in doc.issues:
                    issue_key = issue.split(",")[0]  # First part of issue
                    summary["issues_summary"][issue_key] = (
                        summary["issues_summary"].get(issue_key, 0) + 1
                    )

            if doc.should_be_moved():
                summary["files_to_move"] += 1

                # Categorize move type
                if doc.suggested_location.parent == INBOX_DIR:
                    move_type = "to_inbox"
                else:
                    move_type = f"to_{doc.suggested_location.parent.name}"

                summary["moves_by_type"][move_type] = (
                    summary["moves_by_type"].get(move_type, 0) + 1
                )

        return summary

    def print_analysis(self, summary: Dict[str, Any]):
        """Print analysis results."""
        print("\n" + "=" * 60)
        print("DOCUMENTATION ORGANIZATION ANALYSIS")
        print("=" * 60)

        print("\nOverall Statistics:")
        print(f"  Total markdown files found: {summary['total_files']}")
        print(f"  Files already in docs/: {summary['files_in_docs']}")
        print(f"  Files with front-matter: {summary['files_with_frontmatter']}")
        print(f"  Files with issues: {summary['files_with_issues']}")
        print(f"  Files needing to be moved: {summary['files_to_move']}")

        if summary["moves_by_type"]:
            print("\nMoves by destination:")
            for move_type, count in summary["moves_by_type"].items():
                print(f"  {move_type}: {count} files")

        if summary["issues_summary"]:
            print("\nCommon issues:")
            for issue, count in summary["issues_summary"].items():
                print(f"  {issue}: {count} files")

    def print_detailed_moves(self):
        """Print detailed list of proposed moves."""
        moves = [doc for doc in self.documents if doc.should_be_moved()]

        if not moves:
            print("\nNo files need to be moved!")
            return

        print(f"\nPROPOSED MOVES ({len(moves)} files):")
        print("-" * 60)

        for doc in moves:
            print(f"\n{doc.get_move_description()}")
            if doc.issues:
                for issue in doc.issues:
                    print(f"   WARNING: {issue}")

    def perform_moves(self) -> bool:
        """Perform the actual file moves."""
        moves = [doc for doc in self.documents if doc.should_be_moved()]

        if not moves:
            print("No moves to perform.")
            return True

        if self.dry_run:
            print(f"\nDRY RUN: Would move {len(moves)} files")
            return True

        print(f"\nMoving {len(moves)} files...")

        success_count = 0

        for doc in moves:
            try:
                # Ensure target directory exists
                doc.suggested_location.parent.mkdir(parents=True, exist_ok=True)

                # Move the file
                shutil.move(str(doc.path), str(doc.suggested_location))

                print(f"Moved: {doc.get_move_description()}")
                self.moves_performed.append((doc.path, doc.suggested_location))
                success_count += 1

            except Exception as e:
                print(f"Failed to move {doc.path}: {e}")

        print(f"\nSuccessfully moved {success_count}/{len(moves)} files")
        return success_count == len(moves)

    def update_cross_references(self):
        """Update cross-references in moved files (basic implementation)."""
        if not self.moves_performed or self.dry_run:
            return

        print("\nUpdating cross-references...")

        # This is a basic implementation - could be enhanced
        for old_path, new_path in self.moves_performed:
            try:
                content = new_path.read_text(encoding="utf-8")

                # Update relative links that might be broken
                # This is a simple regex - could be more sophisticated
                updated_content = content

                # Look for markdown links like [text](../file.md)
                def update_link(match, old_p=old_path, new_p=new_path):
                    link_path = match.group(1)
                    if link_path.startswith(("http://", "https://", "#")):
                        return match.group(0)  # Don't change external links or anchors

                    # Try to resolve the link relative to new location
                    try:
                        old_target = old_p.parent / link_path
                        if old_target.exists():
                            # Calculate new relative path
                            new_relative = pathlib.Path(
                                os.path.relpath(old_target, new_p.parent)
                            )
                            return f"]({new_relative})"
                    except Exception:
                        pass

                    return match.group(0)  # Return unchanged if we can't fix it

                updated_content = re.sub(r"\]\(([^)]+)\)", update_link, updated_content)

                if updated_content != content:
                    new_path.write_text(updated_content, encoding="utf-8")
                    print(f"  Updated links in {new_path.relative_to(ROOT)}")

            except Exception as e:
                print(f"  Failed to update links in {new_path}: {e}")

    def generate_organization_report(self):
        """Generate a report of the organization process."""
        if self.dry_run or not self.moves_performed:
            return

        report_path = DOCS_DIR / "index" / "organization-report.md"
        report_path.parent.mkdir(parents=True, exist_ok=True)

        report_content = f"""# Documentation Organization Report

*Generated: {datetime.now(timezone.utc).isoformat()}*

## Summary

This report documents the automatic organization of scattered markdown files
according to the R-DOC-001 rule.

### Files Moved ({len(self.moves_performed)})

"""

        for old_path, new_path in self.moves_performed:
            old_rel = old_path.relative_to(ROOT)
            new_rel = new_path.relative_to(ROOT)
            report_content += f"- `{old_rel}` → `{new_rel}`\n"

        report_content += """

## Organization Rules Applied

- **R-DOC-001**: New docs go to `docs/_inbox/` first
- Files with proper `doc_type` front-matter moved to appropriate subdirectories
- Files without front-matter moved to `docs/_inbox/`
- Cross-references updated where possible

## Next Steps

1. Review files in `docs/_inbox/` and add proper front-matter
2. Move files from inbox to appropriate categories
3. Run `python scripts/validate_docs.py` to validate organization
4. Update any remaining broken cross-references

---

*Generated by: `git-hooks/checks/python/organize_docs.py`*
"""

        report_path.write_text(report_content, encoding="utf-8")
        print(f"\nOrganization report saved to: {report_path.relative_to(ROOT)}")


def main():
    parser = argparse.ArgumentParser(
        description="Organize scattered documentation files according to R-DOC-001"
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        default=True,
        help="Show what would be done without making changes (default)",
    )
    parser.add_argument(
        "--auto-move",
        action="store_true",
        help="Actually perform the file moves (disables dry-run)",
    )
    parser.add_argument(
        "--no-cross-ref-update",
        action="store_true",
        help="Skip updating cross-references in moved files",
    )

    args = parser.parse_args()

    # If auto-move is specified, disable dry-run
    if args.auto_move:
        args.dry_run = False

    print("Documentation Organization Tool")
    print("=" * 50)

    if args.dry_run:
        print("DRY-RUN mode (no changes will be made)")
        print("   Use --auto-move to actually perform moves")
    else:
        print("MOVE mode (files will be moved)")

    # Initialize organizer
    organizer = DocumentOrganizer(dry_run=args.dry_run, auto_move=args.auto_move)

    # Scan repository
    organizer.scan_repository()

    # Analyze organization
    summary = organizer.analyze_organization()
    organizer.print_analysis(summary)

    # Show detailed moves
    organizer.print_detailed_moves()

    # Perform moves if requested
    if summary["files_to_move"] > 0:
        success = organizer.perform_moves()

        if success and not args.dry_run:
            # Update cross-references
            if not args.no_cross_ref_update:
                organizer.update_cross_references()

            # Generate report
            organizer.generate_organization_report()

            print("\nDocumentation organization completed!")
            print("   Next steps:")
            print("   1. Review files in docs/_inbox/")
            print("   2. Add proper front-matter to files missing it")
            print("   3. Run: python scripts/validate_docs.py")
        elif not success:
            print("\nSome moves failed. Check the errors above.")
            sys.exit(1)
    else:
        print("\nAll documentation is already properly organized!")

    # Exit with appropriate code
    if args.dry_run and summary["files_to_move"] > 0:
        print(f"\nRun with --auto-move to organize {summary['files_to_move']} files")
        sys.exit(1)  # Indicate action needed
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()
