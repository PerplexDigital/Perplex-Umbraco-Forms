using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PerplexUmbraco.Forms.Code
{
    public static class Extensions
    {
        public static string Description<TEnum>(this TEnum enm) where TEnum : struct, IConvertible
        {
            var description = typeof(TEnum).GetMember(enm.ToString())[0].GetCustomAttribute<DescriptionAttribute>();
            if (description != null)
                return description.Description;
            else
                return enm.ToString();
        }
    }
}
