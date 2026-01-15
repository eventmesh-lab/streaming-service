using System;

namespace streaming_service.Domain.ValueObjects
{
    public record StreamingUrl
    {
        public string Value { get; init; }
        public bool IsEncrypted { get; init; }

        public StreamingUrl(string value, bool isEncrypted = false)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("URL cannot be empty", nameof(value));

            Value = value;
            IsEncrypted = isEncrypted;
        }

        public static StreamingUrl Create(string url) => new(url);
        
        // Logical placeholder for domain-driven encryption intent
        public StreamingUrl Encrypt(Func<string, string> encryptor) 
            => IsEncrypted ? this : new(encryptor(Value), true);

        public string Decrypt(Func<string, string> decryptor)
            => IsEncrypted ? decryptor(Value) : Value;
    }
}
