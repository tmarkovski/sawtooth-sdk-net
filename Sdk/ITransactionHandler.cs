using System;
using System.Threading.Tasks;
using Sawtooh.Sdk.Processor;

namespace Sawtooh.Sdk
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
