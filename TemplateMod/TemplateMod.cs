using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace TemplateMod
{
    public class TemplateMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Hello from Template Mod!");
        }
    }
}
