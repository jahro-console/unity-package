#if JAHRO_DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Jahro.Core.Commands;

namespace JahroConsole.Core.Internal
{
    public class ConsoleCommandsTestMethods : MonoBehaviour
    {
        public enum ExampleEnum
        {
            OptionOne,
            OptionTwo,
            OptionThree,
            OptionFour
        }

        /*
                Methods:
                    - empty method with return, description, group
                    - empty method without return
                    - empty method without group
                    - empty method without description
                    - method with exception
        */

        [JahroCommand("test-two", "AAA")]
        public static string ExampleFewMethod(string a, string b)
        {
            return a + b;
        }

        [JahroCommand("test-three", "AAA")]
        public static string ExampleThreeMethod(string a, string b, string c)
        {
            return a + b + c;
        }

        [JahroCommand("demo-method-long-name-add", "Example method that can be executed from the Console", "Method Examples")]
        public static void ExampleMethod()
        {

        }


        [JahroCommand("demo-return", "Demo method that can be executed from the Console and return some data to output", "Method Examples")]
        public static string DemoMethodReturnValue()
        {
            return "Current TimeScale: " + Time.timeScale;
        }



        [JahroCommand("demo-parameter", "Demo method that takes parameters", "Method Examples")]
        public static string DemoMethodWithParameter(float timeScale)
        {
            Time.timeScale = timeScale;
            return "TimeScale set to: " + Time.timeScale;
        }

        [JahroCommand("space com", "Demo method that takes parameters", "Method Examples")]
        public static string DemoMethodSpaced()
        {
            return "Demo method";
        }

        [JahroCommand("space dou com", "Demo method that takes parameters", "Method Examples")]
        public static string DemoDoubleMethodSpaced()
        {
            return "Demo method";
        }

        [JahroCommand("space val", "Demo method that takes parameters", "Method Examples")]
        public static string DemoMethodSpacedInt(int i)
        {
            return "Demo method " + i;
        }

        [JahroCommand("space param", "Demo method that takes parameters", "Method Examples")]
        public static string DemoMethodSpaced(string param)
        {
            return "Demo method " + param;
        }

        /*
         * Default values test
         * 
         * non-param + default param
         * float default param
         * bool default param
         * string defaulr param
         * array default param
         */

        [JahroCommand("def-value", "Example method that can be executed from the Console", "Method Examples")]
        public static string DefValuesSimple()
        {
            return "Simple DefValue without param";
        }

        [JahroCommand("def-value", "Example method that can be executed from the Console", "Method Examples")]
        public static string DefValuesSimple(float value = 5.55f)
        {
            return "Simple DefValue with param " + value;
        }


        /*
                Parameters:
                    - int
                    - bool
                    - float
                    - string
                    - Vector3
        */

        [JahroCommand("param-int", "Parameters Example", "Example command that takes integer as a parameter")]
        public static string ExampleParamInt(int intValue)
        {
            return "Int value: <" + intValue + ">";
        }

        [JahroCommand("param-bool", "Parameters Example", "Example command that takes boolean as a parameter")]
        public static string ExampleParamBool(bool boolValue)
        {
            return "Bool value: <" + boolValue + ">";
        }

        [JahroCommand("param-float", "Parameters Example", "Example command that takes float as a parameter")]
        public static string ExampleParamFloat(float floatValue)
        {
            return "Float value: <" + floatValue + ">";
        }

        [JahroCommand("param-double", "Parameters Example", "Example command that takes double as a parameter")]
        public static string ExampleParamDouble(double doubleValue)
        {
            return "Double value: <" + doubleValue + ">";
        }

        [JahroCommand("param-string", "Parameters Example", "Example command that takes string as a parameter")]
        public static string ExampleParamString(string stringValue)
        {
            return "String value: <" + stringValue + ">";
        }

        [JahroCommand("param-vector3", "Parameters Example", "Example command that takes Vector3 as a parameter")]
        public static string ExampleParamVector3(Vector3 vector3Value)
        {
            return "Vector value: <" + vector3Value.ToString() + ">";
        }

        [JahroCommand("param-vector2", "Parameters Example", "Example command that takes Vector2 as a parameter")]
        public static string ExampleParamVector2(Vector2 vector2Value)
        {
            return "Vector value: <" + vector2Value.ToString() + ">";
        }


        [JahroCommand("param-enum", "Parameters Example", "Example command that takes Enum as a parameter")]
        public static string ExampleParamEnum(ExampleEnum enumValue)
        {
            return "Enum value: <" + enumValue.ToString() + ">";
        }

        [JahroCommand("param-exception", "Parameters Example", "Example command that takes Enum as a parameter")]
        public static string ExampleParamException()
        {
            throw new NullReferenceException();
            //return "Enum value: <" + enumValue.ToString() + ">";
        }


