using Domain.Entities;

namespace Infrastructure.Utils
{
    public static class EnumConvertor
    {
        public static string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                1 => "Sunday",
                2 => "Monday",
                3 => "Tuesday",
                4 => "Wednesday",
                5 => "Thursday",
                6 => "Friday",
                7 => "Saturday",
                _ => "Unknown"
            };
        }
        public static string MethodTypeString(MethodType type)
        {
            var result = "undefined";
            if (type == MethodType.Create)
            {
                result = "Create";
            }
            else if (type == MethodType.Delete)
            {
                result = "Delete";
            }
            else
            {
                result = "Update";
            }
            return result;
        }
    }
}
