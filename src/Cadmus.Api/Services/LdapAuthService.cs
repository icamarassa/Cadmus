using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;

namespace Cadmus.Api.Services
{
    [SupportedOSPlatform("windows")]
    public class LdapAuthService
{
    private readonly string _domain;

    public LdapAuthService(IConfiguration config)
    {
        _domain = config["Ldap:Domain"]!; // ex: "SEUDOMINIO"
    }

    public bool ValidateCredentials(string username, string password, out string[] groups)
    {
        groups = Array.Empty<string>();

        using var context = new PrincipalContext(ContextType.Domain, _domain);
        bool isValid = context.ValidateCredentials(username, password);

        if (isValid)
        {
            using var user = UserPrincipal.FindByIdentity(context, username);
            groups = user?.GetAuthorizationGroups()
                          .Select(g => g.Name)
                          .Where(n => n != null)
                          .ToArray()! ?? Array.Empty<string>();
        }

        return isValid;
    }
}
}