# Feature Specification: Reporting Infrastructure with FastReport

**Feature Branch**: `010-fastreport-reporting`
**Created**: 2025-10-22
**Status**: Draft
**Input**: User description: "Adopt FastReport.OpenSource as a reporting plugin with NFun-Report's attribute-driven source generation pattern. Create LablabBean.Reporting.Abstractions with report provider attributes, implement Roslyn source generator for compile-time provider registry, build FastReport plugin for PDF/HTML/image exports, create report templates for build metrics and game statistics, and integrate with CLI for multi-format report generation."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build Engineer Reviews Build Metrics (Priority: P1)

As a build engineer, I need to view comprehensive build reports showing test results, code coverage, and build duration so that I can identify quality issues and performance bottlenecks in the CI/CD pipeline.

**Why this priority**: Build metrics are the foundation for quality assurance and provide immediate ROI. They integrate with existing CI/CD workflows and don't require game runtime, making them the safest and most valuable starting point.

**Independent Test**: Can be fully tested by running a build with the Nuke build system, generating a report in HTML format, opening it in a browser, and verifying that test pass/fail counts, coverage percentages, and build duration are accurately displayed.

**Acceptance Scenarios**:

1. **Given** a completed build with test results, **When** the build engineer runs the report generation command, **Then** an HTML report is generated showing total tests, pass count, fail count, skip count, and overall pass percentage
2. **Given** a build with code coverage data, **When** the report is generated, **Then** the report displays line coverage percentage, branch coverage percentage, and identifies files with coverage below 80%
3. **Given** a build that took 5 minutes and 32 seconds, **When** the report is generated, **Then** the report shows the build duration in minutes and seconds along with timestamps for build start and completion
4. **Given** a report generation request for PDF format, **When** the build engineer specifies `--format pdf`, **Then** a PDF file is created with the same content as the HTML report

---

### User Story 2 - Player Views Game Session Statistics (Priority: P2)

As a player, I want to view detailed statistics about my game session (playtime, kills, deaths, damage dealt) after finishing a play session so that I can track my performance and improvement over time.

**Why this priority**: Game statistics provide player-facing value and enhance engagement. This is the primary user-visible feature and validates the reporting system's ability to handle runtime data collection.

**Independent Test**: Can be fully tested by playing a game session, performing various actions (combat, movement, item collection), ending the session, running the session report command, and verifying that all statistics match the actual gameplay events.

**Acceptance Scenarios**:

1. **Given** a completed game session lasting 45 minutes, **When** the player requests a session report, **Then** the report shows total playtime of 45 minutes, session start time, and session end time
2. **Given** a session where the player killed 12 enemies and died 3 times, **When** the session report is generated, **Then** the report displays 12 kills, 3 deaths, and a kill/death ratio of 4.0
3. **Given** a session where the player dealt 1,250 damage and took 430 damage, **When** the report is generated, **Then** the report shows total damage dealt (1,250), total damage taken (430), and average damage per kill (104)
4. **Given** a player requesting a report in CSV format, **When** they specify `--format csv`, **Then** a CSV file is created that can be opened in Excel or imported into analytics tools

---

### User Story 3 - Developer Generates Plugin System Health Report (Priority: P3)

As a developer, I need to generate reports showing which plugins are loaded, their health status, memory usage, and load times so that I can diagnose plugin system issues and optimize performance.

**Why this priority**: Plugin health reports support internal development and debugging. They leverage existing plugin metrics infrastructure but are primarily for developers, not end users.

**Independent Test**: Can be fully tested by starting the application with multiple plugins loaded, waiting for plugins to initialize, running the plugin status report command, and verifying that each plugin's state, memory usage, and load duration are correctly reported.

**Acceptance Scenarios**:

1. **Given** 5 plugins successfully loaded and 1 plugin failed to load, **When** the developer generates a plugin status report, **Then** the report lists all 6 plugins with their states (5 "Running", 1 "Failed"), shows the failure reason for the failed plugin, and displays overall success rate (83%)
2. **Given** plugins with varying memory usage, **When** the report is generated, **Then** each plugin shows memory usage in MB and the report highlights plugins using more than 50 MB
3. **Given** plugins with load times ranging from 100ms to 2.5 seconds, **When** the report is generated, **Then** each plugin shows load duration and the report sorts plugins by load time to identify slow loaders
4. **Given** a plugin in "Degraded" health state, **When** the health report is generated, **Then** the report displays the health status reason and timestamp of when the degraded state was detected

---

### User Story 4 - CI/CD System Exports Reports as Build Artifacts (Priority: P2)

As a CI/CD system, I need to generate reports automatically at the end of each build and save them as build artifacts so that developers can review historical reports and track quality trends over time.

**Why this priority**: Automation enables continuous monitoring and historical analysis. This is essential for production use but depends on the foundation from P1.

**Independent Test**: Can be fully tested by configuring the build script to run report generation, executing a build, checking the artifacts directory for report files, and verifying that reports are created with timestamped filenames and correct content.

**Acceptance Scenarios**:

