using DropBear.Codex.Core.Models;
using MessagePack;
using System;

namespace DropBear.Codex.Core.ReturnTypes
{
    /// <summary>
    /// Represents the result of an operation that may contain a payload of type <typeparamref name="T"/>.
    /// </summary>
    [MessagePackObject]
    public class ResultWithPayload<T> where T : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultWithPayload{T}"/> class.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the operation was successful.</param>
        /// <param name="payload">The payload of the operation if successful.</param>
        /// <param name="errorMessage">The error message if the operation was not successful.</param>
        internal ResultWithPayload(bool isSuccess, Payload<T>? payload, string errorMessage)
        {
            IsSuccess = isSuccess;
            Payload = payload;
            ErrorMessage = isSuccess ? string.Empty : errorMessage;
        }

        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        [Key(0)]
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the payload of the operation if successful.
        /// </summary>
        [Key(1)]
        public Payload<T>? Payload { get; }

        /// <summary>
        /// Gets the error message if the operation was not successful.
        /// </summary>
        [Key(2)]
        public string ErrorMessage { get; }

        /// <summary>
        /// Validates the integrity of the payload if available.
        /// </summary>
        /// <returns>True if the payload's integrity is validated; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the payload is not available for integrity validation.</exception>
        public bool ValidateIntegrity()
        {
            if (!IsSuccess || Payload is null)
            {
                throw new InvalidOperationException("Payload is not available for integrity validation.");
            }

            return Payload.ValidateIntegrity();
        }
    }
}
