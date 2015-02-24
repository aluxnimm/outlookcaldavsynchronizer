// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using CalDavSynchronizer.EntityMapping;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;
using CalDavSynchronizer.InitialEntityMatching;
using CalDavSynchronizer.Synchronization;
using NUnit.Framework;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  internal class TestSynchronizerSetup : ISynchronizerContext<string, string, int, string, string, int>
  {
    public VersionStorage<string, int> _localVersions;
    public VersionStorage<string, int> _serverVersions;

    public TestRepository _localRepository;
    public TestRepository _serverRepository;

    protected EntityRelationStorage<string, string> _entityRelationStorage;
    protected ISynchronizer _synchronizer;


    public TestSynchronizerSetup ()
    {
      _localVersions = new VersionStorage<string, int>();
      _serverVersions = new VersionStorage<string, int>();

      _localRepository = new TestRepository ("l");
      _serverRepository = new TestRepository ("s");

      _entityRelationStorage = new EntityRelationStorage<string, string>();
    }

    public void AssertServerCount (int i)
    {
      Assert.AreEqual (i, _serverRepository.Count);
    }

    public void AssertLocalCount (int i)
    {
      Assert.AreEqual (i, _localRepository.Count);
    }

    public void AssertServer (string id, int version, string content)
    {
      Assert.AreEqual (Tuple.Create (version, content), _serverRepository[id]);
    }

    public void AssertLocal (string id, int version, string content)
    {
      Assert.AreEqual (Tuple.Create (version, content), _localRepository[id]);
    }

    public void AssertLocal (int version, string content)
    {
      CollectionAssert.Contains (_localRepository.EntityVersionAndContentById.Values, Tuple.Create (version, content));
    }

    public void AssertServer (int version, string content)
    {
      CollectionAssert.Contains (_serverRepository.EntityVersionAndContentById.Values, Tuple.Create (version, content));
    }


    public void InitializeWithTwoEvents ()
    {
      var v1 = _localRepository.Create (v => "Item 1");
      var v2 = _localRepository.Create (v => "Item 2");

      var v3 = _serverRepository.Create (v => "Item 1");
      var v4 = _serverRepository.Create (v => "Item 2");

      _localVersions.SetNewVersions (new[] { v1, v2 });
      _serverVersions.SetNewVersions (new[] { v3, v4 });

      _entityRelationStorage.AddRelation (v1.Id, v3.Id);
      _entityRelationStorage.AddRelation (v2.Id, v4.Id);
    }

    public IEntityMapper<string, string> EntityMapper
    {
      get { return new Mapper(); }
    }

    public EntityRepositoryBase<string, string, int> AtypeRepository
    {
      get { return _localRepository; }
    }

    public EntityRepositoryBase<string, string, int> BtypeRepository
    {
      get { return _serverRepository; }
    }


    public DateTime From
    {
      get { return DateTime.Now; }
    }

    public DateTime To
    {
      get { return DateTime.Now; }
    }

    public InitialEntityMatcher<string, string, int, string, string, int> InitialEntityMatcher {
      get { throw new NotImplementedException(); }
    }

    public void SaveChaches (EntityCaches<string, int, string, int> caches)
    {
    }

    public EntityCaches<string, int, string, int> LoadOrCreateCaches (out bool createdNew)
    {
      createdNew = false;

      return new EntityCaches<string, int, string, int> (
          _localVersions,
          _serverVersions,
          _entityRelationStorage
          );
    }
  }
}