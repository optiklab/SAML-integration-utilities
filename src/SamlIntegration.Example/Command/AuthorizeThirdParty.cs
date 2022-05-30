namespace SamlIntegration.Example.Command
{
    public class AuthorizeThirdParty : IAuthenticatedCommand
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}
