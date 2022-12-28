using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDE.FAIRS.Domain
{
    public static class MethodExtensions
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string dataSource) where T : struct
        {
            T retVal = default(T);
            Enum.TryParse<T>(dataSource, true, out retVal);
            return retVal;
        }

        public static int LastDayOfMonth(this DateTime dateTime)
        {
            if(dateTime.Month == 1 || dateTime.Month == 3 || dateTime.Month == 5 || dateTime.Month == 7
                || dateTime.Month == 8 || dateTime.Month == 10 || dateTime.Month == 12)
            {
                return 31;
            }
            else if (dateTime.Month == 4 || dateTime.Month == 6 || dateTime.Month == 9 || dateTime.Month == 11)
            {
                return 30;
            }
            else
            {
                if (DateTime.IsLeapYear(dateTime.Year))
                    return 29;
                else
                    return 28;
            }
        }
    }

   
}
