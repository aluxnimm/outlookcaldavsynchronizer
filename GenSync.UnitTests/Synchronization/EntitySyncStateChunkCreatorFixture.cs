using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.Synchronization;
using NUnit.Framework;
using Rhino.Mocks;

namespace GenSync.UnitTests.Synchronization
{
  [TestFixture]
  public class EntitySyncStateChunkCreatorFixture
  {
    private const int ChunkSize = 5;
    private int _nextId = 0;
    private EntitySyncStateChunkCreator<int, int, int, int, int, int, int> _entitySyncStateChunkCreator;

    [SetUp]
    public void SetUp()
    {
      _entitySyncStateChunkCreator = new EntitySyncStateChunkCreator<int, int, int, int, int, int, int>(ChunkSize);
    }

    [Test]
    public void CreateChunks_NoContextLoadsAnEntity_Creates1Chunk()
    {
      var entitySyncStateContexts = new[]
      {
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,false),
      };

      var chunks =_entitySyncStateChunkCreator.CreateChunks(entitySyncStateContexts, EqualityComparer<int>.Default, EqualityComparer<int>.Default).ToArray();

      Assert.That(chunks.Count, Is.EqualTo(1));
      Assert.That(chunks[0].AEntitesToLoad.Count, Is.EqualTo(0));
      Assert.That(chunks[0].BEntitesToLoad.Count, Is.EqualTo(0));
      Assert.That(chunks[0].Contexts, Is.EqualTo(entitySyncStateContexts));
    }

    [Test]
    public void CreateChunks_ContextsLoadAnEntity_Creates1Chunks()
    {
      var entitySyncStateContexts = new[]
      {
        CreateContext(false,false),
        CreateContext(true,true),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,true),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(true,true),
        CreateContext(false,false),
        CreateContext(false,true),
        CreateContext(false,false),
        CreateContext(true,false),
      };

      var chunks = _entitySyncStateChunkCreator.CreateChunks(entitySyncStateContexts, EqualityComparer<int>.Default, EqualityComparer<int>.Default).ToArray();

      Assert.That(chunks.Count, Is.EqualTo(1));
      Assert.That(chunks[0].AEntitesToLoad.Count, Is.EqualTo(3));
      Assert.That(chunks[0].BEntitesToLoad.Count, Is.EqualTo(4));
      Assert.That(chunks[0].Contexts, Is.EqualTo(entitySyncStateContexts));
    }

    [Test]
    public void CreateChunks_6ContextsLoadAnEntityButOnly4EntitiesOfEachRepository_Creates1Chunks()
    {
      var entitySyncStateContexts = new[]
      {
        CreateContext(true,false),
        CreateContext(true,true),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(false,true),
        CreateContext(false,false),
        CreateContext(false,false),
        CreateContext(true,true),
        CreateContext(false,false),
        CreateContext(false,true),
        CreateContext(false,false),
        CreateContext(true,false),
      };

      var chunks = _entitySyncStateChunkCreator.CreateChunks(entitySyncStateContexts, EqualityComparer<int>.Default, EqualityComparer<int>.Default).ToArray();

      Assert.That(chunks.Count, Is.EqualTo(1));
      Assert.That(chunks[0].AEntitesToLoad.Count, Is.EqualTo(4));
      Assert.That(chunks[0].BEntitesToLoad.Count, Is.EqualTo(4));
      Assert.That(chunks[0].Contexts, Is.EqualTo(entitySyncStateContexts));
    }

    [Test]
    public void CreateChunks_6ContextsLoadATypeEntity_Creates2Chunks()
    {
      var entitySyncStateContexts = new[]
      {
        CreateContext(true,false),
        CreateContext(true,true),
        CreateContext(false,false),
        CreateContext(true,false),
        CreateContext(true,true),
        CreateContext(true,false),

        CreateContext(false,false),
        CreateContext(true,true),
        CreateContext(false,false),
        CreateContext(false,true),
        CreateContext(false,false),
        CreateContext(true,false),
      };

      var chunks = _entitySyncStateChunkCreator.CreateChunks(entitySyncStateContexts, EqualityComparer<int>.Default, EqualityComparer<int>.Default).ToArray();

      Assert.That(chunks.Count, Is.EqualTo(2));

      Assert.That(chunks[0].AEntitesToLoad.Count, Is.EqualTo(5));
      Assert.That(chunks[0].BEntitesToLoad.Count, Is.EqualTo(2));
      Assert.That(chunks[0].Contexts.Count, Is.EqualTo(6));
      Assert.That(chunks[0].Contexts, Is.SubsetOf(entitySyncStateContexts));

      Assert.That(chunks[1].AEntitesToLoad.Count, Is.EqualTo(2));
      Assert.That(chunks[1].BEntitesToLoad.Count, Is.EqualTo(2));
      Assert.That(chunks[1].Contexts.Count, Is.EqualTo(6));
      Assert.That(chunks[1].Contexts, Is.SubsetOf(entitySyncStateContexts));
    }

    [Test]
    public void CreateChunks_6ContextsLoadBTypeEntity_Creates2Chunks()
    {
      var entitySyncStateContexts = new[]
      {
        CreateContext(false ,true),
        CreateContext(true  ,true),
        CreateContext(false,false),
        CreateContext(false ,true),
        CreateContext(true  ,true),
        CreateContext(false ,true),

        CreateContext(false,false),
        CreateContext(true,true),
        CreateContext(false,false),
        CreateContext(false,true),
        CreateContext(false,false),
        CreateContext(true,false),
      };

      var chunks = _entitySyncStateChunkCreator.CreateChunks(entitySyncStateContexts, EqualityComparer<int>.Default, EqualityComparer<int>.Default).ToArray();

      Assert.That(chunks.Count, Is.EqualTo(2));

      Assert.That(chunks[0].AEntitesToLoad.Count, Is.EqualTo(2));
      Assert.That(chunks[0].BEntitesToLoad.Count, Is.EqualTo(5));
      Assert.That(chunks[0].Contexts.Count, Is.EqualTo(6));
      Assert.That(chunks[0].Contexts, Is.SubsetOf(entitySyncStateContexts));

      Assert.That(chunks[1].AEntitesToLoad.Count, Is.EqualTo(2));
      Assert.That(chunks[1].BEntitesToLoad.Count, Is.EqualTo(2));
      Assert.That(chunks[1].Contexts.Count, Is.EqualTo(6));
      Assert.That(chunks[1].Contexts, Is.SubsetOf(entitySyncStateContexts));
    }

    IEntitySyncStateContext<int, int, int, int, int, int, int> CreateContext(bool loadsAEntity, bool loadsBEntity)
    {
      var stub = MockRepository.GenerateStub<IEntitySyncStateContext<int, int, int, int, int, int, int>>();

      stub
        .Stub(_ => _.AddRequiredEntitiesToLoad(null, null))
        .IgnoreArguments()
        .WhenCalled(_ =>
        {
          if (loadsAEntity)
            ((Func<int, bool>) _.Arguments[0])(_nextId++);

          if (loadsBEntity)
            ((Func<int, bool>) _.Arguments[1])(_nextId++);
        });

      return stub;
    }
  }
}
