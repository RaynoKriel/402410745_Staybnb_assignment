namespace _402410745_Staybnb_assignment.Models;

// Small lists/constants used in more than one place.
public static class PropertyTypes
{
    public static readonly string[] All = { "Apartment", "House", "Cabin", "Villa" , "FarmHouse", "BeachHouse"};
}

public static class BookingRules
{
    // ServiceFee = 12% of (PricePerNight x Nights)
    public const decimal ServiceFeeRate = 0.12m;
}
