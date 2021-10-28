using RDS.TextParser.Tokens;
using System;

namespace RDS.TextParser
{
    public static class Constants
    {
        public static readonly Type t_string = typeof(string);
        public static readonly Type t_bool = typeof(bool);
        public static readonly Type t_byte = typeof(byte);
        public static readonly Type t_sbyte = typeof(sbyte);
        public static readonly Type t_char = typeof(char);
        public static readonly Type t_decimal = typeof(decimal);
        public static readonly Type t_double = typeof(double);
        public static readonly Type t_float = typeof(float);
        public static readonly Type t_int = typeof(int);
        public static readonly Type t_uint = typeof(uint);
        public static readonly Type t_long = typeof(long);
        public static readonly Type t_ulong = typeof(ulong);
        public static readonly Type t_object = typeof(object);
        public static readonly Type t_short = typeof(short);
        public static readonly Type t_ushort = typeof(ushort);
        public static readonly Type t_DateTime = typeof(DateTime);

        public static readonly RuntimeTypeHandle handle_Token = typeof(Token).TypeHandle;
        public static readonly RuntimeTypeHandle handle_ConditionalToken = typeof(ConditionalToken).TypeHandle;
        public static readonly RuntimeTypeHandle handle_JSONToken = typeof(JSONToken).TypeHandle;
        public static readonly RuntimeTypeHandle handle_TypeToken = typeof(Type_Token).TypeHandle;
        public static readonly RuntimeTypeHandle handle_ListToken = typeof(ListToken).TypeHandle;
        public static readonly RuntimeTypeHandle handle_TypeListToken = typeof(TypeListToken).TypeHandle;
        public static readonly RuntimeTypeHandle handle_TableToken = typeof(TableToken).TypeHandle;

        public static readonly char c_tab = char.Parse("\t");
        public static readonly char c_newln = char.Parse("\n");
        public static readonly char c_return = char.Parse("\r");

        public static readonly char c_lt = char.Parse("<");
        public static readonly char c_gt = char.Parse(">");
        public static readonly char c_space = char.Parse(" ");
        public static readonly char c_comma = char.Parse(",");
        public static readonly char c_fwdSlash = char.Parse("/");

        public static char[] _removals = new char[] {
            c_newln,
            c_return,
            c_tab 
        };

        public static readonly Type[] BuiltInTypes = new Type[] { 
            t_string, 
            t_bool, 
            t_byte, 
            t_byte, 
            t_sbyte, 
            t_char, 
            t_decimal, 
            t_double, 
            t_float, 
            t_int, 
            t_uint, 
            t_ulong, 
            t_object, 
            t_short, 
            t_ushort, 
            t_DateTime 
        };
    }
}
