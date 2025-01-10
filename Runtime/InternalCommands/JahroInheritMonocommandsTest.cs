#if JAHRO_DEBUG
using System.Collections;
using System.Collections.Generic;
using Jahro.Core.Commands;
using UnityEngine;

namespace Jahro.Core.Internal
{
    public class JahroInheritMonocommandsTest : JahroMonocommandsTest
    {
        [JahroCommand("mono-i-simple", "", "mono")]
        public void MonoInhSimple()
        {

        }

        [JahroCommand("mono-i-return", "", "mono")]
        public string MonoInhSimpleReturn()
        {
            return "Mono executed simple" + " by " + this.name;
        }

        [JahroCommand("mono-i-simple-float", "", "mono")]
        public string MonoInhParam(float fv)
        {
            return "Mono " + fv + " by " + this.name;
        }

        [JahroCommand("mono-i-simple-str", "", "mono")]
        public string MonoInhParamString(string str)
        {
            return "Mono executed " + str + " by " + this.name;
        }
    }
}
#endif