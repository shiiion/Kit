using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace Kit.Core
{
    class CSCodeCompiler
    {
        private static Assembly buildCode(string code)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters param = new CompilerParameters();
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;
            CompilerResults results = codeProvider.CompileAssemblyFromSource(param, code);
            if(results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compile failed :\n");
                foreach(CompilerError error in results.Errors)
                {
                    errors.AppendLine($"Line {error.Line},{error.Column}: {error.ErrorText}");
                }
                throw new Exceptions.CompilerErrorException(errors.ToString());
            }
            else
            {
                return results.CompiledAssembly;
            }
        }

        public static object ExecuteCode(string code, string namespaceName, string className, string functionName, bool isStatic, params object[] args)
        {
            object ret = null;

            Assembly asm;
            try
            {
                asm = buildCode(code);
            }
            catch(Exceptions.CompilerErrorException e)
            {
                throw e;
            }
            object instance = null;
            Type type = null;
            if(isStatic)
            {
                type = asm.GetType(namespaceName + "." + className);
            }
            else
            {
                instance = asm.CreateInstance(namespaceName + "." + className);
                type = instance.GetType();
            }

            MethodInfo method = type.GetMethod(functionName);
            ret = method.Invoke(instance, args);
            return ret;
        }
    }
}