        /*
                Overload:
                    - none
                    - int
                    - string
                    - float
                    - string
                    - Vector3
                    - int, int
                    - float, float
                    - string, float
                    - string, int 
        */

        [JahroCommand("overload-test", "", "Overload test")]
        public static string TestOverloadParam()
        {
            return "Overload with ";
        }

        [JahroCommand("overload-test", "", "Overload test")]
        public static string TestOverloadParam(int integerValue)
        {
            return "Overload with integer " + integerValue;
        }

        [JahroCommand("overload-test", "", "Overload test")]
        public static string TestOverloadParam(string stringValue)
        {
            return "Overload with string value " + stringValue;
        }
        /*
                [JahroCommand("overload-test", "", "Overload test")]
                public static string TestOverloadFloat(float floatValue)
                {
                    return "Overload with float " + floatValue;
                }

                [JahroCommand("overload-test", "", "Overload test")]
                public static string TestOverloadParam(Vector3 vector3Value)
                {
                    return "Overload with vector " + vector3Value.ToString();
                }

                [JahroCommand("overload-test", "", "Overload test")]
                public static string TestOverloadParam(int firstInt, int secondInt, int thirdInt)
                {
                    return "Overload with 3 Ints: " + firstInt + "; " + secondInt + "; " + thirdInt;
                }

                [JahroCommand("overload-test", "", "Overload test")]
                public static string TestOverloadParam(float firstFloat, float secondFloat)
                {
                    return "Overload with 2 floats: " + firstFloat + "; " + secondFloat;
                }

                [JahroCommand("overload-test", "", "Overload test")]
                public static string TestOverloadParam(string stringValue, float floatValue)
                {
                    return "Overload with string and float: " + stringValue + "; " + floatValue;
                }

                [JahroCommand("overload-test", "", "Overload test")]
                public static string TestOverloadParam(string stringValue, string intValue)
                {
                    return "Overload with string and int: " + stringValue + "; " + intValue;
                }
        */
        /*
                Arrays:
                - int
                - bool
                - float
                - string
                - Vector3
        */

        [JahroCommand("array-int", "Example of using integer array as a parameter", "Arrays Example")]
        public static string TestArrayInt(int[] arrayInt)
        {
            string output = "Array of ints: \n";
            foreach (var i in arrayInt)
            {
                output += i + "\n";
            }
            return output;
        }

        [JahroCommand("array-bool", "Example of using boolean array as a parameter", "Arrays Example")]
        public static string TestArrayBool(bool[] arrayBool)
        {
            string output = "Array of bools: \n";
            foreach (var i in arrayBool)
            {
                output += i + "\n";
            }
            return output;
        }

        [JahroCommand("array-float", "Example of using float array as a parameter", "Arrays Example")]
        public static string TestArrayFloat(float[] arrayFloat)
        {
            string output = "Array of floats: \n";
            foreach (var i in arrayFloat)
            {
                output += i + "\n";
            }
            return output;
        }

        [JahroCommand("array-string", "Example of using string array as a parameter", "Arrays Example")]
        public static string TestArrayString(string[] arrayString)
        {
            string output = "Array of strings: \n";
            foreach (var i in arrayString)
            {
                output += i + "\n";
            }
            return output;
        }


        /*
                Mixed Parameters:
                    - int, bool, float
                    - string, bool, string
                    - Vector3, int, Vector3
                    - Vector3, array int
                    - string array, string array
        */

        [JahroCommand("mixed-types", "", "Mix Types")]
        public static string ExampleMixedTypes(int i, float f, bool b)
        {
            return "Mixed values: " + i + " -- " + f + " -- " + b;
        }

        [JahroCommand("mixed-array", "", "Mix Types")]
        public static string ExampleMixesTypesWithArray(string stringValue, float[] floatArray)
        {
            string output = stringValue + "and array of floats: \n";
            foreach (var i in floatArray)
            {
                output += i + "\n";
            }
            return output;
        }

        [JahroCommand("params")]
        public static string TryThis()
        {
            return "Some result:";
        }


        // public static string TryThis()
        // public static string TryThis(int value)
        // public static string TryThis(float value)
        // public static string TryThis(double value)
        // public static string TryThis(bool value)
        // public static string TryThis(string value)
        // public static string TryThis(Vector2 value)
        // public static string TryThis(Vector3 value)
        // public static string TryThis(int[] array)
        // public static string TryThis(float[] array)
        // public static string TryThis(string[] array)
        // public static string TryThis(Vector2[] array)
        // public static string TryThis(Vector3[] array)
        // public static string TryThis(int valueA, int valueB)
        // public static string TryThis(float valueA, int[] array)
        // public static string TryThis(Enum value)

    }
}
#endif