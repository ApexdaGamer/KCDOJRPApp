using System;
using System.Collections.Generic;
using CitizenFX.Core;
using static CitizenFX.Server.Native.Natives; // need this apparently. not CitizenFX.Shared.Native.Natives.
                                              // i guess that makes sense.
namespace KCDOJRPApp.Server
{
    public class Class1 : BaseScript
    {
        public Class1()
        {
            Debug.WriteLine("hi, this is the server thing");
        }
    }
}