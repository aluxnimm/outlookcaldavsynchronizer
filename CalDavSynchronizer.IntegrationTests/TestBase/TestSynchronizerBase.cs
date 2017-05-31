using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.Synchronization;
using GenSync.Logging;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  public abstract class TestSynchronizerBase
  {
    protected IOutlookSynchronizer Synchronizer { get; private set; }
    public readonly TestComponentContainer TestComponentContainer;
    protected Options Options;

    protected TestSynchronizerBase(Options options, TestComponentContainer testComponentContainer)
    {
      Options = options ?? throw new ArgumentNullException(nameof(options));
      TestComponentContainer = testComponentContainer ?? throw new ArgumentNullException(nameof(testComponentContainer));
    }

    public abstract Task DeleteAllEntites();

    protected abstract Task<IOutlookSynchronizer> InitializeOverride();

    public async Task Initialize()
    {
      Synchronizer = await InitializeOverride();
    }

    public void ClearCache()
    {
      var profileDataDirectory = TestComponentContainer.ComponentContainer.GetProfileDataDirectory(Options.Id);
      if (Directory.Exists(profileDataDirectory))
        Directory.Delete(profileDataDirectory, true);
    }

    public async Task ClearEventRepositoriesAndCache()
    {
      await DeleteAllEntites();
      ClearCache();
    }

    

    public async Task SynchronizeAndCheck(
      int? unchangedA, int? addedA, int? changedA, int? deletedA,
      int? unchangedB, int? addedB, int? changedB, int? deletedB,
      int? createA, int? updateA, int? deleteA,
      int? createB, int? updateB, int? deleteB,
      int? ordinalOfReportToCheck = null,
      int? maximumOpenItemsPerType = null)
    {
      TestComponentContainer.SetMaximumOpenItemsPerType(maximumOpenItemsPerType);
      try
      {
        var report = await SynchronizeAndAssertNoErrors();

        var aDelta = ParseDelta(ExtractLoggerInfo(report.ADelta, ordinalOfReportToCheck));
        AssertEqual(aDelta.Unchanged, unchangedA);
        AssertEqual(aDelta.Added, addedA);
        AssertEqual(aDelta.Deleted, deletedA);
        AssertEqual(aDelta.Changed, changedA);

        var bDelta = ParseDelta(ExtractLoggerInfo(report.BDelta, ordinalOfReportToCheck));
        AssertEqual(bDelta.Unchanged, unchangedB);
        AssertEqual(bDelta.Added, addedB);
        AssertEqual(bDelta.Deleted, deletedB);
        AssertEqual(bDelta.Changed, changedB);

        var aJobs = ParseJobInfo(ExtractLoggerInfo(report.AJobsInfo, ordinalOfReportToCheck));
        AssertEqual(aJobs.Create, createA);
        AssertEqual(aJobs.Update, updateA);
        AssertEqual(aJobs.Delete, deleteA);

        var bJobs = ParseJobInfo(ExtractLoggerInfo(report.BJobsInfo, ordinalOfReportToCheck));
        AssertEqual(bJobs.Create, createB);
        AssertEqual(bJobs.Update, updateB);
        AssertEqual(bJobs.Delete, deleteB);
      }
      finally
      {
        TestComponentContainer.SetMaximumOpenItemsPerType(null);
      }
    }

    void AssertEqual(int current, int? expected)
    {
      if (expected == null)
        return;

      Assert.That(current, Is.EqualTo(expected));
    }

    protected virtual string ExtractLoggerInfo(string lineWithAllInfos,int? reportNumber)
    {
      if (reportNumber == null)
        return lineWithAllInfos;

      return 
        lineWithAllInfos
        .Trim()
        .Replace("(", "")
        .Replace(")", "")
        .Split(new[] { '|'})
        .Select(s => s.Trim())
        .ElementAt(reportNumber.Value);
    }


    (int Create, int Update, int Delete) ParseJobInfo(string jobInfo)
    {
      var match = Regex.Match(jobInfo, @"Create (?<create>\d+) , Update (?<update>\d+) , Delete (?<delete>\d+)");
      Assert.That(match.Success, Is.True);
      return (int.Parse(match.Groups["create"].Value), int.Parse(match.Groups["update"].Value), int.Parse(match.Groups["delete"].Value));
    }

    (int Unchanged, int Added, int Deleted, int Changed) ParseDelta(string delta)
    {
      var match = Regex.Match(delta, @"Unchanged: (?<unchanged>\d+) , Added: (?<added>\d+) , Deleted (?<deleted>\d+) ,  Changed (?<changed>\d+)");
      Assert.That(match.Success, Is.True);
      return (int.Parse(match.Groups["unchanged"].Value), int.Parse(match.Groups["added"].Value), int.Parse(match.Groups["deleted"].Value), int.Parse(match.Groups["changed"].Value));
    }


    public async Task<SynchronizationReport> SynchronizeAndAssertNoErrors()
    {
      var report = await Synchronize();
      Assert.That(report.HasErrors, Is.False);
      Assert.That(report.HasWarnings, Is.False);
      return report;
    }

    public async Task<SynchronizationReport> Synchronize()
    {
      var reportSink = new TestReportSink();

      using (var logger = new SynchronizationLogger(Options.Id, Options.Name, reportSink, TestComponentContainer.GeneralOptions.IncludeEntityReportsWithoutErrorsOrWarnings))
      {
        await Synchronizer.Synchronize(logger);
      }

      return reportSink.SynchronizationReport;
    }
  }
}