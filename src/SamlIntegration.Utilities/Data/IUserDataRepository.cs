namespace SamlIntegration.Utilities.Data
{
    public interface IUserDataRepository
    {
        /// <summary>
        /// Returns an e-mail of the user within current user context.
        /// </summary>
        string GetUserEmail();

        /// <summary>
        /// Returns some global unique id for the user that is useful
        /// in a context of integration with third-party vendor using SAML protocol.
        /// This means, that it can be any kinds of UUID, GUID, account numbers, or anything that makes sense.
        /// NOTE! This method is just an example, so you might have a very different way to get this user id.
        /// </summary>
        string GetUserGuid();
    }
}
