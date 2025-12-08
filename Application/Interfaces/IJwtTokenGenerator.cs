using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        public string GenerateToken(IdentityUser user, IList<string> roles);
    }
}
