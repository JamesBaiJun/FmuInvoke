using System;
using System.Collections.Generic;
using System.Text;

namespace Femyou.Common
{
    public class CommonTool
    {
        /// <summary>
        /// 获取小数位数
        /// </summary>
        /// <param name="decimalV">小数</param>
        /// <returns></returns>
        public static int GetNumberOfDecimalPlaces(double decimalV)
        {
            int result = 0;
            string[] temp = decimalV.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 2 && temp[1].Length > 0 && !temp[1].ToUpper().Contains("E"))
            {
                int index = temp[1].Length - 1;
                while (temp[1][index] == '0' && index-- > 0) ;
                result = index + 1;
            }
            else
            {
                temp = decimalV.ToString().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length == 2 && temp[1].Length > 0)
                {
                    result = int.Parse(temp[1]);
                }
                string s = temp[0].Substring(0, temp[0].Length - 1);
                temp = s.Split('.');
                if (temp.Length == 2 && temp[1].Length > 0)
                {
                    int index = temp[1].Length - 1;
                    while (temp[1][index] == '0' && index-- > 0) ;
                    result = result + index + 1;
                }
            }
            return result;
        }
    }
}
