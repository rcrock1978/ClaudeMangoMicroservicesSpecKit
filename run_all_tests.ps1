$testProjects = @(
    "tests/Mango.Services.Product.UnitTests/Mango.Services.Product.UnitTests.csproj",
    "tests/Mango.Services.ShoppingCart.UnitTests/Mango.Services.ShoppingCart.UnitTests.csproj",
    "tests/Mango.Services.Coupon.UnitTests/Mango.Services.Coupon.UnitTests.csproj",
    "tests/Mango.Services.Order.UnitTests/Mango.Services.Order.UnitTests.csproj",
    "tests/Mango.Services.Email.UnitTests/Mango.Services.Email.UnitTests.csproj",
    "tests/Mango.Services.Reward.UnitTests/Mango.Services.Reward.UnitTests.csproj",
    "tests/Mango.Services.Payment.UnitTests/Mango.Services.Payment.UnitTests.csproj",
    "tests/Mango.Services.Auth.UnitTests/Mango.Services.Auth.UnitTests.csproj",
    "tests/Mango.Services.Admin.Accounts.UnitTests/Mango.Services.Admin.Accounts.UnitTests.csproj"
)

$totalPassed = 0
$totalFailed = 0

foreach ($project in $testProjects) {
    Write-Host "Testing: $project" -ForegroundColor Cyan
    $result = dotnet test $project -v:q --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ PASSED" -ForegroundColor Green
    } else {
        Write-Host "✗ FAILED" -ForegroundColor Red
    }
    Write-Host "---"
}
