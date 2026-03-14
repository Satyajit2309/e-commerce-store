namespace EcommerceStore.Constants;

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Pending, Paid, Shipped, Delivered, Cancelled];

    public static bool IsValidTransition(string current, string next)
    {
        return (current, next) switch
        {
            (Pending, Paid) => true,
            (Pending, Cancelled) => true,
            (Paid, Shipped) => true,
            (Paid, Cancelled) => true,
            (Shipped, Delivered) => true,
            _ => false
        };
    }
}
