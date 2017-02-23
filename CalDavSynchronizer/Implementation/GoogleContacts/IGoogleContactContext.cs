namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public interface IGoogleContactContext
  {
    IGoogleGroupCache GroupCache { get; }
    IGoogleContactCache ContactCache { get; }
  }
}