1. **Given** a build completing successfully, **When** the CI/CD system runs the report generation step, **Then** reports are saved to `artifacts/reports/` with filenames including the build number and timestamp (e.g., `build-metrics-123-2025-10-22T14-30-00.html`)
2. **Given** a report generation failure due to missing test results, **When** the CI/CD system attempts to generate a report, **Then** the build step fails with a clear error message indicating which data files are missing
3. **Given** multiple report formats configured (HTML and PDF), **When** the build completes, **Then** both HTML and PDF versions are created and stored in the artifacts directory
4. **Given** a build with 500+ test results, **When** the report is generated, **Then** the report generation completes within 10 seconds and the resulting HTML file is under 5 MB

---

### Edge Cases

- **What happens when report generation is requested but no data is available?**
  - System displays a message indicating no data was found and suggests running the applicable action (build, test, or play session) first
  - Empty reports are not generated; the command exits with a non-zero exit code

- **How does the system handle corrupted or incomplete test result files?**
  - System logs a warning with the specific file that failed to parse
  - Report generation continues with available data
  - Report includes a notice indicating that some data sources were unavailable

- **What happens when a report template file is missing or invalid?**
  - System falls back to a default embedded template
  - Logs a warning about the missing custom template
  - Report is generated successfully with default formatting

- **How does the system handle extremely large data sets (10,000+ test results)?**
  - Report data is paginated or summarized (e.g., show summary statistics and detailed results for failures only)
  - System warns if report file size exceeds 10 MB
  - Offers option to export raw data to CSV for analysis in external tools

- **What happens when multiple report formats are requested simultaneously?**
  - System generates all requested formats in a single command execution
  - If one format fails, others still complete successfully
  - Summary message indicates which formats succeeded and which failed

- **How does the system handle concurrent report generation requests?**
  - Reports include unique identifiers (timestamp + random suffix) to prevent filename collisions
  - Each request operates independently without interfering with others
  - Temporary files are isolated per request

## Requirements *(mandatory)*

### Functional Requirements

#### Reporting Abstractions

- **FR-001**: System MUST provide marker attributes for identifying report providers at compile time
- **FR-002**: Report provider attributes MUST include an identifier, optional name, optional version, and optional category
- **FR-003**: System MUST support multiple report providers per category
- **FR-004**: Report provider metadata MUST be extractable during compilation without runtime reflection

#### Source Generator

- **FR-005**: System MUST generate a static registry of all report providers during compilation
- **FR-006**: Source generator MUST scan for classes and methods decorated with report provider attributes
- **FR-007**: Generated registry MUST provide lookup methods by provider ID and category
- **FR-008**: Source generator MUST produce incremental compilation output to optimize build performance
- **FR-009**: Source generator MUST emit diagnostic warnings if duplicate provider IDs are detected

#### FastReport Plugin

- **FR-010**: System MUST load FastReport as an optional plugin that can be enabled or disabled
- **FR-011**: FastReport plugin MUST register a reporting service in the plugin registry
- **FR-012**: Reporting service MUST support generating reports from templates and data objects
- **FR-013**: System MUST support exporting reports to HTML, PDF, and PNG image formats
- **FR-014**: FastReport plugin MUST use FastReport.OpenSource (MIT license) as the base library
- **FR-015**: PDF export functionality MUST use the FastReport.OpenSource.Export.PdfSimple plugin

#### Report Templates

- **FR-016**: System MUST provide report templates for build metrics, test results, code coverage, and plugin health status
- **FR-017**: Templates MUST be stored as `.frx` files (FastReport XML format) in a templates directory
- **FR-018**: System MUST allow users to customize templates or provide their own
- **FR-019**: Templates MUST support data binding to strongly-typed data models

#### Build Metrics Provider

- **FR-020**: System MUST collect build duration (start time, end time, total duration)
- **FR-021**: System MUST parse test results from xUnit XML output files
- **FR-022**: System MUST parse code coverage data from coverlet JSON output files
- **FR-023**: Build metrics provider MUST aggregate total tests, pass count, fail count, skip count, and pass percentage
- **FR-024**: Code coverage provider MUST report line coverage percentage and branch coverage percentage
- **FR-025**: System MUST identify test failures with test name, error message, and stack trace

#### Game Statistics Provider

- **FR-026**: System MUST track game session duration (start time, end time, total playtime)
- **FR-027**: System MUST record combat statistics (kills, deaths, damage dealt, damage taken)
- **FR-028**: System MUST calculate derived metrics (kill/death ratio, average damage per kill)
- **FR-029**: Game statistics MUST be collected via the existing AnalyticsPlugin event bus integration
- **FR-030**: System MUST export session data to JSON files for later report generation

#### Plugin Health Provider

- **FR-031**: System MUST collect plugin status from the existing PluginHealthChecker
- **FR-032**: Plugin health reports MUST include plugin state (Running, Failed, Degraded, Stopped)
- **FR-033**: System MUST report plugin memory usage in megabytes
- **FR-034**: System MUST report plugin load duration in milliseconds
- **FR-035**: Failed plugins MUST include failure reasons and timestamps

#### CLI Integration

