// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GenSync.Logging
{
  public class SynchronizationReport
  {
    private LoadError[] _loadErrors;
    private EntitySynchronizationReport[] _entitySynchronizationReports;
    public string ProfileName { get; set; }
    public Guid ProfileId { get; set; }
    public DateTime StartTime { get; set; }

    [XmlIgnore]
    public TimeSpan Duration { get; set; }

    public string ADelta { get; set; }
    public string BDelta { get; set; }
    public string AJobsInfo { get; set; }
    public string BJobsInfo { get; set; }

    public LoadError[] LoadErrors
    {
      get { return _loadErrors ?? new LoadError[] { }; }
      set { _loadErrors = value; }
    }

    public EntitySynchronizationReport[] EntitySynchronizationReports
    {
      get { return _entitySynchronizationReports ?? new EntitySynchronizationReport[] { }; }
      set { _entitySynchronizationReports = value; }
    }

    public string ExceptionThatLeadToAbortion { get; set; }
    public bool ConsiderExceptionThatLeadToAbortionAsWarning { get; set; }

    [XmlElement (ElementName = "Duration")]
    public string Duration_ForSerializationOnly
    {
      get { return Duration.ToString(); }
      set { Duration = TimeSpan.Parse (value); }
    }

    public bool HasErrors
    {
      get
      {
        return !string.IsNullOrEmpty(ExceptionThatLeadToAbortion) && !ConsiderExceptionThatLeadToAbortionAsWarning
               || EntitySynchronizationReports.Any(r => !string.IsNullOrEmpty(r.ExceptionThatLeadToAbortion) || r.Errors.Length > 0)
               || LoadErrors.Any(m => !m.IsWarning);
      }
    }

    public bool HasWarnings => !string.IsNullOrEmpty(ExceptionThatLeadToAbortion) && ConsiderExceptionThatLeadToAbortionAsWarning ||
                               EntitySynchronizationReports.Any(r => r.Warnings.Length > 0) ||
                               LoadErrors.Any(m => m.IsWarning);

    public void MergeSubReport(IReadOnlyCollection<SynchronizationReport> subReports)
    {
      if (subReports.Count == 0)
        return;

      if (!string.IsNullOrWhiteSpace(ExceptionThatLeadToAbortion) || subReports.Any(r => !string.IsNullOrWhiteSpace(r.ExceptionThatLeadToAbortion)))
      {
        var exceptionBuilder = new StringBuilder();
        exceptionBuilder.Append("'");
        exceptionBuilder.Append(ProfileName);
        exceptionBuilder.Append("'\r\n");

        if (!string.IsNullOrWhiteSpace(ExceptionThatLeadToAbortion))
          exceptionBuilder.AppendLine(ExceptionThatLeadToAbortion);

        foreach (var subReport in subReports)
        {
          exceptionBuilder.Append("'");
          exceptionBuilder.Append(subReport.ProfileName);
          exceptionBuilder.Append("'\r\n");

          if (!string.IsNullOrWhiteSpace(subReport.ExceptionThatLeadToAbortion))
            exceptionBuilder.AppendLine(subReport.ExceptionThatLeadToAbortion);
        }

        ExceptionThatLeadToAbortion = exceptionBuilder.ToString();
      }

      ProfileName = $"{ProfileName} ( {string.Join(" | ", subReports.Select(r => r.ProfileName))} )";
      ADelta = $"{ADelta} ( {string.Join(" | ", subReports.Select(r => r.ADelta))} )";
      BDelta = $"{BDelta} ( {string.Join(" | ", subReports.Select(r => r.BDelta))} )";
      AJobsInfo = $"{AJobsInfo} ( {string.Join(" | ", subReports.Select(r => r.AJobsInfo))} )";
      BJobsInfo = $"{BJobsInfo} ( {string.Join(" | ", subReports.Select(r => r.BJobsInfo))} )";

      var entitySynchronizationReports = new List<EntitySynchronizationReport>(EntitySynchronizationReports);
      var loadErrors = new List<LoadError>(LoadErrors);

      foreach (var subReport in subReports)
      {
       entitySynchronizationReports.AddRange(subReport.EntitySynchronizationReports);
        loadErrors.AddRange(subReport.LoadErrors);
      }

      EntitySynchronizationReports = entitySynchronizationReports.ToArray();
      LoadErrors = loadErrors.ToArray();
    }
  }
}