using CalDavSynchronizer.Ui.ConnectionTests;

namespace CalDavSynchronizer.Ui.Options.ResourceSelection.ViewModels
{
  static class AccessPrivilegesConverter
  {
    public static string ToString(AccessPrivileges accessPrivileges)
    {
      return (accessPrivileges & AccessPrivileges.All) == AccessPrivileges.All ? "rw" : "r";
    }
  }
}