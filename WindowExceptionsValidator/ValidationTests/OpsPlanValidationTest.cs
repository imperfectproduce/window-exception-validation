using System.Text.Json;
using WindowExceptionsValidation.Entities;

namespace WindowExceptionsValidation.ValidationTests;

[TestFixture]
public class OpsPlanValidationTests
{
    private const string InputFilePath = @"./csv/Xmas and NY Holiday Market_Zone Exception Tool - Import Zone.csv";

    private const string FilePathToBeValidated = @"./csv/Christmas and New Years Window Exceptions - Ops Plan.csv";

    private readonly List<OpsPlanRecord> _expectedOpsPlan =
        CsvParser.ParseImportZoneWithChangesToDeliveryDay(InputFilePath).ToList();

    [Test]
    public void ShouldHaveExpectedNumberOfWindowExceptions()
    {
        var actualOpsPlan = CsvParser.GetOpsPlans()
            .Where(p => !p.Is_Employee_Zone)
            .ToList();

        Assert.That(actualOpsPlan, Has.Count.EqualTo(_expectedOpsPlan.Count));
        Assert.That(_expectedOpsPlan.Except(actualOpsPlan), Is.Empty);
        Assert.That(actualOpsPlan.Except(_expectedOpsPlan), Is.Empty);
    }

    [Test]
    public void ShouldContainMatchingOutputForEachInput()
    {
        var actualOpsPlan = CsvParser.GetOpsPlans().ToList();

        foreach (var record in _expectedOpsPlan) Assert.That(actualOpsPlan, Contains.Item(record));
    }

    [Test]
    public void ShouldHaveContentInAllColumns()
    {
        var actualOpsPlan = CsvParser.GetOpsPlans().ToList();

        foreach (var record in actualOpsPlan)
        {
            Assert.That(record.City_Name, Is.Not.Empty);
            Assert.That(record.Original_Delivery_Day.ToString(), Is.Not.Empty);
            Assert.That(record.Exception_Delivery_Day.ToString(), Is.Not.Empty);
            Assert.That(record.Original_Delivery_Date, Is.GreaterThanOrEqualTo(0));
            Assert.That(record.Exception_Delivery_Date, Is.GreaterThanOrEqualTo(0));
        }
    }

    [Test]
    public void ShouldNotContainDuplicates()
    {
        var actualOpsPlan = CsvParser.GetOpsPlans().ToList();

        Assert.That(actualOpsPlan.GroupBy(r => r).Count(r => r.Count() > 1), Is.Zero);
    }

    [Test]
    public void ShouldNotContainUnchangedWindows()
    {
        var opsPlan = CsvParser.GetOpsPlans().ToList();

        var outputUnchangedByDay = opsPlan.Count(r => r.Exception_Delivery_Day == r.Original_Delivery_Day);
        var outputUnchangedByDate = opsPlan.Count(r => r.Exception_Delivery_Date == r.Original_Delivery_Date);

        Assert.That(outputUnchangedByDay, Is.EqualTo(0));
        Assert.That(outputUnchangedByDate, Is.EqualTo(0));
    }

    [Test]
    public void ShouldMatchImportZoneTotals()
    {
        const int laxDupeThatShouldHaveBeenRemoved = 1;
        var expectedEmpCount = CsvParser.GetEmpZones().Count(z => z.Original_Delivery_Day != z.Exception_Delivery_Day);
        
        var actualEmpCount = CsvParser.GetOpsPlans().Count(p => p.Is_Employee_Zone);

        Assert.That(actualEmpCount, Is.EqualTo(expectedEmpCount - laxDupeThatShouldHaveBeenRemoved));
    }

    [Test]
    public void ShouldSummarizeOpsPlan()
    {
        var contentSplitByLine = File.ReadAllLines(FilePathToBeValidated);
        var opsPlan = CsvParser.GetOpsPlans().ToList();
        var unchanged = opsPlan
            .Where(r => r.Exception_Delivery_Day == r.Original_Delivery_Day)
            .ToList();

        Console.WriteLine($"{contentSplitByLine.Length} lines");
        Console.WriteLine($"{opsPlan.Count} total window exceptions");
        Console.WriteLine($"{unchanged.Count()} unchanged windows");
        Console.WriteLine(JsonSerializer.Serialize(unchanged.Select(p => p.old_window_id).ToList()));
        Assert.Pass();
    }
}