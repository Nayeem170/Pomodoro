# Script to identify slow tests in Pomodoro.Web.Tests
# Run this to find which tests are taking the most time

Write-Host "Identifying slow tests..." -ForegroundColor Cyan

# Run tests with detailed timing
$testOutput = dotnet test tests/Pomodoro.Web.Tests/Pomodoro.Web.Tests.csproj --logger "console;verbosity=detailed" --no-build 2>&1

# Extract test timing information
$slowTests = @()

foreach ($line in $testOutput) {
    if ($line -match "Passed\s+(.+?)\s+\((\d+)ms\)|Failed\s+(.+?)\s+\((\d+)ms\)|Skipped\s+(.+?)\s+\((\d+)ms\)") {
        $testName = $matches[1]
        $duration = [int]$matches[2]
        
        if ($duration -gt 1000) {
            $slowTests += [PSCustomObject]@{
                TestName = $testName
                Duration = $duration
                DurationFormatted = "$($duration / 1000)s"
            }
        }
    }
}

# Sort by duration (slowest first)
$slowTests = $slowTests | Sort-Object -Property Duration -Descending

# Display results
Write-Host "`n=== SLOW TESTS (> 1 second) ===" -ForegroundColor Yellow
if ($slowTests.Count -eq 0) {
    Write-Host "No slow tests found!" -ForegroundColor Green
} else {
    $slowTests | Format-Table -AutoSize
    
    Write-Host "`n=== SUMMARY ===" -ForegroundColor Yellow
    Write-Host "Total slow tests: $($slowTests.Count)" -ForegroundColor Red
    Write-Host "Total time spent on slow tests: $([math]::Round(($slowTests | Measure-Object -Property Duration -Sum).Sum / 1000, 2)) seconds" -ForegroundColor Red
}

# Save to file
$slowTests | Export-Csv -Path "slow-tests.csv" -NoTypeInformation
Write-Host "`nSlow tests saved to: slow-tests.csv" -ForegroundColor Cyan

# Suggest next steps
Write-Host "`n=== RECOMMENDATIONS ===" -ForegroundColor Cyan
Write-Host "1. Review slow tests above for optimization opportunities" -ForegroundColor White
Write-Host "2. Consider using [Trait('Category', 'Slow')] on slow tests" -ForegroundColor White
Write-Host "3. Run only fast tests during development: dotnet test --filter 'Category!=Slow'" -ForegroundColor White
Write-Host "4. Optimize tests with Task.Delay() - use synchronization instead" -ForegroundColor White
