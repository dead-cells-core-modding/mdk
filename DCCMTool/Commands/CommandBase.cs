using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCMTool.Commands
{
    internal interface ICommandBase
    {
        abstract static Type GetArgType();
        void SetArguments(object args);
        void Execute();
    }
    internal abstract class CommandBase<TArg> : ICommandBase
    {
        protected TArg Arguments { get; private set; } = default!;
        public static Type GetArgType()
        {
            return typeof(TArg);
        }

        public abstract void Execute();

        public void SetArguments(object args)
        {
            Arguments = (TArg)args;
        }
    }
}
