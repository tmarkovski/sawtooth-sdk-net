using System;
using System.Threading.Tasks;
using Sawtooth.Sdk.Processor;

namespace Sawtooth.Sdk.Processor
{
    /// <summary>
    /// Transaction handler.
    /// </summary>
    public interface ITransactionHandler
    {
        /// <summary>
        /// Gets the name of the family.
        /// </summary>
        /// <value>The name of the family.</value>
        string FamilyName { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        string Version { get; }

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        /// <value>The namespaces.</value>
        string[] Namespaces { get; }

        /// <summary>
        /// Called when the processor recieves <see cref="TpProcessRequest" /> message/>
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="request">Request.</param>
        /// <param name="context">Context.</param>
        Task ApplyAsync(TpProcessRequest request, TransactionContext context);
    }
}
