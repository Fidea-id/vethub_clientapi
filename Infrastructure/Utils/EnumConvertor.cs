using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utils
{
    public static class EnumConvertor
    {
        public static string MethodTypeString(MethodType type)
        {
            var result = "undefined";
            if (type == MethodType.Create)
            {
                result = "Create";
            }
            else if(type == MethodType.Delete) 
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
