using Logistic.DaysOfStayTracker.Core.Common;

namespace Logistic.DaysOfStayTracker.Core.Drivers;

public class Driver : Entity
{
    public string FirstName { get; internal set; } = string.Empty;
    public string LastName { get; internal set; } = string.Empty;
    public DateOnly? VisaExpiryDate { get; internal set; }

    public bool VisaExpiresSoon
    {
        get
        {
            if (VisaExpiryDate == null)
                return false;

            var withoutOneMonth = VisaExpiryDate.Value.AddMonths(-1);
            var today = DateOnly.FromDateTime(DateTime.Today);
            return withoutOneMonth <= today;
        }
    }
    
    internal Driver(){}
}