using System;

namespace DatingApp.Api.Extensions
{
    public static class DateTimeExtensions
    {
        public static int calculateAge(this DateTime dob )   {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            Console.WriteLine(age);
            //For checking the birthday has come on this year or not
            var year = today.AddYears(-age);
            Console.WriteLine(year);

            if(dob.Date > year){
                age--;
            }
            return age;

        }
    }
}