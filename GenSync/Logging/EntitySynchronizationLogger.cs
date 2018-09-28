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
using System.Reflection;
using log4net;

namespace GenSync.Logging
{
  public class EntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>  : IFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>
    {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly SynchronizationOperation _operation;
    private List<string> _mappingErrors ;
    private List<string> _mappingWarnings;
    private string _aId;
    private string _bId;
    private string _aLogDisplayName;
    private string _bLogDisplayName;
    private string _exceptionThatLeadToAbortion;
    private readonly IEntityLogMessageFactory<TAtypeEntity, TBtypeEntity> _entityLogMessageFactory;

    public EntitySynchronizationLogger(SynchronizationOperation operation, IEntityLogMessageFactory<TAtypeEntity, TBtypeEntity> entityLogMessageFactory)
    {
      _operation = operation;
      _entityLogMessageFactory = entityLogMessageFactory ?? throw new ArgumentNullException(nameof(entityLogMessageFactory));
    }

    private List<string> MappingErrors => _mappingErrors ?? (_mappingErrors = new List<string>());
    private List<string> MappingWarnings => _mappingWarnings ?? (_mappingWarnings = new List<string>());

    public event EventHandler Disposed;

    void OnDisposed ()
    {
      var handler = Disposed;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    public void LogMappingError (string message)
    {
      MappingErrors.Add (message);
    }

    public void LogMappingError (string message, Exception exception)
    {
      MappingErrors.Add (message + " : " + exception.ToString());
    }

    public void LogMappingWarning (string warning)
    {
      MappingWarnings.Add (warning);
    }

    public void LogMappingWarning (string warning, Exception exception)
    {
      MappingWarnings.Add (warning + " : " + exception.ToString ());
    }

    public void SetAId (TAtypeEntityId id)
    {
      _aId = id.ToString();
    }

    public void SetBId (TBtypeEntityId id)
    {
      _bId = id.ToString();
    }

    public void LogA(TAtypeEntity entity)
    {
      try
      {
        _aLogDisplayName = _entityLogMessageFactory.GetADisplayNameOrNull(entity);
      }
      catch (Exception x)
      {
        s_logger.Error(null, x);
        _aLogDisplayName = "<Error>";
      }
    }

    public void LogB(TBtypeEntity entity)
    {
      try
      {
        _bLogDisplayName = _entityLogMessageFactory.GetBDisplayNameOrNull(entity);
      }
      catch (Exception x)
      {
        s_logger.Error(null, x);
        _bLogDisplayName = "<Error>";
      }
    }
    
    public void LogAbortedDueToError (Exception exception)
    {
      _exceptionThatLeadToAbortion = exception.ToString();
    }

    public void LogAbortedDueToError (string errorMessage)
    {
      _exceptionThatLeadToAbortion = errorMessage;
    }

    public EntitySynchronizationReport GetReport ()
    {
      return new EntitySynchronizationReport()
      {
        AId = _aId,
        BId = _bId,
        ADisplayName = _aLogDisplayName,
        BDisplayName = _bLogDisplayName,
        ExceptionThatLeadToAbortion = _exceptionThatLeadToAbortion,
        MappingErrors =  _mappingErrors?.ToArray() ?? new string[0],
        MappingWarnings = _mappingWarnings?.ToArray() ?? new string[0],
        Operation = _operation
      };
    }

    public bool HasErrorsOrWarnings =>
        _mappingErrors?.Count > 0
        || _mappingWarnings?.Count > 0
        || !string.IsNullOrEmpty (_exceptionThatLeadToAbortion);

    public void Dispose ()
    {
      OnDisposed();
    }
  }
}