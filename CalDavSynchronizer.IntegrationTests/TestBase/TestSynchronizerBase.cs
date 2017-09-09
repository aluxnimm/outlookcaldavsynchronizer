using System;
using System.IO;
using System.Linq;
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
      int unchangedA, int addedA, int changedA, int deletedA,
      int unchangedB, int addedB, int changedB, int deletedB,
      int createA, int updateA, int deleteA,
      int createB, int updateB, int deleteB,
      int? ordinalOfReportToCheck = null,
      int? maximumOpenItemsPerType = null)
    {
      TestComponentContainer.SetMaximumOpenItemsPerType(maximumOpenItemsPerType);
      try
      {
        var report = await SynchronizeAndAssertNoErrors();
        Assert.That(ExtractLoggerInfo(report.ADelta, ordinalOfReportToCheck), Is.EqualTo($"Unchanged: {unchangedA} , Added: {addedA} , Deleted {deletedA} ,  Changed {changedA}"));
        Assert.That(ExtractLoggerInfo(report.BDelta, ordinalOfReportToCheck), Is.EqualTo($"Unchanged: {unchangedB} , Added: {addedB} , Deleted {deletedB} ,  Changed {changedB}"));
        Assert.That(ExtractLoggerInfo(report.AJobsInfo, ordinalOfReportToCheck), Is.EqualTo($"Create {createA} , Update {updateA} , Delete {deleteA}"));
        Assert.That(ExtractLoggerInfo(report.BJobsInfo, ordinalOfReportToCheck), Is.EqualTo($"Create {createB} , Update {updateB} , Delete {deleteB}"));
      }
      finally
      {
        TestComponentContainer.SetMaximumOpenItemsPerType(null);
      }
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