- **FR-036**: System MUST provide a `report` command with subcommands for each report type (build, session, plugin-status)
- **FR-037**: CLI MUST support `--format` option to specify output format (html, pdf, png, csv)
- **FR-038**: CLI MUST support `--output` option to specify output file path
- **FR-039**: Default output format MUST be HTML if not specified
- **FR-040**: System MUST display a success message with the output file path after report generation

#### Error Handling

- **FR-041**: System MUST provide clear error messages when report generation fails
- **FR-042**: System MUST validate that required data files exist before attempting report generation
- **FR-043**: System MUST log warnings for missing or corrupted data files but continue with available data
- **FR-044**: System MUST exit with non-zero exit codes when report generation fails

### Key Entities *(include if feature involves data)*

- **ReportProvider**: Represents a source of report data with an identifier, name, version, and category. Providers are discovered at compile time via attributes.

- **ReportTemplate**: Represents a FastReport template file (`.frx` format) that defines the visual layout and data bindings for a report.

- **BuildMetrics**: Contains aggregated build data including start time, end time, duration, test counts, coverage percentages, and failure details.

- **GameSessionStatistics**: Contains gameplay data for a single session including playtime, combat metrics (kills, deaths, damage), and derived statistics (K/D ratio).

- **PluginHealthSnapshot**: Contains health information for a single plugin including state, memory usage, load duration, and health status with timestamps.

- **ReportDefinition**: Specifies what report to generate, including the template to use, the data provider to query, output format, and output path.

- **ReportExportFormat**: Enumeration of supported export formats (HTML, PDF, PNG, CSV).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Build engineers can generate build metric reports in under 5 seconds for builds with up to 500 tests
- **SC-002**: Players can view session statistics immediately after ending a game session (report generation under 2 seconds)
- **SC-003**: Reports are generated successfully for 100% of successful builds without requiring manual intervention
- **SC-004**: Developers can identify the slowest-loading plugin within 30 seconds of starting the application
- **SC-005**: Report file sizes remain under 5 MB for typical datasets (500 tests, 10 plugins, 1-hour game session)
- **SC-006**: Users can switch between HTML and PDF output formats using a single command-line flag
- **SC-007**: 95% of report generation requests complete without errors when data files are present
- **SC-008**: Report generation with missing or corrupted data files degrades gracefully by generating partial reports with warnings rather than failing completely

## Assumptions

- FastReport.OpenSource version 2026.1.0 or later is used, which supports .NET 8 console applications
- Test results are available in xUnit XML format (standard output from `dotnet test`)
- Code coverage data is available in coverlet JSON format (standard output from coverlet.collector)
- Game session data is collected by the existing AnalyticsPlugin and stored in JSON files
- Plugin metrics are available from the existing PluginSystemMetrics and PluginHealthChecker infrastructure
- Users have basic familiarity with command-line tools for running report commands
- HTML reports can be viewed in any modern web browser (Chrome, Firefox, Edge)
- PDF export using FastReport.OpenSource.Export.PdfSimple provides sufficient quality for build reports (advanced PDF features like encryption are not required initially)

## Out of Scope

- **Real-time report dashboards**: This feature focuses on on-demand report generation, not live-updating dashboards
- **Report scheduling**: Automated periodic report generation (daily, weekly) is not included; users must trigger reports manually or via CI/CD scripts
- **Report email delivery**: Sending reports via email is not included; users must access generated files directly
- **Custom report designer UI**: Users must edit `.frx` template files manually or use the FastReport Designer Community Edition (external tool)
- **Advanced PDF features**: Encryption, digital signatures, and custom font embedding require FastReport.Core (commercial version) and are not included
- **Excel/Word export formats**: XLSX and DOCX exports require FastReport.Core and are not included in this phase
- **Interactive charts**: Reports use static tables and basic visualizations; interactive charts require additional libraries
- **Multi-language reports**: Report templates are in English only; localization is not included
- **Report version history**: System does not track changes to report content over time; users must manage report archives manually

## Dependencies

- **FastReport.OpenSource NuGet package** (version 2026.1.0+): Required for report generation and template processing
- **FastReport.OpenSource.Export.PdfSimple NuGet package** (version 2026.1.2+): Required for PDF export functionality
- **Existing AnalyticsPlugin**: Game statistics collection depends on the AnalyticsPlugin tracking events via the event bus
- **Existing PluginSystemMetrics and PluginHealthChecker**: Plugin health reports depend on metrics already being collected by the plugin system
- **Nuke build system**: Build metrics integration depends on the Nuke build scripts being able to invoke report generation commands
- **xUnit and coverlet**: Test and coverage reports depend on xUnit producing XML output and coverlet producing JSON coverage files

## Notes

- This specification deliberately avoids implementation details (e.g., specific C# classes, namespaces, project structure) to remain technology-agnostic at the specification level
- The phased approach (P1: Build metrics, P2: Game statistics, P3: Plugin health) allows incremental delivery and validation
- FastReport plugin architecture ensures that reporting functionality can be enabled, disabled, or replaced without affecting core application functionality
- Report templates (`.frx` files) are version-controlled alongside code to ensure reproducibility
- The specification focuses on file-based report generation rather than web-based dashboards to minimize complexity and dependencies in the initial implementation
