#if JAHRO_DEBUG
using System.Collections;
using System.Collections.Generic;
using Jahro.Core.Commands;
using UnityEngine;

namespace Jahro.Core.Internal
{
    public class JahroMonocommandsTest : MonoBehaviour
    {
        [JahroCommand("mono-simple", "", "mono")]
        public void MonoSimple()
        {

        }

        [JahroCommand("mono-simple-return", "", "mono")]
        public string MonoSimpleReturn()
        {
            return "Mono executed simple" + " by " + this.name;
        }

        [JahroCommand("mono-param", "", "mono")]
        public string MonoParam(float fv)
        {
            return "Mono " + fv + " by " + this.name;
        }

        [JahroCommand("mono-param-str", "", "mono")]
        public string MonoParamString(string str)
        {
            return "Mono executed " + str + " by " + this.name;
        }
    }

    
}
#endif