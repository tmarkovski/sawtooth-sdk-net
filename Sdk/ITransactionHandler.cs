using System;
using System.Threading.Tasks;
using Sawtooth.Sdk.Processor;

namespace Sawtooth.Sdk
{
    public interface ITransactionHandler
    {
        string FamilyName
        {
            get;
            set;
        }

        string Version
        {
            get;
            set;
        }

        string[] Namespaces
        {
            get;
            set;
        }

        Task Apply(TpProcessRequest request, Context context);
    }
}
