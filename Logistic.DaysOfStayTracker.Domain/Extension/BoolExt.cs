namespace Logistic.DaysOfStayTracker.Core.Extension;

public static class BoolExt
{
    public static string ToUserFriendlyString(this bool self) => self ? "Да" : "Нет";
}