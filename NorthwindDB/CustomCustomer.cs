namespace NorthwindDB;

public partial class Customer
{
    public override string ToString()
    {
        return $"{CompanyName}: {ContactName} Tel:{Phone}";
    }
}