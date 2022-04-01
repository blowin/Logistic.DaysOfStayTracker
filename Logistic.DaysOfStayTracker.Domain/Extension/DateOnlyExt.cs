namespace Logistic.DaysOfStayTracker.Core.Extension;

public static class DateOnlyExt
{
    public static DateTime AsDateTime(this DateOnly self) => self.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero));

    public static TimeSpan Subtract(this DateOnly self, DateOnly right)
    {
        var l = self.AsDateTime();
        var r = right.AsDateTime();
        return l - r;
    }

    public static DateOnly Min(this DateOnly self, DateOnly right) => self > right ? right : self;
    
    public static DateOnly Max(this DateOnly self, DateOnly right) => self > right ? self : right;
}