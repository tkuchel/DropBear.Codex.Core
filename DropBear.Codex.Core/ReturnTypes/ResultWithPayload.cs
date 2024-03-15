using DropBear.Codex.Core.Models;
using MessagePack;

namespace DropBear.Codex.Core.ReturnTypes
{
    [MessagePackObject]
    public class ResultWithPayload<T> where T : notnull
    {
        [Key(0)] private readonly bool _isSuccess;
        [Key(1)] private readonly Payload<T>? _payload;
        [Key(2)] private readonly string _errorMessage;

        internal ResultWithPayload(bool isSuccess, Payload<T>? payload, string errorMessage)
        {
            _isSuccess = isSuccess;
            _payload = payload;
            _errorMessage = errorMessage;
        }

        public bool IsSuccess => _isSuccess;
        public Payload<T>? Payload => _payload;
        public string ErrorMessage => _errorMessage;

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
