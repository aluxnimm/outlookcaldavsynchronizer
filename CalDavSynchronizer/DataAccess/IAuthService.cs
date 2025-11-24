using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.DataAccess
{
    public interface IAuthService
    {
        string GetAccessToken();
    }
}
