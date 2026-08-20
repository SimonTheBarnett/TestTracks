# Test Tracks Playwright Java

This is a Playwright Java starter framework for browser and API test automation using Maven, Cucumber-JVM, JUnit Platform and PicoContainer.

It uses BDD feature files, Playwright browser automation, Playwright API testing, environment-based test data, page objects, thin API clients, atomic cleanup, reports and evidence capture.

The example tests target `https://automationintesting.online`, which is included only to make the framework concrete.

## Requirements

- Oracle JDK 25 LTS.
- Apache Maven 3.9.16.
- Internet access to restore Maven packages and install Playwright browser binaries.
- Access to the public demo application if you want to run the example tests.

## Install Java on Windows

The simplest PowerShell route is Winget:

```powershell
winget install -e --id Oracle.JDK.25
```

Close and reopen PowerShell after the install, then check Java:

```powershell
java -version
```

You should see an Oracle `25.x` JDK. If `java` is still not found, restart VS Code or Windows Terminal so the updated `PATH` is picked up.

## Install Maven on Windows

Install Apache Maven once from the official Apache binary ZIP:

```powershell
$mavenVersion = "3.9.16"
$mavenHome = "$env:USERPROFILE\Tools\apache-maven-$mavenVersion"
$zipPath = "$env:TEMP\apache-maven-$mavenVersion-bin.zip"

New-Item -ItemType Directory -Force "$env:USERPROFILE\Tools" | Out-Null
Invoke-WebRequest "https://dlcdn.apache.org/maven/maven-3/$mavenVersion/binaries/apache-maven-$mavenVersion-bin.zip" -OutFile $zipPath
Expand-Archive -LiteralPath $zipPath -DestinationPath "$env:USERPROFILE\Tools" -Force

[Environment]::SetEnvironmentVariable("MAVEN_HOME", $mavenHome, "User")
[Environment]::SetEnvironmentVariable("Path", [Environment]::GetEnvironmentVariable("Path", "User") + ";$mavenHome\bin", "User")

$env:MAVEN_HOME = $mavenHome
$env:Path = "$mavenHome\bin;$env:Path"
```

Check Maven:

```powershell
mvn -version
```

## Quick Start

From the repository root:

```powershell
cd .\playwright-java
```

Confirm Maven can see Java:

```powershell
mvn -version
```

Install Chromium once:

```powershell
mvn exec:java@install-chromium
```

Run the examples:

```powershell
mvn test
```

Default runs use `ENV=dev`, `BROWSER=chromium` and `HEADLESS=false`.

## Run Tests

Run all examples:

```powershell
mvn test
```

Run API examples only:

```powershell
mvn test -Dgroups=api
```

Run UI examples only:

```powershell
mvn test -Dgroups=ui
```

Run smoke examples only:

```powershell
mvn test -Dgroups=smoke
```

Run with a different environment, browser or headless mode:

```powershell
mvn test -DENV=test -DBROWSER=firefox -DHEADLESS=true
```

## Run Options

System properties such as `-DENV=test` take precedence over process environment variables.

Supported browsers:

```text
chromium
firefox
edge
```

Edge uses Playwright Chromium with the `msedge` channel. If Edge is requested but not installed, the run fails instead of silently falling back to Chromium.

Install browsers with:

```powershell
mvn exec:java@install-chromium
mvn exec:java@install-firefox
mvn exec:java@install-edge
```

To install all supported browsers in one go:

```powershell
mvn exec:java@install-browsers
```

## Quality Checks

```powershell
mvn test-compile
mvn spotless:check
mvn checkstyle:check
mvn spotless:apply
```

`test-compile` is a quick project validation command because it does not run live public-demo scenarios.

## Environment Data

Environment data lives under:

```text
src/test/java/testtracks/data/environments/dev
src/test/java/testtracks/data/environments/test
```

`all-targets.json` contains named UI sites, named API endpoints, credentials, timeouts and trace settings. The scenario files contain only test data:

```text
api-auth.json
api-booking.json
api-room.json
ui-booking.json
```

There are no magical default targets. Step/support code explicitly requests the named site, API or credential it needs.

## Framework Responsibilities

**Environment = where. Data = what. Builders = make it usable/unique. Steps = intent and orchestration. Supporting test code = application knowledge. Base classes = Playwright plumbing.**

API request payloads are test data first. Scenario JSON is loaded as Jackson `ObjectNode`, builders clone it and make it unique, and thin API classes send it through Playwright.

Every scenario owns the mutable data it creates. Cleanup is registered immediately after successful creation and runs LIFO in teardown, even when the scenario fails.

## Reports and Evidence

Cucumber built-in HTML and message reports are written under:

```text
TestResults/<timestamp>/
```

The report files are:

```text
TestResults/<timestamp>/test-report.html
TestResults/<timestamp>/cucumber-messages.ndjson
```

The report includes run details, API evidence, browser details, console/page errors when captured, and screenshots for UI failures.

Playwright traces are saved separately for failing UI scenarios:

```text
TestResults/<timestamp>/<scenario-safe-name>/trace.zip
```

The trace path is logged into the Cucumber report, but the binary `trace.zip` is not embedded in `test-report.html`.

## Common Setup Fixes

If Maven says `JAVA_HOME` is missing or incorrect, check that Oracle JDK 25 is installed:

```powershell
winget list --id Oracle.JDK.25
```

If Oracle JDK is installed somewhere else, set `JAVA_HOME` once:

```powershell
[Environment]::SetEnvironmentVariable("JAVA_HOME", "C:\Program Files\Java\jdk-25.0.4.1", "User")
```

Close and reopen PowerShell after setting it.

If a UI test says the Chromium executable does not exist, install the browser:

```powershell
mvn exec:java@install-chromium
```

If `BROWSER=edge` fails, install Edge for Playwright:

```powershell
mvn exec:java@install-edge
```

If the public demo site is unavailable or slow, first check the framework without running live examples:

```powershell
mvn test-compile
mvn spotless:check
mvn checkstyle:check
```

## Structure

```text
src/main/java/testtracks
  reusable Playwright API/UI plumbing, runtime, cleanup, configuration and diagnostics

src/test/java/testtracks
  feature files, Cucumber glue, page objects, demo API services, builders, environment data, reporting and support
```

## Design Rules

- No Selenium, REST Assured, Spring, TestNG, Allure or ExtentReports.
- No shared mutable scenario state.
- No fixed sleeps or `networkidle` as readiness.
- UI tests may create prerequisites through API.
- Page objects stay Playwright-native.
- Generic HTTP mechanics live in `BaseApi`.
- Demo quirks stay in demo service/page classes, not base classes.

## Version Notes

Baseline verification on 20 August 2026 found Maven 3.9.16, Playwright Java 1.62.0 and JUnit 6.1.3 current. Cucumber and Jackson had newer patch releases available, so this project uses Cucumber-JVM 7.34.7 and Jackson 2.22.2.
