# Execute Mutation Testing (exercise)

1. Install stryker

```
dotnet tool install -g dotnet-stryker
```

2. Run stryker

```
dotnet stryker
```

3. Examine results

- Go to ./StrykerOutput folder
- Open the latest report with name `mutation-report.html` in the browser
- Try to get familiar with the report and its metrics
- Check the result of `Domain/StockLevel.cs` file and add missing tests/assertions in the code
- Run stryker again to see if all is fixed
