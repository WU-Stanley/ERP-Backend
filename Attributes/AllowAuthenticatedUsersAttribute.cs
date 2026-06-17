namespace WUIAM.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AllowAuthenticatedUsersAttribute : Attribute
    {
    }
}
