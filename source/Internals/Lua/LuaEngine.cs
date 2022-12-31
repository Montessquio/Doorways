using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Doorways.Internals.Lua
{
    internal static class LuaEngine
    {
        public struct LuaScript
        {
            Script script;
            Span _span; // Stored in the script to handle log messages.

            public LuaScript(Script script, Span _span)
            {
                this.script = script;
                this._span = _span;
            }

            public DynValue Execute(string fn)
            {
                DynValue res;
                try
                {
                    res = script.Call(script.Globals[fn]);
                }
                catch(KeyNotFoundException)
                {
                    throw new KeyNotFoundException($"Function '{fn}' was not found in script ");
                }
                return res;
            }
        }

        public static LuaScript Instantiate(string sourceFilePath, DoorwaysMod sourceMod)
        {
            string sourceFileName = Path.GetFileName(sourceFilePath);
            Span _span = Logger.Instance.Span(sourceFileName, sourceMod.ModName);

            Script s = new Script(CoreModules.Preset_HardSandbox | CoreModules.Metatables);
            SetGlobals(ref s, _span, sourceMod);

            if(!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException($"Could not find a script at '{sourceFilePath}'");
            }
            try
            {
                s.DoString(File.ReadAllText(sourceFilePath));
            }
            catch(InterpreterException me)
            {
                throw new InvalidDataException($"The script at '{sourceFilePath}' caused MoonSharp to throw an Exception", me);
            }
            catch(Exception e)
            {
                throw new Exception($"The script at '{sourceFilePath}' threw an Exception", e);
            }

            return new LuaScript(s, _span);
        }

        private static void SetGlobals(ref Script s, Span _span, DoorwaysMod mod)
        {
            s.Options.DebugPrint = str => { _span.Debug(str); };

            s.Globals["id"] = (Func<string, string>)mod.CanonicalizeId;
            s.Globals["instantiate"] = (Func<string, LuaCard>)LuaCard.Instantiate;
        }
    }
}
