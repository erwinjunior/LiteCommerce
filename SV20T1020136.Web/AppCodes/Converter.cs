using System.Globalization;

namespace SV20T1020136.Web
{
    public static class Converter
    {
        /// <summary>
        /// Chuyển chuỗi s sang giá trị datetime theo các formats quy định
        /// trả về null nếu không thành công
        /// </summary>
        /// <param name="s"></param>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string s, string formats = "dd/MM/yyyy;dd-MM-yyyy;dd.MM.yyyy")
        {
            // this keyword: extension function for variable have type is string
            try
            {
                return DateTime.ParseExact(s, formats.Split(';'), CultureInfo.InvariantCulture);

            }
            catch
            {
                return null;
            }
        }
    }
}
