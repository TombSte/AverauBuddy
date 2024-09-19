namespace AverauBuddy.Example.CustomExceptions;

public class ForbiddenException(string scope, string missingGrant) : Exception
{
    public string Scope {
        get;
        set;
    } = scope;

    public string MissingGrant {
        get;
        set;
    } = missingGrant;
}