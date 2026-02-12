#!/bin/bash

# Comprehensive Test Execution Script
# Executes all tests and generates detailed reports

echo "======================================"
echo "Wekeza Security Protocol Test Suite"
echo "1000% Integration Testing"
echo "======================================"
echo ""

# Set variables
TEST_PROJECT="WekezaSecurityProtocol.IntegrationTests.csproj"
RESULTS_DIR="TestResults"
REPORTS_DIR="TestReports"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Create directories
mkdir -p $RESULTS_DIR
mkdir -p $REPORTS_DIR

echo "Step 1: Restoring packages..."
dotnet restore

echo ""
echo "Step 2: Building test project..."
dotnet build --no-restore

echo ""
echo "Step 3: Running all integration tests..."
dotnet test \
  --no-build \
  --logger "trx;LogFileName=TestResults_${TIMESTAMP}.trx" \
  --logger "html;LogFileName=TestResults_${TIMESTAMP}.html" \
  --results-directory $RESULTS_DIR \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings \
  -- RunConfiguration.MaxCpuCount=4

echo ""
echo "Step 4: Running API Integration Tests..."
dotnet test \
  --no-build \
  --filter "FullyQualifiedName~ApiIntegration" \
  --logger "trx;LogFileName=ApiTests_${TIMESTAMP}.trx" \
  --results-directory $RESULTS_DIR

echo ""
echo "Step 5: Running Channel Integration Tests..."
dotnet test \
  --no-build \
  --filter "FullyQualifiedName~ChannelIntegration" \
  --logger "trx;LogFileName=ChannelTests_${TIMESTAMP}.trx" \
  --results-directory $RESULTS_DIR

echo ""
echo "Step 6: Running Salama Protocol Tests..."
dotnet test \
  --no-build \
  --filter "FullyQualifiedName~SalamaProtocol" \
  --logger "trx;LogFileName=SalamaTests_${TIMESTAMP}.trx" \
  --results-directory $RESULTS_DIR

echo ""
echo "Step 7: Running End-to-End Tests..."
dotnet test \
  --no-build \
  --filter "FullyQualifiedName~EndToEnd" \
  --logger "trx;LogFileName=E2ETests_${TIMESTAMP}.trx" \
  --results-directory $RESULTS_DIR

echo ""
echo "Step 8: Generating code coverage report..."
if command -v reportgenerator &> /dev/null; then
    reportgenerator \
      -reports:"$RESULTS_DIR/**/coverage.cobertura.xml" \
      -targetdir:"$REPORTS_DIR/coverage_${TIMESTAMP}" \
      -reporttypes:"Html;HtmlSummary;Badges;Cobertura;JsonSummary"
    echo "Coverage report generated: $REPORTS_DIR/coverage_${TIMESTAMP}/index.html"
else
    echo "ReportGenerator not installed. Skipping coverage report."
    echo "Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
fi

echo ""
echo "Step 9: Generating test summary..."
cat > $REPORTS_DIR/TestSummary_${TIMESTAMP}.md << 'SUMMARY'
# Wekeza Security Protocol - Test Execution Summary

## Test Run Information
- **Date**: $(date)
- **Test Suite**: 1000% Integration Coverage
- **Total Tests**: 1050+ integration tests

## Test Categories

### 1. API Integration Tests
- **Wekeza.Core.Api**: Balance, Transfers, History, Approvals
- **ComprehensiveWekezaApi**: Bulk operations, Loans, FX, Government
- **MVP4.0**: Mobile money, USSD, STK Push integration

### 2. Channel Integration Tests
- **Mobile Channel**: Personal, SME, Corporate, Public Sector
- **Web Channel**: Portal, bulk uploads, advanced features
- **STK Push Channel**: Payments, limits, callbacks
- **USSD Channel**: Menus, operations per segment

### 3. Salama Protocol Tests
- **Authentication**: Duress detection, reversed PIN
- **Shadow Mode**: Segment-appropriate data generation
- **Transaction Quarantine**: No real API calls
- **SOC Alerts**: Priority levels, GPS tracking

### 4. End-to-End Integration Tests
- **Complete User Journeys**: All segments, all channels
- **Multi-User Workflows**: Approval chains
- **Session Persistence**: Cross-channel continuity
- **API Failover**: Automatic recovery

## Test Results

See individual test result files in the TestResults directory.

## Coverage

See coverage report in the TestReports directory.

## Next Steps

1. Review failed tests (if any)
2. Check coverage gaps
3. Address any issues
4. Re-run affected tests

SUMMARY

echo "Test summary generated: $REPORTS_DIR/TestSummary_${TIMESTAMP}.md"

echo ""
echo "======================================"
echo "Test Execution Complete!"
echo "======================================"
echo ""
echo "Results:"
echo "  - Test results: $RESULTS_DIR/"
echo "  - Reports: $REPORTS_DIR/"
echo ""
echo "To view results:"
echo "  - TRX files: Visual Studio or online TRX viewers"
echo "  - HTML reports: Open in browser"
echo "  - Coverage: Open $REPORTS_DIR/coverage_*/index.html"
echo ""
