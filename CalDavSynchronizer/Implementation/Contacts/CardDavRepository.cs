// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using GenSync.EntityRepositories;
using log4net;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class CardDavRepository : IEntityRepository<vCard, Uri, string>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICardDavDataAccess _cardDavDataAccess;
    private readonly vCardStandardWriter _vCardWriter;
    private readonly EntityType _entityType;
    private readonly IDateTimeRangeProvider _dateTimeRangeProvider;

    public enum EntityType
    {
      Event,
      Todo
    }

    public CardDavRepository (ICardDavDataAccess cardDavDataAccess, EntityType entityType, IDateTimeRangeProvider dateTimeRangeProvider)
    {
      _cardDavDataAccess = cardDavDataAccess;
      _vCardWriter = new vCardStandardWriter();
      _entityType = entityType;
      _dateTimeRangeProvider = dateTimeRangeProvider;
    }

    public IReadOnlyList<EntityIdWithVersion<Uri, string>> GetVersions ()
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CardDavRepository.GetVersions"))
      {
        return _cardDavDataAccess.GetContacts ();
      }
    }

    public Task<IReadOnlyList<EntityWithVersion<Uri, vCard>>> Get (ICollection<Uri> ids)
    {
      return Task.Factory.StartNew (() =>
      {
        if (ids.Count == 0)
          return new EntityWithVersion<Uri, vCard>[] { };

        using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("CardDavRepository.Get ({0} entitie(s))", ids.Count)))
        {
          var entities = _cardDavDataAccess.GetEntities (ids);
          return ParallelDeserialize (entities);
        }
      });
    }

    public void Cleanup (IReadOnlyDictionary<Uri, vCard> entities)
    {
      // nothing to do
    }

    private IReadOnlyList<EntityWithVersion<Uri, vCard>> ParallelDeserialize (IReadOnlyList<EntityWithVersion<Uri, string>> serializedEntities)
    {
      var result = new List<EntityWithVersion<Uri, vCard>>();

      Parallel.ForEach (
          serializedEntities,
          () => Tuple.Create (new vCardStandardReader(), new List<Tuple<Uri, vCard>>()),
          (serialized, loopState, threadLocal) =>
          {
            vCard vcard;

            if (TryDeserialize (serialized.Entity, out vcard, serialized.Id, threadLocal.Item1))
              threadLocal.Item2.Add (Tuple.Create (serialized.Id, vcard));
            return threadLocal;
          },
          threadLocal =>
          {
            lock (result)
            {
              foreach (var calendar in threadLocal.Item2)
                result.Add (EntityWithVersion.Create (calendar.Item1, calendar.Item2));
            }
          });

      return result;
    }


    public bool Delete (Uri entityId)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return _cardDavDataAccess.DeleteEntity (entityId);
      }
    }

    public EntityIdWithVersion<Uri, string> Update (Uri entityId, vCard entityToUpdate, Func<vCard, vCard> entityModifier)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        vCard newVcard = new vCard ();
        newVcard = entityModifier (newVcard);
    

        return _cardDavDataAccess.UpdateEntity (entityId, Serialize (newVcard));
      }
    }

    public EntityIdWithVersion<Uri, string> Create (Func<vCard, vCard> entityInitializer)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        vCard newVcard = new vCard ();
        newVcard = entityInitializer (newVcard);
        return _cardDavDataAccess.CreateEntity (Serialize (newVcard));
      }
    }


    private string Serialize (vCard vcard)
    {
      using (var writer = new StringWriter())
      {
        _vCardWriter.Write (vcard, writer);
        writer.Flush();
        return writer.GetStringBuilder().ToString();
      }
    }

    private static bool TryDeserialize (string iCalData, out vCard vcard, Uri uriOfCalendarForLogging, vCardStandardReader deserializer)
    {
      vcard = null;
      try
      {
        vcard = Deserialize (iCalData, deserializer);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserilaize ICalData of '{0}':\r\n{1}", uriOfCalendarForLogging, iCalData), x);
        return false;
      }
    }

    private static vCard Deserialize (string vcardData, vCardStandardReader serializer)
    {
      using (var reader = new StringReader (vcardData))
      {
        return serializer.Read (reader);
      }
    }
  }
}