using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace performancetests
{
    static class Extensions
    {
        private static class EnumStrings<T>
        {
            private static string[] _strings = null;
            private static Dictionary<int, string> _stringDictionary = null;
            private static bool IsStandardSequence(Array values)
            {
                List<Enum> valuesList = new List<Enum>();
                foreach (Enum value in values)
                {
                    valuesList.Add(value);
                }
                valuesList.Sort();
                for (int i = 0; i < valuesList.Count; i++)
                {
                    if (((IConvertible)valuesList[i]).ToInt32(CultureInfo.InvariantCulture) != i)
                    {
                        return false;
                    }
                }
                return true;
            }
            static EnumStrings()
            {
                if (typeof(T).IsEnum)
                {
                    if (IsStandardSequence(Enum.GetValues(typeof(T))))
                    {
                        _strings = new string[Enum.GetValues(typeof(T)).Length];
                        foreach (System.Enum value in Enum.GetValues(typeof(T)))
                        {
                            _strings[((IConvertible)value).ToInt32(CultureInfo.InvariantCulture)] = value.ToString();
                        }
                    }
                    else
                    {
                        _stringDictionary = new Dictionary<int, string>();
                        foreach (System.Enum value in Enum.GetValues(typeof(T)))
                        {
                            int valueAsInt = ((IConvertible)value).ToInt32(CultureInfo.InvariantCulture);
                            if (!_stringDictionary.ContainsKey(valueAsInt))
                                _stringDictionary.Add(valueAsInt, value.ToString());
                        }
                    }
                }
                else
                {
                    throw new Exception("Generic type must be an enumeration");
                }
            }
            public static string GetEnumString(int value)
            {
                string description;
                if (_strings != null)
                {
                    description = _strings[(int)value];
                }
                else
                {
                    _stringDictionary.TryGetValue(value, out description);
                }
                return description;
            }
        }
        public static string FastToString(this TimEnum color)
        {
            return EnumStrings<TimEnum>.GetEnumString((int)color);
        }

    }

    enum TimEnum
    {
        SomeEnum,
        SomeOtherEnum,
        SomeOtherValue
    }
    class Program
    {
        private static readonly Dictionary<TimEnum, string> timEnums = new Dictionary<TimEnum, string>()
        {
            { TimEnum.SomeEnum, "SomeEnum"} ,
            { TimEnum.SomeOtherEnum, "SomeOtherEnum"},
            { TimEnum.SomeOtherValue, "SomeOtherValue"}
        };

        static void Main(string[] args)
        {
            Console.WriteLine(MeasureMethod("ToStrng called : {0} : ", () => TimEnum.SomeEnum.ToString()));
            for (int i = 0; i < 1; i++)
            {
                Console.WriteLine(MeasureMethod("Int casting performed : {0}", () => { var x = (int)TimEnum.SomeEnum; }));
                Console.WriteLine(MeasureMethod("Fast ToString called : {0}", () => TimEnum.SomeEnum.FastToString()));
                Console.WriteLine(MeasureMethod("Dictionary lookup : {0}", () => { var x = timEnums[TimEnum.SomeEnum]; }));
            }

            Console.WriteLine();
            Console.Read();
        }

        static string MeasureMethod(string messageTemplate, Action act)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < Math.Pow(10, 7); i++)
            {
                act();
            }

            sw.Stop();
            return string.Format(messageTemplate, sw.Elapsed);
        }
    }
}
